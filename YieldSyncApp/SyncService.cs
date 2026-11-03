using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace YieldSyncApp;

public class SyncService
{
    private readonly string _destConnStr;
    private readonly Action<string> _logger;

    public SyncService(string destConnStr, Action<string> logger)
    {
        _destConnStr = destConnStr;
        _logger = logger;
    }

    public async Task<bool> TestConnectionAsync(string connStr, string serverLabel, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger($"Testing connection to [{serverLabel}]...");
            using var conn = new SqlConnection(connStr);
            var connectTask = conn.OpenAsync(cancellationToken);
            if (await Task.WhenAny(connectTask, Task.Delay(8000, cancellationToken)) == connectTask)
            {
                await connectTask; // throw any exception if occurred
                _logger($"✅ Connection to [{serverLabel}] SUCCESSFUL.");
                return true;
            }
            else
            {
                _logger($"❌ Connection to [{serverLabel}] TIMED OUT (8s).");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger($"❌ Connection to [{serverLabel}] FAILED: {ex.Message}");
            return false;
        }
    }

    public async Task EnsureDestinationTableExistsAsync(CancellationToken cancellationToken = default)
    {
        _logger("Checking destination table [dbo].[CI_Yield] in CI_LSS...");
        const string ddl = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CI_Yield' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[CI_Yield] (
        [Id]               BIGINT IDENTITY(1,1) NOT NULL,
        [DeviceCode]       NVARCHAR(100)        NOT NULL,
        [Date]             DATE                 NOT NULL,
        [FirstPassYield]   DECIMAL(7, 4)        NOT NULL DEFAULT (0),
        [OverallYield]     DECIMAL(7, 4)        NOT NULL DEFAULT (0),
        [Division]         NVARCHAR(50)         NULL,
        [TotalQtyIn]       FLOAT                NULL DEFAULT (0),
        [TotalQtyOut]      FLOAT                NULL DEFAULT (0),
        [TotalDefectFirst] FLOAT                NULL DEFAULT (0),
        [TotalDefectFinal] FLOAT                NULL DEFAULT (0),
        [SourceServer]     NVARCHAR(100)        NULL,
        [SourceDatabase]   NVARCHAR(100)        NULL,
        [LastSyncDate]     DATETIME             NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_CI_Yield] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_CI_Yield_Device_Date_Div' AND object_id = OBJECT_ID('dbo.CI_Yield'))
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX [UQ_CI_Yield_Device_Date_Div] 
            ON [dbo].[CI_Yield] ([DeviceCode] ASC, [Date] ASC, [Division] ASC);
    END;

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CI_Yield_Date' AND object_id = OBJECT_ID('dbo.CI_Yield'))
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_CI_Yield_Date] ON [dbo].[CI_Yield] ([Date] ASC);
    END;

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CI_Yield_DeviceCode' AND object_id = OBJECT_ID('dbo.CI_Yield'))
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_CI_Yield_DeviceCode] ON [dbo].[CI_Yield] ([DeviceCode] ASC);
    END;
