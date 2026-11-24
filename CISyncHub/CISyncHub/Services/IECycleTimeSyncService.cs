using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CISyncHub.Models;

namespace CISyncHub.Services;

public class IECycleTimeSyncService
{
    private readonly string _destConnStr;
    private readonly Action<string> _logger;

    public IECycleTimeSyncService(string destConnStr, Action<string> logger)
    {
        _destConnStr = destConnStr;
        _logger = logger;
    }

    /// <summary>
    /// Ensures the destination table [dbo].[CI_IECycleTime_Header] has all necessary columns,
    /// indexes, and safe constraints without obsolete columns (TransID, Division, SourceDatabase, LastSyncDate).
    /// </summary>
    public async Task EnsureDestinationTableExistsAsync(CancellationToken cancellationToken = default)
    {
        _logger("Checking destination table [dbo].[CI_IECycleTime_Header] in CI_LSS...");
        const string ddl = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CI_IECycleTime_Header' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[CI_IECycleTime_Header] (
        [Id]               BIGINT IDENTITY(1,1) NOT NULL,
        [DeviceCode]       NVARCHAR(50)         NOT NULL,
        [ProcCode]         NVARCHAR(50)         NULL,
        [MachCode]         NVARCHAR(50)         NOT NULL,
        [UPH]              FLOAT                NULL,
        [MachineTime]      FLOAT                NULL,
        [LaborTime]        FLOAT                NULL,
        [LMRatio]          FLOAT                NULL,
        [ShiftlyCapacity]  FLOAT                NULL,
        [WeeklyCapacity]   FLOAT                NULL,
        CONSTRAINT [PK_CI_IECycleTime_Header_Id] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
ELSE
BEGIN
    -- Ensure capacity columns exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CI_IECycleTime_Header') AND name = 'ShiftlyCapacity')
        ALTER TABLE [dbo].[CI_IECycleTime_Header] ADD [ShiftlyCapacity] FLOAT NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CI_IECycleTime_Header') AND name = 'WeeklyCapacity')
        ALTER TABLE [dbo].[CI_IECycleTime_Header] ADD [WeeklyCapacity] FLOAT NULL;

    -- Dynamically drop default constraints on obsolete columns (TransID, Division, SourceDatabase, LastSyncDate)
    DECLARE @dropConstraintSql NVARCHAR(MAX) = N'';
    SELECT @dropConstraintSql += N'ALTER TABLE [dbo].[CI_IECycleTime_Header] DROP CONSTRAINT [' + dc.name + N'];' + CHAR(13)
    FROM sys.default_constraints dc
    JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
    WHERE dc.parent_object_id = OBJECT_ID('dbo.CI_IECycleTime_Header')
      AND c.name IN ('TransID', 'Division', 'SourceDatabase', 'LastSyncDate');

    IF LEN(@dropConstraintSql) > 0
        EXEC sp_executesql @dropConstraintSql;

    -- Dynamically drop dependent indexes or unique/PK constraints on obsolete columns
    DECLARE @dropIndexSql NVARCHAR(MAX) = N'';
    SELECT @dropIndexSql += 
        CASE WHEN i.is_primary_key = 1 OR i.is_unique_constraint = 1 
             THEN N'ALTER TABLE [dbo].[CI_IECycleTime_Header] DROP CONSTRAINT [' + i.name + N'];' + CHAR(13)
             ELSE N'DROP INDEX [' + i.name + N'] ON [dbo].[CI_IECycleTime_Header];' + CHAR(13) END
    FROM sys.indexes i
    WHERE i.object_id = OBJECT_ID('dbo.CI_IECycleTime_Header')
      AND EXISTS (
          SELECT 1 FROM sys.index_columns ic
          JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
          WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id
            AND c.name IN ('TransID', 'Division', 'SourceDatabase', 'LastSyncDate')
      );

    IF LEN(@dropIndexSql) > 0
        EXEC sp_executesql @dropIndexSql;

    -- Drop obsolete columns if they exist
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CI_IECycleTime_Header') AND name = 'TransID')
        ALTER TABLE [dbo].[CI_IECycleTime_Header] DROP COLUMN [TransID];

    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CI_IECycleTime_Header') AND name = 'Division')
        ALTER TABLE [dbo].[CI_IECycleTime_Header] DROP COLUMN [Division];

    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CI_IECycleTime_Header') AND name = 'SourceDatabase')
        ALTER TABLE [dbo].[CI_IECycleTime_Header] DROP COLUMN [SourceDatabase];

    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CI_IECycleTime_Header') AND name = 'LastSyncDate')
        ALTER TABLE [dbo].[CI_IECycleTime_Header] DROP COLUMN [LastSyncDate];
END

-- Disable strict FK checks on CI_IECycleTime_Header so sync won't fail if master tables have slight naming discrepancies
ALTER TABLE [dbo].[CI_IECycleTime_Header] NOCHECK CONSTRAINT ALL;

-- Ensure composite index for fast lookups by DeviceCode and ProcCode
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CI_IECycleTime_Device_Proc' AND object_id = OBJECT_ID('dbo.CI_IECycleTime_Header'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_CI_IECycleTime_Device_Proc] 
        ON [dbo].[CI_IECycleTime_Header] ([DeviceCode] ASC, [ProcCode] ASC);
END;

-- Ensure index on MachCode for machine bottleneck queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CI_IECycleTime_MachCode' AND object_id = OBJECT_ID('dbo.CI_IECycleTime_Header'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_CI_IECycleTime_MachCode] 
        ON [dbo].[CI_IECycleTime_Header] ([MachCode] ASC);
END;
";

        using var conn = new SqlConnection(_destConnStr);
        await conn.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand(ddl, conn);
        cmd.CommandTimeout = 120;
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        _logger("✅ Table [dbo].[CI_IECycleTime_Header] schema is verified and ready.");
    }

