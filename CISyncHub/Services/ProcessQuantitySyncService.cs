using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CISyncHub.Models;

namespace CISyncHub.Services;

/// <summary>
/// Service responsible for bulk synchronizing machine-level process quantity (QtyIn & QtyOut)
/// from division databases (MLS, OPTIC, AIS: dmPChart) into CI_LSS.dbo.ProcessQuantity.
/// </summary>
public class ProcessQuantitySyncService
{
    private readonly string _destConnStr;
    private readonly Action<string> _logger;

    public ProcessQuantitySyncService(string destConnStr, Action<string> logger)
    {
        _destConnStr = destConnStr;
        _logger = logger;
    }

    /// <summary>
    /// Ensures target table [dbo].[ProcessQuantity] exists and contains required columns and indexes in CI_LSS.
    /// </summary>
    public async Task EnsureDestinationTableExistsAsync(CancellationToken cancellationToken = default)
    {
        _logger("Checking destination table [dbo].[ProcessQuantity] in CI_LSS...");
        const string ddl = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProcessQuantity' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[ProcessQuantity] (
        [Id]               BIGINT IDENTITY(1,1) NOT NULL,
        [MachCode]         NVARCHAR(100)        NOT NULL,
        [ProdDate]         DATE                 NOT NULL,
        [QuantityIn]       FLOAT                NOT NULL DEFAULT (0),
        [QuantityOut]      FLOAT                NOT NULL DEFAULT (0),
        [DeviceCode]       NVARCHAR(100)        NULL DEFAULT (''),
        [ProcCode]         NVARCHAR(50)         NULL DEFAULT (''),
        [Division]         NVARCHAR(50)         NULL,
        [SourceServer]     NVARCHAR(100)        NULL,
        [SourceDatabase]   NVARCHAR(100)        NULL,
        [LastSyncDate]     DATETIME             NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_ProcessQuantity] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ProcessQuantity') AND name = 'QuantityOut')
        ALTER TABLE [dbo].[ProcessQuantity] ADD [QuantityOut] FLOAT NOT NULL DEFAULT (0);

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ProcessQuantity') AND name = 'Division')
        ALTER TABLE [dbo].[ProcessQuantity] ADD [Division] NVARCHAR(50) NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ProcessQuantity') AND name = 'SourceServer')
        ALTER TABLE [dbo].[ProcessQuantity] ADD [SourceServer] NVARCHAR(100) NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ProcessQuantity') AND name = 'SourceDatabase')
        ALTER TABLE [dbo].[ProcessQuantity] ADD [SourceDatabase] NVARCHAR(100) NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ProcessQuantity') AND name = 'LastSyncDate')
        ALTER TABLE [dbo].[ProcessQuantity] ADD [LastSyncDate] DATETIME NOT NULL DEFAULT (GETDATE());

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ProcessQuantity') AND name = 'DeviceCode')
        ALTER TABLE [dbo].[ProcessQuantity] ADD [DeviceCode] NVARCHAR(100) NULL DEFAULT ('');

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ProcessQuantity') AND name = 'ProcCode')
        ALTER TABLE [dbo].[ProcessQuantity] ADD [ProcCode] NVARCHAR(50) NULL DEFAULT ('');
END

-- Drop legacy unique index if exists
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_ProcessQuantity_Mach_Date_Dev_Proc_Div' AND object_id = OBJECT_ID('dbo.ProcessQuantity'))
BEGIN
    DROP INDEX [UQ_ProcessQuantity_Mach_Date_Dev_Proc_Div] ON [dbo].[ProcessQuantity];
END;

-- Deduplicate any existing records to ensure unique (MachCode, ProdDate, Division)
;WITH Aggregated AS (
    SELECT 
        MachCode, 
        ProdDate, 
        ISNULL(Division, '') AS DivKey,
        MIN(Id) AS KeepId,
        SUM(QuantityIn) AS TotalIn,
        SUM(QuantityOut) AS TotalOut
    FROM [dbo].[ProcessQuantity]
    GROUP BY MachCode, ProdDate, ISNULL(Division, '')
    HAVING COUNT(*) > 1
)
UPDATE p
SET p.QuantityIn = a.TotalIn,
    p.QuantityOut = a.TotalOut
FROM [dbo].[ProcessQuantity] p
INNER JOIN Aggregated a ON p.Id = a.KeepId;

;WITH DuplicatesToDelete AS (
    SELECT 
        p.Id,
        ROW_NUMBER() OVER (PARTITION BY p.MachCode, p.ProdDate, ISNULL(p.Division, '') ORDER BY p.Id ASC) AS rn
    FROM [dbo].[ProcessQuantity] p
)
DELETE FROM DuplicatesToDelete WHERE rn > 1;

-- Unique index ensuring exactly 1 daily total row per machine per division
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_ProcessQuantity_Mach_Date_Div' AND object_id = OBJECT_ID('dbo.ProcessQuantity'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_ProcessQuantity_Mach_Date_Div] 
        ON [dbo].[ProcessQuantity] ([MachCode] ASC, [ProdDate] ASC, [Division] ASC);
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProcessQuantity_ProdDate_MachCode' AND object_id = OBJECT_ID('dbo.ProcessQuantity'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProcessQuantity_ProdDate_MachCode] 
        ON [dbo].[ProcessQuantity] ([ProdDate] ASC, [MachCode] ASC)
        INCLUDE ([QuantityIn], [QuantityOut]);
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProcessQuantity_MachCode' AND object_id = OBJECT_ID('dbo.ProcessQuantity'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProcessQuantity_MachCode] 
        ON [dbo].[ProcessQuantity] ([MachCode] ASC);
END;
";