END;";

        using var conn = new SqlConnection(_destConnStr);
        await conn.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand(ddl, conn);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        _logger("✅ Table [dbo].[CI_Yield] is ready and verified.");
    }

    public async Task<SyncResult> SyncSourceAsync(
        SyncSourceConfig source,
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var result = new SyncResult { Division = source.Division };

        _logger($"------------------------------------------------------------");
        _logger($"[START] Syncing Division: {source.Division} ({source.Name})...");
        _logger($"Source: {source.ServerName} -> DB: {source.DatabaseName}");

        try
        {
            var dtYield = await ExtractYieldDataAsync(source, startDate, endDate, cancellationToken);
            result.RecordsExtracted = dtYield.Rows.Count;
            _logger($"Extracted {result.RecordsExtracted:N0} daily yield records from {source.Division}.");

            if (result.RecordsExtracted == 0)
            {
                _logger($"ℹ️ No records found for division {source.Division} in the selected date window.");
                result.Success = true;
                sw.Stop();
                result.Duration = sw.Elapsed;
                return result;
            }

            // Upsert into target table CI_Yield
            result.RecordsUpserted = await UpsertToDestinationAsync(dtYield, cancellationToken);
            _logger($"✅ Successfully upserted {result.RecordsUpserted:N0} records into [dbo].[CI_Yield].");

            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger($"❌ Error syncing {source.Division}: {ex.Message}");
        }
        finally
        {
            sw.Stop();
            result.Duration = sw.Elapsed;
            _logger($"[END] Division: {source.Division} finished in {sw.Elapsed.TotalSeconds:F2} seconds.");
        }

        return result;
    }

    private async Task<DataTable> ExtractYieldDataAsync(
        SyncSourceConfig source,
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken)
    {
        var dt = new DataTable();
        dt.Columns.Add("DeviceCode", typeof(string));
        dt.Columns.Add("Date", typeof(DateTime));
        dt.Columns.Add("FirstPassYield", typeof(decimal));
        dt.Columns.Add("OverallYield", typeof(decimal));
        dt.Columns.Add("Division", typeof(string));
        dt.Columns.Add("TotalQtyIn", typeof(double));
        dt.Columns.Add("TotalQtyOut", typeof(double));
        dt.Columns.Add("TotalDefectFirst", typeof(double));
        dt.Columns.Add("TotalDefectFinal", typeof(double));
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

        string sql = $@"
SELECT 
    LTRIM(RTRIM(p.DeviceCode)) AS [DeviceCode],
    CAST(COALESCE(p.DateFinishActual, p.DateFinish, p.DateStart) AS DATE) AS [Date],
    SUM(CAST(ISNULL(p.QuantityIn, 0) AS FLOAT)) AS [TotalQtyIn],
    SUM(CAST(ISNULL(p.QuantityOut, 0) AS FLOAT)) AS [TotalQtyOut],
    SUM(CAST(ISNULL(d.DefectFirst, ISNULL(p.QuantityDefect, 0)) AS FLOAT)) AS [TotalDefectFirst],
    SUM(CAST(ISNULL(p.QuantityDefect, 0) AS FLOAT)) AS [TotalDefectFinal]
FROM dmPChart p WITH (NOLOCK)
LEFT JOIN (
    SELECT 
        ID, 
        SUM(CAST(ISNULL(QuantityDefectFirst, QuantityDefect) AS FLOAT)) AS DefectFirst,
        SUM(CAST(ISNULL(QuantityDefect, 0) AS FLOAT)) AS DefectFinal
    FROM dmPChartDefect WITH (NOLOCK)
    GROUP BY ID
) d ON p.ID = d.ID
WHERE p.DeviceCode IS NOT NULL 
  AND LTRIM(RTRIM(p.DeviceCode)) <> ''
  AND COALESCE(p.DateFinishActual, p.DateFinish, p.DateStart) IS NOT NULL
  {dateFilter}
GROUP BY 
    LTRIM(RTRIM(p.DeviceCode)),
    CAST(COALESCE(p.DateFinishActual, p.DateFinish, p.DateStart) AS DATE)
ORDER BY 
    [Date] DESC, [DeviceCode] ASC;";

        using var conn = new SqlConnection(source.ConnectionString);
        await conn.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand(sql, conn);
        cmd.CommandTimeout = 300;
        if (startDate.HasValue)
            cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = startDate.Value });
        if (endDate.HasValue)
            cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = endDate.Value.Date.AddDays(1) });

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            string deviceCode = reader.GetString(0);
            DateTime date = reader.GetDateTime(1);
            double qtyIn = reader.IsDBNull(2) ? 0 : Convert.ToDouble(reader.GetValue(2));
            double qtyOut = reader.IsDBNull(3) ? 0 : Convert.ToDouble(reader.GetValue(3));
            double defectFirst = reader.IsDBNull(4) ? 0 : Convert.ToDouble(reader.GetValue(4));
            double defectFinal = reader.IsDBNull(5) ? 0 : Convert.ToDouble(reader.GetValue(5));

            // Calculate FPY (%)
            decimal fpy = 0m;
            if (qtyIn > 0)
            {
                double passFirst = qtyIn - defectFirst;
                if (passFirst < 0) passFirst = 0;
                double calc = (passFirst / qtyIn) * 100.0;
                if (calc > 100.0) calc = 100.0;
                fpy = Math.Round((decimal)calc, 4);
            }

            // Calculate Overall Yield (%)
            decimal overallYield = 0m;
            if (qtyIn > 0)
            {
                double goodUnits = qtyOut > 0 ? qtyOut : (qtyIn - defectFinal);
                if (goodUnits < 0) goodUnits = 0;
                double calc = (goodUnits / qtyIn) * 100.0;
                if (calc > 100.0) calc = 100.0;
                overallYield = Math.Round((decimal)calc, 4);
            }

            dt.Rows.Add(
                deviceCode,
                date,
                fpy,
                overallYield,
                source.Division,
                qtyIn,
                qtyOut,
                defectFirst,
                defectFinal,
                source.ServerName,
                source.DatabaseName
            );
        }

        return dt;
    }

    private async Task<int> UpsertToDestinationAsync(DataTable dtSource, CancellationToken cancellationToken)
    {
        using var conn = new SqlConnection(_destConnStr);
        await conn.OpenAsync(cancellationToken);

        // 1. Create a local temporary staging table
        const string createStagingSql = @"
CREATE TABLE #Staging_CI_Yield (
    [DeviceCode]       NVARCHAR(100) NOT NULL,
    [Date]             DATE          NOT NULL,
    [FirstPassYield]   DECIMAL(7, 4) NOT NULL,
    [OverallYield]     DECIMAL(7, 4) NOT NULL,
    [Division]         NVARCHAR(50)  NOT NULL,
    [TotalQtyIn]       FLOAT         NOT NULL,
    [TotalQtyOut]      FLOAT         NOT NULL,
    [TotalDefectFirst] FLOAT         NOT NULL,
    [TotalDefectFinal] FLOAT         NOT NULL,
    [SourceServer]     NVARCHAR(100) NULL,
    [SourceDatabase]   NVARCHAR(100) NULL
);";

        using (var cmdStaging = new SqlCommand(createStagingSql, conn))
        {
            await cmdStaging.ExecuteNonQueryAsync(cancellationToken);
        }

        // 2. Bulk copy into staging table
        using (var bulkCopy = new SqlBulkCopy(conn))
        {
            bulkCopy.DestinationTableName = "#Staging_CI_Yield";
            bulkCopy.BulkCopyTimeout = 300;
            bulkCopy.BatchSize = 5000;

            bulkCopy.ColumnMappings.Add("DeviceCode", "DeviceCode");
            bulkCopy.ColumnMappings.Add("Date", "Date");
            bulkCopy.ColumnMappings.Add("FirstPassYield", "FirstPassYield");
            bulkCopy.ColumnMappings.Add("OverallYield", "OverallYield");
            bulkCopy.ColumnMappings.Add("Division", "Division");
            bulkCopy.ColumnMappings.Add("TotalQtyIn", "TotalQtyIn");
            bulkCopy.ColumnMappings.Add("TotalQtyOut", "TotalQtyOut");
            bulkCopy.ColumnMappings.Add("TotalDefectFirst", "TotalDefectFirst");
            bulkCopy.ColumnMappings.Add("TotalDefectFinal", "TotalDefectFinal");
            bulkCopy.ColumnMappings.Add("SourceServer", "SourceServer");
            bulkCopy.ColumnMappings.Add("SourceDatabase", "SourceDatabase");

            await bulkCopy.WriteToServerAsync(dtSource, cancellationToken);
        }

        // 3. MERGE from staging to destination table [dbo].[CI_Yield]
        const string mergeSql = @"