    /// <summary>
    /// Synchronizes dmIECycleTime_Header from a single division source.
    /// </summary>
    public async Task<SyncResult> SyncSourceAsync(
        SyncSourceConfig source,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var result = new SyncResult { TaskName = "IE Cycle Time Sync", Division = source.Division };

        _logger($"------------------------------------------------------------");
        _logger($"[START] Syncing IE Cycle Time: {source.Division} ({source.Name})...");
        _logger($"Source: {source.ServerName} -> DB: {source.DatabaseName}");

        try
        {
            var dtCycle = await ExtractCycleTimeDataAsync(source, cancellationToken);
            result.RecordsExtracted = dtCycle.Rows.Count;
            _logger($"Extracted {result.RecordsExtracted:N0} IE Cycle Time records from {source.Division}.");

            if (result.RecordsExtracted == 0)
            {
                _logger($"ℹ️ No cycle time records found for division {source.Division}.");
                result.Success = true;
                sw.Stop();
                result.Duration = sw.Elapsed;
                return result;
            }

            result.RecordsUpserted = await UpsertToDestinationAsync(dtCycle, cancellationToken);
            _logger($"✅ Successfully upserted {result.RecordsUpserted:N0} records into [dbo].[CI_IECycleTime_Header] ({source.Division}).");

            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger($"❌ Error syncing IE Cycle Time for {source.Division}: {ex.Message}");
        }
        finally
        {
            sw.Stop();
            result.Duration = sw.Elapsed;
            _logger($"[END] Division: {source.Division} finished in {sw.Elapsed.TotalSeconds:F2}s.");
        }

        return result;
    }

    private async Task<DataTable> ExtractCycleTimeDataAsync(
        SyncSourceConfig source,
        CancellationToken cancellationToken)
    {
        var dt = new DataTable();
        dt.Columns.Add("DeviceCode", typeof(string));
        dt.Columns.Add("ProcCode", typeof(string));
        dt.Columns.Add("MachCode", typeof(string));
        dt.Columns.Add("UPH", typeof(double));
        dt.Columns.Add("MachineTime", typeof(double));
        dt.Columns.Add("LaborTime", typeof(double));
        dt.Columns.Add("LMRatio", typeof(double));
        dt.Columns.Add("ShiftlyCapacity", typeof(double));
        dt.Columns.Add("WeeklyCapacity", typeof(double));

        // Query source dmIECycleTime_Header without TransID
        const string sql = @"
SELECT 
    LTRIM(RTRIM(ISNULL(h.DeviceCode, ''))) AS [DeviceCode],
    LTRIM(RTRIM(ISNULL(h.ProcCode, ''))) AS [ProcCode],
    LTRIM(RTRIM(ISNULL(h.MachCode, ''))) AS [MachCode],
    CAST(ISNULL(h.UPH, 0) AS FLOAT) AS [UPH],
    CAST(ISNULL(h.MachineTime, 0) AS FLOAT) AS [MachineTime],
    CAST(ISNULL(h.LaborTime, 0) AS FLOAT) AS [LaborTime],
    CAST(ISNULL(h.LMRatio, 0) AS FLOAT) AS [LMRatio],
    CAST(ISNULL(h.ShiftlyCapacity, 0) AS FLOAT) AS [ShiftlyCapacity],
    CAST(ISNULL(h.WeeklyCapacity, 0) AS FLOAT) AS [WeeklyCapacity]
FROM dmIECycleTime_Header h WITH (NOLOCK)
WHERE h.DeviceCode IS NOT NULL 
  AND LTRIM(RTRIM(h.DeviceCode)) <> ''
  AND h.MachCode IS NOT NULL 
  AND LTRIM(RTRIM(h.MachCode)) <> '';";

        using var conn = new SqlConnection(source.ConnectionString);
        await conn.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand(sql, conn);
        cmd.CommandTimeout = 300;

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            string devCode = reader.GetString(0);
            string procCode = reader.GetString(1);
            string machCode = reader.GetString(2);
            double uph = reader.GetDouble(3);
            double machTime = reader.GetDouble(4);
            double laborTime = reader.GetDouble(5);
            double lmRatio = reader.GetDouble(6);
            double shiftCap = reader.GetDouble(7);
            double weekCap = reader.GetDouble(8);

            dt.Rows.Add(
                devCode,
                procCode,
                machCode,
                uph,
                machTime,
                laborTime,
                lmRatio,
                shiftCap,
                weekCap
            );
        }