        using var conn = new SqlConnection(_destConnStr);
        await conn.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand(ddl, conn);
        cmd.CommandTimeout = 120;
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        _logger("✅ Table [dbo].[ProcessQuantity] schema is verified and ready in CI_LSS.");
    }

    /// <summary>
    /// Synchronizes machine-level QtyIn & QtyOut from dmPChart of a division source.
    /// </summary>
    public async Task<SyncResult> SyncSourceAsync(
        SyncSourceConfig source,
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var result = new SyncResult { TaskName = "Process Quantity Sync", Division = source.Division };

        _logger($"------------------------------------------------------------");
        _logger($"[START] Syncing Machine Process Quantity (dmPChart): {source.Division} ({source.Name})...");
        _logger($"Source: {source.ServerName} -> DB: {source.DatabaseName}");

        try
        {
            var dt = await ExtractProcessQuantityDataAsync(source, startDate, endDate, cancellationToken);
            result.RecordsExtracted = dt.Rows.Count;
            _logger($"Extracted {result.RecordsExtracted:N0} machine-level production records from {source.Division}.");

            if (result.RecordsExtracted == 0)
            {
                _logger($"ℹ️ No records found for division {source.Division} in the selected date window.");
                result.Success = true;
                sw.Stop();
                result.Duration = sw.Elapsed;
                return result;
            }

            result.RecordsUpserted = await UpsertToDestinationAsync(dt, cancellationToken);
            _logger($"✅ Successfully upserted {result.RecordsUpserted:N0} records into [dbo].[ProcessQuantity].");
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger($"❌ Error syncing Process Quantity {source.Division}: {ex.Message}");
        }
        finally
        {
            sw.Stop();
            result.Duration = sw.Elapsed;
            _logger($"[END] Division: {source.Division} Process Quantity sync finished in {sw.Elapsed.TotalSeconds:F2}s.");
        }

        return result;
    }

    private async Task<DataTable> ExtractProcessQuantityDataAsync(
        SyncSourceConfig source,
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken)
    {
        var dt = new DataTable();
        dt.Columns.Add("MachCode", typeof(string));
        dt.Columns.Add("ProdDate", typeof(DateTime));
        dt.Columns.Add("QuantityIn", typeof(double));
        dt.Columns.Add("QuantityOut", typeof(double));
        dt.Columns.Add("Division", typeof(string));
        dt.Columns.Add("SourceServer", typeof(string));
        dt.Columns.Add("SourceDatabase", typeof(string));

        string dateFilter = string.Empty;
        if (startDate.HasValue && endDate.HasValue)
        {
            dateFilter = "AND COALESCE(p.DateFinishActual, p.DateFinish, p.DateStart) >= @StartDate AND COALESCE(p.DateFinishActual, p.DateFinish, p.DateStart) < @EndDate";
        }
        else if (startDate.HasValue)
        {
            dateFilter = "AND COALESCE(p.DateFinishActual, p.DateFinish, p.DateStart) >= @StartDate";
        }

        // Akumulasi total QtyIn & QtyOut per mesin per hari (1 row per machine per calendar day)
        string sql = $@"
SELECT 
    LTRIM(RTRIM(p.MachCode)) AS [MachCode],
    CAST(COALESCE(p.DateFinishActual, p.DateFinish, p.DateStart) AS DATE) AS [ProdDate],
    SUM(CAST(ISNULL(p.QuantityIn, 0) AS FLOAT)) AS [QuantityIn],
    SUM(CAST(ISNULL(p.QuantityOut, 0) AS FLOAT)) AS [QuantityOut]
FROM dmPChart p WITH (NOLOCK)
WHERE p.MachCode IS NOT NULL 
  AND LTRIM(RTRIM(p.MachCode)) <> ''
  AND COALESCE(p.DateFinishActual, p.DateFinish, p.DateStart) IS NOT NULL
  {dateFilter}
GROUP BY 
    LTRIM(RTRIM(p.MachCode)),
    CAST(COALESCE(p.DateFinishActual, p.DateFinish, p.DateStart) AS DATE)
ORDER BY 
    [ProdDate] DESC, [MachCode] ASC;";

        using var conn = new SqlConnection(source.ConnectionString);
        await conn.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand(sql, conn);
        cmd.CommandTimeout = 300;
        if (startDate.HasValue)
            cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = startDate.Value.Date });
        if (endDate.HasValue)
            cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = endDate.Value.Date.AddDays(1) });

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            string machCode = reader.GetString(0);
            DateTime prodDate = reader.GetDateTime(1);
            double qtyIn = reader.IsDBNull(2) ? 0 : Convert.ToDouble(reader.GetValue(2));
            double qtyOut = reader.IsDBNull(3) ? 0 : Convert.ToDouble(reader.GetValue(3));

            dt.Rows.Add(
                machCode,
                prodDate,
                qtyIn,
                qtyOut,
                source.Division,
                source.ServerName,
                source.DatabaseName
            );
        }

        return dt;
    }

    private async Task<int> UpsertToDestinationAsync(DataTable dt, CancellationToken cancellationToken)
    {
        using var conn = new SqlConnection(_destConnStr);
        await conn.OpenAsync(cancellationToken);

        // 1. Create Staging Table in destination CI_LSS (Daily Machine Level)
        const string createStageSql = @"
CREATE TABLE #Stage_ProcessQuantity (
    [MachCode]       NVARCHAR(100) NOT NULL,
    [ProdDate]       DATE          NOT NULL,
    [QuantityIn]     FLOAT         NOT NULL,
    [QuantityOut]    FLOAT         NOT NULL,
    [Division]       NVARCHAR(50)  NULL,
    [SourceServer]   NVARCHAR(100) NULL,
    [SourceDatabase] NVARCHAR(100) NULL
);";

        using (var cmdStage = new SqlCommand(createStageSql, conn))
        {
            await cmdStage.ExecuteNonQueryAsync(cancellationToken);
        }

        // 2. SqlBulkCopy into Staging Table
        using (var bulkCopy = new SqlBulkCopy(conn))
        {
            bulkCopy.DestinationTableName = "#Stage_ProcessQuantity";
            bulkCopy.BulkCopyTimeout = 300;
            bulkCopy.BatchSize = 5000;

            bulkCopy.ColumnMappings.Add("MachCode", "MachCode");
            bulkCopy.ColumnMappings.Add("ProdDate", "ProdDate");
            bulkCopy.ColumnMappings.Add("QuantityIn", "QuantityIn");
            bulkCopy.ColumnMappings.Add("QuantityOut", "QuantityOut");
            bulkCopy.ColumnMappings.Add("Division", "Division");
            bulkCopy.ColumnMappings.Add("SourceServer", "SourceServer");
            bulkCopy.ColumnMappings.Add("SourceDatabase", "SourceDatabase");

            await bulkCopy.WriteToServerAsync(dt, cancellationToken);
        }

        // 3. MERGE Staging Table into dbo.ProcessQuantity (Unique per MachCode, ProdDate, Division)
        const string mergeSql = @"