MERGE INTO [dbo].[CI_Yield] AS target
USING #Staging_CI_Yield AS source
ON (
    target.DeviceCode = source.DeviceCode 
    AND target.Date = source.Date 
    AND target.Division = source.Division
)
WHEN MATCHED THEN
    UPDATE SET 
        target.FirstPassYield   = source.FirstPassYield,
        target.OverallYield     = source.OverallYield,
        target.TotalQtyIn       = source.TotalQtyIn,
        target.TotalQtyOut      = source.TotalQtyOut,
        target.TotalDefectFirst = source.TotalDefectFirst,
        target.TotalDefectFinal = source.TotalDefectFinal,
        target.SourceServer     = source.SourceServer,
        target.SourceDatabase   = source.SourceDatabase,
        target.LastSyncDate     = GETDATE()
WHEN NOT MATCHED BY TARGET THEN
    INSERT (
        DeviceCode, Date, FirstPassYield, OverallYield, 
        Division, TotalQtyIn, TotalQtyOut, TotalDefectFirst, TotalDefectFinal, 
        SourceServer, SourceDatabase, LastSyncDate
    )
    VALUES (
        source.DeviceCode, source.Date, source.FirstPassYield, source.OverallYield, 
        source.Division, source.TotalQtyIn, source.TotalQtyOut, source.TotalDefectFirst, source.TotalDefectFinal, 
        source.SourceServer, source.SourceDatabase, GETDATE()
    );";

        using (var cmdMerge = new SqlCommand(mergeSql, conn))
        {
            cmdMerge.CommandTimeout = 300;
            int affected = await cmdMerge.ExecuteNonQueryAsync(cancellationToken);
            return affected;
        }
    }
}