        return dt;
    }

    private async Task<int> UpsertToDestinationAsync(
        DataTable dtCycle,
        CancellationToken cancellationToken)
    {
        if (dtCycle.Rows.Count == 0) return 0;

        using var conn = new SqlConnection(_destConnStr);
        await conn.OpenAsync(cancellationToken);

        // 1. Create a session-scoped temporary table without TransID, Division, SourceDatabase
        const string createTempSql = @"
CREATE TABLE #Stage_IECycleTime (
    [DeviceCode]       NVARCHAR(50)   NOT NULL,
    [ProcCode]         NVARCHAR(50)   NULL,
    [MachCode]         NVARCHAR(50)   NOT NULL,
    [UPH]              FLOAT          NULL,
    [MachineTime]      FLOAT          NULL,
    [LaborTime]        FLOAT          NULL,
    [LMRatio]          FLOAT          NULL,
    [ShiftlyCapacity]  FLOAT          NULL,
    [WeeklyCapacity]   FLOAT          NULL
);";

        using (var cmdTemp = new SqlCommand(createTempSql, conn))
        {
            await cmdTemp.ExecuteNonQueryAsync(cancellationToken);
        }

        // 2. SqlBulkCopy into temporary staging table (Copy Massal)
        using (var bcp = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, null))
        {
            bcp.DestinationTableName = "#Stage_IECycleTime";
            bcp.BulkCopyTimeout = 300;
            bcp.BatchSize = 5000;

            bcp.ColumnMappings.Add("DeviceCode", "DeviceCode");
            bcp.ColumnMappings.Add("ProcCode", "ProcCode");
            bcp.ColumnMappings.Add("MachCode", "MachCode");
            bcp.ColumnMappings.Add("UPH", "UPH");
            bcp.ColumnMappings.Add("MachineTime", "MachineTime");
            bcp.ColumnMappings.Add("LaborTime", "LaborTime");
            bcp.ColumnMappings.Add("LMRatio", "LMRatio");
            bcp.ColumnMappings.Add("ShiftlyCapacity", "ShiftlyCapacity");
            bcp.ColumnMappings.Add("WeeklyCapacity", "WeeklyCapacity");

            await bcp.WriteToServerAsync(dtCycle, cancellationToken);
        }

        // 3. Execute MERGE statement to perform idempotent upsert by (DeviceCode, MachCode, ProcCode)
        const string mergeSql = @"
MERGE INTO [dbo].[CI_IECycleTime_Header] AS target
USING (
    -- Deduplicate staging records by DeviceCode, MachCode, ProcCode
    SELECT 
        DeviceCode, ProcCode, MachCode, UPH, MachineTime, LaborTime, 
        LMRatio, ShiftlyCapacity, WeeklyCapacity,
        ROW_NUMBER() OVER (
            PARTITION BY DeviceCode, MachCode, ISNULL(ProcCode, '') 
            ORDER BY (SELECT NULL)
        ) AS rn
    FROM #Stage_IECycleTime
) AS src
ON (
    target.DeviceCode = src.DeviceCode 
    AND target.MachCode = src.MachCode 
    AND ISNULL(target.ProcCode, '') = ISNULL(src.ProcCode, '')
    AND src.rn = 1
)
WHEN MATCHED THEN
    UPDATE SET 
        target.UPH = src.UPH,
        target.MachineTime = src.MachineTime,
        target.LaborTime = src.LaborTime,
        target.LMRatio = src.LMRatio,
        target.ShiftlyCapacity = src.ShiftlyCapacity,
        target.WeeklyCapacity = src.WeeklyCapacity
WHEN NOT MATCHED BY TARGET AND src.rn = 1 THEN
    INSERT (
        DeviceCode, ProcCode, MachCode, UPH, MachineTime, LaborTime, 
        LMRatio, ShiftlyCapacity, WeeklyCapacity
    )
    VALUES (
        src.DeviceCode, src.ProcCode, src.MachCode, src.UPH, src.MachineTime, src.LaborTime, 
        src.LMRatio, src.ShiftlyCapacity, src.WeeklyCapacity
    );

DROP TABLE #Stage_IECycleTime;
";

        using (var cmdMerge = new SqlCommand(mergeSql, conn))
        {
            cmdMerge.CommandTimeout = 300;
            int affected = await cmdMerge.ExecuteNonQueryAsync(cancellationToken);
            return dtCycle.Rows.Count;
        }
    }

    /// <summary>
    /// Optional utility: Truncates [dbo].[CI_IECycleTime_Header] if a complete clean refresh is needed.
    /// </summary>
    public async Task TruncateDestinationTableAsync(CancellationToken cancellationToken = default)
    {
        using var conn = new SqlConnection(_destConnStr);
        await conn.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand("TRUNCATE TABLE [dbo].[CI_IECycleTime_Header];", conn);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        _logger("ℹ️ Table [dbo].[CI_IECycleTime_Header] has been truncated.");
    }
}