MERGE [dbo].[ProcessQuantity] AS target
USING #Stage_ProcessQuantity AS source
ON (target.[MachCode] = source.[MachCode]
    AND target.[ProdDate] = source.[ProdDate]
    AND ISNULL(target.[Division], '') = ISNULL(source.[Division], ''))
WHEN MATCHED THEN
    UPDATE SET
        target.[QuantityIn]     = source.[QuantityIn],
        target.[QuantityOut]    = source.[QuantityOut],
        target.[SourceServer]   = source.[SourceServer],
        target.[SourceDatabase] = source.[SourceDatabase],
        target.[LastSyncDate]   = GETDATE()
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([MachCode], [ProdDate], [QuantityIn], [QuantityOut], [Division], [SourceServer], [SourceDatabase], [LastSyncDate])
    VALUES (source.[MachCode], source.[ProdDate], source.[QuantityIn], source.[QuantityOut], source.[Division], source.[SourceServer], source.[SourceDatabase], GETDATE());

DROP TABLE #Stage_ProcessQuantity;
";

        using (var cmdMerge = new SqlCommand(mergeSql, conn))
        {
            cmdMerge.CommandTimeout = 600;
            int affected = await cmdMerge.ExecuteNonQueryAsync(cancellationToken);
            return affected;
        }
    }
}

