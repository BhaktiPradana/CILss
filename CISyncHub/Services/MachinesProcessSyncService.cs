using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CISyncHub.Models;

namespace CISyncHub.Services;

public class MachinesProcessSyncService
{
    private readonly string _destConnStr;
    private readonly Action<string> _logger;

    public MachinesProcessSyncService(string destConnStr, Action<string> logger)
    {
        _destConnStr = destConnStr;
        _logger = logger;
    }

    /// <summary>
    /// Ensures destination table [dbo].[CI_MachinesProcess] exists in CI_LSS.
    /// </summary>
    public async Task EnsureDestinationTableExistsAsync(CancellationToken cancellationToken = default)
    {
        _logger("Checking destination table [dbo].[CI_MachinesProcess] in CI_LSS...");
        const string ddl = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CI_MachinesProcess' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[CI_MachinesProcess] (
        [MachCode]         NVARCHAR(50)   NOT NULL,
        [ProcCode]         NVARCHAR(50)   NOT NULL,
        [CurrentCount]     INT            NULL,
        [MaxCount]         INT            NULL,
        [Division]         NVARCHAR(50)   NULL,
        [LastSyncDate]     DATETIME       NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_CI_MachinesProcess] PRIMARY KEY CLUSTERED ([MachCode] ASC, [ProcCode] ASC)
    );
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CI_MachinesProcess') AND name = 'Division')
        ALTER TABLE [dbo].[CI_MachinesProcess] ADD [Division] NVARCHAR(50) NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CI_MachinesProcess') AND name = 'CurrentCount')
        ALTER TABLE [dbo].[CI_MachinesProcess] ADD [CurrentCount] INT NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CI_MachinesProcess') AND name = 'MaxCount')
        ALTER TABLE [dbo].[CI_MachinesProcess] ADD [MaxCount] INT NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CI_MachinesProcess') AND name = 'LastSyncDate')
        ALTER TABLE [dbo].[CI_MachinesProcess] ADD [LastSyncDate] DATETIME NOT NULL DEFAULT (GETDATE());
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CI_MachinesProcess_ProcCode' AND object_id = OBJECT_ID('dbo.CI_MachinesProcess'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_CI_MachinesProcess_ProcCode] ON [dbo].[CI_MachinesProcess] ([ProcCode] ASC);
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CI_MachinesProcess_MachCode' AND object_id = OBJECT_ID('dbo.CI_MachinesProcess'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_CI_MachinesProcess_MachCode] ON [dbo].[CI_MachinesProcess] ([MachCode] ASC);
END;
";

        using var conn = new SqlConnection(_destConnStr);
        await conn.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand(ddl, conn);
        cmd.CommandTimeout = 120;
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        _logger("✅ Table [dbo].[CI_MachinesProcess] schema is verified and ready.");
    }

    /// <summary>
    /// Synchronizes dmMachinesProcess from a division source (MLS, OPTIC, or AIS).
    /// </summary>
    public async Task<SyncResult> SyncSourceAsync(
        SyncSourceConfig source,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var result = new SyncResult { TaskName = "Machine Matrix Sync", Division = source.Division };

        _logger($"------------------------------------------------------------");
        _logger($"[START] Syncing Machine Matrix (dmMachinesProcess): {source.Division} ({source.Name})...");
        _logger($"Source: {source.ServerName} -> DB: {source.DatabaseName}");

        try
        {
            var dt = await ExtractMachinesProcessDataAsync(source, cancellationToken);
            result.RecordsExtracted = dt.Rows.Count;
            _logger($"Extracted {result.RecordsExtracted:N0} machine-process matrix records from {source.Division}.");

            if (result.RecordsExtracted == 0)
            {
                _logger($"ℹ️ No machine-process records found for division {source.Division}.");
                result.Success = true;
                sw.Stop();
                result.Duration = sw.Elapsed;
                return result;
            }

            result.RecordsUpserted = await UpsertToDestinationAsync(dt, cancellationToken);
            _logger($"✅ Successfully upserted {result.RecordsUpserted:N0} records into [dbo].[CI_MachinesProcess] ({source.Division}).");

            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger($"❌ Error syncing Machine Matrix for {source.Division}: {ex.Message}");
        }
        finally
        {
            sw.Stop();
            result.Duration = sw.Elapsed;
            _logger($"[END] Division: {source.Division} finished in {sw.Elapsed.TotalSeconds:F2}s.");
        }

        return result;
    }

    private async Task<DataTable> ExtractMachinesProcessDataAsync(
        SyncSourceConfig source,
        CancellationToken cancellationToken)
    {
        var dt = new DataTable();
        dt.Columns.Add("MachCode", typeof(string));
        dt.Columns.Add("ProcCode", typeof(string));
        dt.Columns.Add("CurrentCount", typeof(int));
        dt.Columns.Add("MaxCount", typeof(int));
        dt.Columns.Add("Division", typeof(string));

        // In MLS, CurrentCount and MaxCount exist. In AIS/Optic, they may not exist.
        // We dynamically check sys.columns or use ISNULL if column exists.
        string sql = @"
SELECT 
    LTRIM(RTRIM(ISNULL(MachCode, ''))) AS [MachCode],
    LTRIM(RTRIM(ISNULL(ProcCode, ''))) AS [ProcCode],
    " + (string.Equals(source.Division, "MLS", StringComparison.OrdinalIgnoreCase) 
        ? "ISNULL(CurrentCount, 0) AS [CurrentCount], ISNULL(MaxCount, 0) AS [MaxCount]" 
        : "CAST(0 AS INT) AS [CurrentCount], CAST(0 AS INT) AS [MaxCount]") + @"
