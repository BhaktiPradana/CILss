using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CISyncHub.Models;

namespace CISyncHub.Services;

public class EmployeeSyncService
{
    private readonly string _destConnStr;
    private readonly Action<string> _logger;

    public EmployeeSyncService(string destConnStr, Action<string> logger)
    {
        _destConnStr = destConnStr;
        _logger = logger;
    }

    public async Task EnsureDestinationTableExistsAsync(CancellationToken cancellationToken = default)
    {
        _logger("Checking destination table [dbo].[CI_Employee] in CI_LSS...");
        const string ddl = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CI_Employee' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[CI_Employee] (
        [EMP_NO]           VARCHAR(20)   NOT NULL,
        [EMP_NAME]         VARCHAR(100)  NOT NULL,
        [DEPARTMENT_CODE]  VARCHAR(50)   NULL,
        [COST_CENTRE_CODE] VARCHAR(50)   NULL,
        [SECTION_CODE]     VARCHAR(50)   NULL,
        [WORKGROUP]        VARCHAR(50)   NULL,
        [PATTERN_CODE]     VARCHAR(50)   NULL,
        [LastSyncDate]     DATETIME      NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_CI_Employee] PRIMARY KEY CLUSTERED ([EMP_NO] ASC)
    );

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CI_Employee_Section' AND object_id = OBJECT_ID('dbo.CI_Employee'))
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_CI_Employee_Section] ON [dbo].[CI_Employee] ([SECTION_CODE] ASC);
    END;
END;";

        using var conn = new SqlConnection(_destConnStr);
        await conn.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand(ddl, conn);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        _logger("✅ Table [dbo].[CI_Employee] is verified.");
    }

    public async Task<EmployeeSyncResult> SyncEmployeesAsync(string sourceConnStr, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var result = new EmployeeSyncResult();

        _logger("------------------------------------------------------------");
        _logger("[START] Syncing Active Employees from BTMTime (ETCRESSQL02)...");

        try
        {
            // 1. Ensure target table exists
            await EnsureDestinationTableExistsAsync(cancellationToken);

            // 2. Read employee data from source server [BTMTime]
            var dtEmployee = new DataTable();
            _logger("Connecting to source [ETCRESSQL02.BTMTime]...");

            using (var sourceConn = new SqlConnection(sourceConnStr))
            {
                await sourceConn.OpenAsync(cancellationToken);
                const string query = @"
                    SELECT 
                        EMP_NO, 
                        EMP_NAME, 
                        DEPARTMENT_CODE, 
                        COST_CENTRE_CODE, 
                        SECTION_CODE, 
                        WORKGROUP, 
                        PATTERN_CODE
                    FROM EMPLOYEE WITH (NOLOCK)
                    WHERE TERMINATION_STATUS IS NULL OR LTRIM(RTRIM(TERMINATION_STATUS)) = ''";

                using var cmd = new SqlCommand(query, sourceConn);
                cmd.CommandTimeout = 120;
                using var da = new SqlDataAdapter(cmd);
                da.Fill(dtEmployee);
            }

            result.TotalEmployeesSynced = dtEmployee.Rows.Count;
            _logger($"Found {result.TotalEmployeesSynced:N0} active employees in BTMTime.");

            if (result.TotalEmployeesSynced == 0)
            {
                _logger("⚠️ No employee records found to transfer. Aborting.");
                result.Success = true;
                return result;
            }

            // 3. Connect to destination server [CI_LSS] and transfer
            _logger("Connecting to destination [CI_LSS] to update CI_Employee...");
            using (var destConn = new SqlConnection(_destConnStr))
            {
                await destConn.OpenAsync(cancellationToken);

                // Use transaction to ensure safe truncate and reload
                using var tx = destConn.BeginTransaction();
                try
                {
                    _logger("Refreshing records in CI_Employee table...");
                    using (var cmdClear = new SqlCommand("TRUNCATE TABLE CI_Employee", destConn, tx))
                    {
                        await cmdClear.ExecuteNonQueryAsync(cancellationToken);
                    }

                    using (var bulkCopy = new SqlBulkCopy(destConn, SqlBulkCopyOptions.Default, tx))
                    {
                        bulkCopy.DestinationTableName = "CI_Employee";
                        bulkCopy.BulkCopyTimeout = 600;
                        bulkCopy.BatchSize = 5000;

                        bulkCopy.ColumnMappings.Add("EMP_NO", "EMP_NO");
                        bulkCopy.ColumnMappings.Add("EMP_NAME", "EMP_NAME");
                        bulkCopy.ColumnMappings.Add("DEPARTMENT_CODE", "DEPARTMENT_CODE");
                        bulkCopy.ColumnMappings.Add("COST_CENTRE_CODE", "COST_CENTRE_CODE");
                        bulkCopy.ColumnMappings.Add("SECTION_CODE", "SECTION_CODE");
                        bulkCopy.ColumnMappings.Add("WORKGROUP", "WORKGROUP");
                        bulkCopy.ColumnMappings.Add("PATTERN_CODE", "PATTERN_CODE");

                        await bulkCopy.WriteToServerAsync(dtEmployee, cancellationToken);
                    }

                    tx.Commit();
                    _logger($"✅ Successfully transferred {result.TotalEmployeesSynced:N0} employees to [dbo].[CI_Employee].");
                    result.Success = true;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger($"❌ Error syncing Employees: {ex.Message}");
        }
        finally
        {
            sw.Stop();
            result.Duration = sw.Elapsed;
            _logger($"[END] Employee sync finished in {sw.Elapsed.TotalSeconds:F2}s.");
        }

        return result;
    }
}

