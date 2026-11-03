using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YieldSyncApp;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        // Check if headless sync was requested from command line / Task Scheduler
        if (args.Contains("--silent") || args.Contains("--sync-now"))
        {
            RunHeadlessSync().GetAwaiter().GetResult();
            return;
        }

        Application.Run(new MainForm());
    }

    private static async Task RunHeadlessSync()
    {
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Starting headless CI Yield Sync...");
        var form = new MainForm();
        
        // Execute sync with default last 7 days window
        var method = form.GetType().GetMethod("ExecuteSyncAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (method != null)
        {
            var task = (Task)method.Invoke(form, null)!;
            await task;
        }
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Headless CI Yield Sync completed.");
    }
}