FROM dmMachinesProcess WITH (NOLOCK)
WHERE MachCode IS NOT NULL 
  AND LTRIM(RTRIM(MachCode)) <> ''
  AND ProcCode IS NOT NULL 
  AND LTRIM(RTRIM(ProcCode)) <> '';";

        using var conn = new SqlConnection(source.ConnectionString);
        await conn.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand(sql, conn);
        cmd.CommandTimeout = 300;

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            string machCode = reader.GetString(0);
            string procCode = reader.GetString(1);
            int currentCount = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetValue(2));
            int maxCount = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetValue(3));

            dt.Rows.Add(machCode, procCode, currentCount, maxCount, source.Division);
        }

        return dt;
    }

    private async Task<int> UpsertToDestinationAsync(
        DataTable dt,
        CancellationToken cancellationToken)
    {
        if (dt.Rows.Count == 0) return 0;

        using var conn = new SqlConnection(_destConnStr);
        await conn.OpenAsync(cancellationToken);

        const string createTempSql = @"
CREATE TABLE #Stage_MachinesProcess (
    [MachCode]     NVARCHAR(50) NOT NULL,
    [ProcCode]     NVARCHAR(50) NOT NULL,
    [CurrentCount] INT          NULL,
    [MaxCount]     INT          NULL,
    [Division]     NVARCHAR(50) NULL
);";

        using (var cmdTemp = new SqlCommand(createTempSql, conn))
        {
            await cmdTemp.ExecuteNonQueryAsync(cancellationToken);
        }

        using (var bcp = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, null))
        {
            bcp.DestinationTableName = "#Stage_MachinesProcess";
            bcp.BulkCopyTimeout = 300;
            bcp.BatchSize = 5000;

            bcp.ColumnMappings.Add("MachCode", "MachCode");
            bcp.ColumnMappings.Add("ProcCode", "ProcCode");
            bcp.ColumnMappings.Add("CurrentCount", "CurrentCount");
            bcp.ColumnMappings.Add("MaxCount", "MaxCount");
            bcp.ColumnMappings.Add("Division", "Division");

            await bcp.WriteToServerAsync(dt, cancellationToken);
        }

        const string mergeSql = @"
MERGE INTO [dbo].[CI_MachinesProcess] AS target
USING (
    SELECT 
        MachCode, ProcCode, CurrentCount, MaxCount, Division,
        ROW_NUMBER() OVER (PARTITION BY MachCode, ProcCode ORDER BY CurrentCount DESC) AS rn
    FROM #Stage_MachinesProcess
) AS src
ON (
    target.MachCode = src.MachCode 
    AND target.ProcCode = src.ProcCode
    AND src.rn = 1
)
WHEN MATCHED THEN
    UPDATE SET 
        target.CurrentCount = src.CurrentCount,
        target.MaxCount = src.MaxCount,
        target.Division = src.Division,
        target.LastSyncDate = GETDATE()
WHEN NOT MATCHED BY TARGET AND src.rn = 1 THEN
    INSERT (MachCode, ProcCode, CurrentCount, MaxCount, Division, LastSyncDate)
    VALUES (src.MachCode, src.ProcCode, src.CurrentCount, src.MaxCount, src.Division, GETDATE());

DROP TABLE #Stage_MachinesProcess;
";

        using (var cmdMerge = new SqlCommand(mergeSql, conn))
        {
            cmdMerge.CommandTimeout = 300;
            int affected = await cmdMerge.ExecuteNonQueryAsync(cancellationToken);
            return dt.Rows.Count;
        }
    }
}
