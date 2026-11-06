using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using CISyncHub.Models;
using CISyncHub.Services;

namespace CISyncHub;

internal static class Program
{
    [DllImport("kernel32.dll")]
    private static extern bool AttachConsole(int dwProcessId);
    private const int ATTACH_PARENT_PROCESS = -1;

    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    [STAThread]
    static async Task<int> Main(string[] args)
    {
        // Check for CLI / headless mode flags
        bool runAll = args.Any(a => a.Equals("--sync-all", StringComparison.OrdinalIgnoreCase) || a.Equals("-all", StringComparison.OrdinalIgnoreCase));
        bool runYield = args.Any(a => a.Equals("--sync-yield", StringComparison.OrdinalIgnoreCase) || a.Equals("-yield", StringComparison.OrdinalIgnoreCase));
        bool runEmployee = args.Any(a => a.Equals("--sync-employee", StringComparison.OrdinalIgnoreCase) || a.Equals("-employee", StringComparison.OrdinalIgnoreCase));
        bool runTest = args.Any(a => a.Equals("--test-conn", StringComparison.OrdinalIgnoreCase) || a.Equals("-test", StringComparison.OrdinalIgnoreCase));

        if (runAll || runYield || runEmployee || runTest)
        {
            // Attach console output if invoked from cmd/powershell
            if (!AttachConsole(ATTACH_PARENT_PROCESS))
            {
                AllocConsole();
            }

            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine("   CI Sync Hub - Automated Headless Synchronization Runner");
            Console.WriteLine("============================================================");

            return await RunHeadlessAsync(runAll, runYield, runEmployee, runTest);
        }

        // Standard Windows GUI mode
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
        return 0;
    }

    private static async Task<int> RunHeadlessAsync(bool runAll, bool runYield, bool runEmployee, bool runTest)
    {
        try
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            if (!File.Exists(configPath))
            {
                configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            }

            string destConnStr = "Server=BTMSQL01\\DIGITMONT_DEV;Database=CI_LSS;User Id=CI_Hub_User;Password=CiHubB4tam@excelitas;TrustServerCertificate=True;";
            string sourceMLS = "Server=ETCBTMA03\\DIGITMONT;Database=digimon_lighting;Integrated Security=SSPI;TrustServerCertificate=True;";
            string sourceOptic = "Server=ETCBTMA03\\DIGITMONT;Database=digimon_vision;Integrated Security=SSPI;TrustServerCertificate=True;";
            string sourceAIS = "Server=ETCBTMSQL01\\DIGITMONT;Database=dbDigitmontDetection;Integrated Security=SSPI;TrustServerCertificate=True;";
            string sourceBTMTime = "Server=ETCRESSQL02;Database=BTMTime;User Id=BTM_IT_RO;Password=ITreadonly!;TrustServerCertificate=True;";
            int daysBack = 7;

            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("ConnectionStrings", out var conns))
                {
                    if (conns.TryGetProperty("Destination", out var pDest)) destConnStr = pDest.GetString() ?? destConnStr;
                    if (conns.TryGetProperty("Source_MLS", out var pMls)) sourceMLS = pMls.GetString() ?? sourceMLS;
                    if (conns.TryGetProperty("Source_OPTIC", out var pOpt)) sourceOptic = pOpt.GetString() ?? sourceOptic;
                    if (conns.TryGetProperty("Source_AIS", out var pAis)) sourceAIS = pAis.GetString() ?? sourceAIS;
                    if (conns.TryGetProperty("Source_BTMTime", out var pHr)) sourceBTMTime = pHr.GetString() ?? sourceBTMTime;
                }

                if (root.TryGetProperty("SyncSettings", out var sync))
                {
                    if (sync.TryGetProperty("DefaultDaysBack", out var pDays)) daysBack = pDays.GetInt32();
                }
            }

            void Logger(string msg) => Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}");

            if (runTest)
            {
                var diag = new ConnectionDiagnosticService(Logger);
                Logger("Testing all connections...");
                bool okDest = await diag.TestConnectionAsync(destConnStr, "Destination (CI_LSS)");
                bool okMls = await diag.TestConnectionAsync(sourceMLS, "Source MLS");
                bool okOptic = await diag.TestConnectionAsync(sourceOptic, "Source OPTIC");
                bool okAis = await diag.TestConnectionAsync(sourceAIS, "Source AIS");
                bool okHr = await diag.TestConnectionAsync(sourceBTMTime, "Source HR BTMTime");
                return (okDest && okMls && okOptic && okAis && okHr) ? 0 : 1;
            }

            bool overallSuccess = true;

            if (runAll || runEmployee)
            {
                var empService = new EmployeeSyncService(destConnStr, Logger);
                await empService.EnsureDestinationTableExistsAsync();
                var empRes = await empService.SyncEmployeesAsync(sourceBTMTime);
                if (!empRes.Success) overallSuccess = false;
            }

            if (runAll || runYield)
            {
                var yieldService = new YieldSyncService(destConnStr, Logger);
                await yieldService.EnsureDestinationTableExistsAsync();

                DateTime startDate = DateTime.Today.AddDays(-daysBack);
                DateTime endDate = DateTime.Today;

                var sources = new List<SyncSourceConfig>
                {
                    new SyncSourceConfig { Key = "MLS", Name = "Digitmont MLS", Division = "MLS", ConnectionString = sourceMLS, ServerName = @"ETCBTMA03\DIGITMONT", DatabaseName = "digimon_lighting" },
                    new SyncSourceConfig { Key = "OPTIC", Name = "Digitmont OPTIC", Division = "OPTIC", ConnectionString = sourceOptic, ServerName = @"ETCBTMA03\DIGITMONT", DatabaseName = "digimon_vision" },
                    new SyncSourceConfig { Key = "AIS", Name = "Digitmont AIS", Division = "AIS", ConnectionString = sourceAIS, ServerName = @"ETCBTMSQL01\DIGITMONT", DatabaseName = "dbDigitmontDetection" }
                };

                foreach (var src in sources)
                {
                    var res = await yieldService.SyncSourceAsync(src, startDate, endDate);
                    if (!res.Success) overallSuccess = false;
                }
            }

            Console.WriteLine(overallSuccess
                ? "✅ [HEADLESS SYNC SUCCESS] All requested synchronization tasks completed."
                : "❌ [HEADLESS SYNC WARNING] Some synchronization tasks reported errors.");

            return overallSuccess ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Fatal error during headless execution: {ex.Message}");
            return 1;
        }
    }
}

