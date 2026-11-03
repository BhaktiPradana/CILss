using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YieldSyncApp;

public partial class MainForm : Form
{
    private string _destConnStr = "Server=BTMSQL01\\DIGITMONT_DEV;Database=CI_LSS;User Id=CI_Hub_User;Password=CiHubB4tam@excelitas;TrustServerCertificate=True;MultipleActiveResultSets=False";
    private readonly Dictionary<string, SyncSourceConfig> _sources = new();

    private NotifyIcon? _notifyIcon;
    private ContextMenuStrip? _trayMenu;
    private bool _allowExit = false;

    private System.Windows.Forms.Timer? _scheduleTimer;
    private DateTime _lastAutoSyncDate = DateTime.MinValue;
    private int _scheduleHour = 6;
    private int _scheduleMinute = 0;

    public MainForm()
    {
        InitializeComponent();
        LoadConfiguration();
        InitializeSystemTray();
        InitializeScheduler();

        LogMessage("============================================================");
        LogMessage("🚀 CI Yield Synchronization Hub Initialized");
        LogMessage($"Target: [BTMSQL01\\DIGITMONT_DEV].[CI_LSS].[dbo].[CI_Yield]");
        LogMessage($"Scheduled Auto-Sync: {(_chkAutoSchedule_Active() ? $"Active daily at {_scheduleHour:D2}:{_scheduleMinute:D2} AM" : "Disabled")}");
        LogMessage("Click 'START SYNC NOW' to begin manual synchronization.");
        LogMessage("============================================================");
    }

    private bool _chkAutoSchedule_Active() => chkAutoSchedule?.Checked ?? true;

    private void LoadConfiguration()
    {
        // Default configs
        _sources["MLS"] = new SyncSourceConfig
        {
            Key = "MLS",
            Name = "MLS (Lighting)",
            Division = "MLS",
            ServerName = "ETCBTMA03\\DIGITMONT",
            DatabaseName = "digimon_lighting",
            ConnectionString = "Server=ETCBTMA03\\DIGITMONT;Database=digimon_lighting;Integrated Security=SSPI;TrustServerCertificate=True;",
            IsEnabled = true
        };

        _sources["OPTIC"] = new SyncSourceConfig
        {
            Key = "OPTIC",
            Name = "OPTIC / VISION",
            Division = "OPTIC",
            ServerName = "ETCBTMA03\\DIGITMONT",
            DatabaseName = "digimon_vision",
            ConnectionString = "Server=ETCBTMA03\\DIGITMONT;Database=digimon_vision;Integrated Security=SSPI;TrustServerCertificate=True;",
            IsEnabled = true
        };

        _sources["AIS"] = new SyncSourceConfig
        {
            Key = "AIS",
            Name = "AIS (Detection)",
            Division = "AIS",
            ServerName = "ETCBTMSQL01\\DIGITMONT",
            DatabaseName = "dbDigitmontDetection",
            ConnectionString = "Server=ETCBTMSQL01\\DIGITMONT;Database=dbDigitmontDetection;Integrated Security=SSPI;TrustServerCertificate=True;",
            IsEnabled = true
        };

        // Try load from appsettings.json
        try
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("ConnectionStrings", out var connStrings))
                {
                    if (connStrings.TryGetProperty("Destination", out var dest))
                        _destConnStr = dest.GetString() ?? _destConnStr;

                    if (connStrings.TryGetProperty("Source_MLS", out var mls))
                        _sources["MLS"].ConnectionString = mls.GetString() ?? _sources["MLS"].ConnectionString;

                    if (connStrings.TryGetProperty("Source_OPTIC", out var optic))
                        _sources["OPTIC"].ConnectionString = optic.GetString() ?? _sources["OPTIC"].ConnectionString;

                    if (connStrings.TryGetProperty("Source_AIS", out var ais))
                        _sources["AIS"].ConnectionString = ais.GetString() ?? _sources["AIS"].ConnectionString;
                }

                if (root.TryGetProperty("SyncSettings", out var syncSettings))
                {
                    if (syncSettings.TryGetProperty("DailyScheduleHour", out var h))
                        _scheduleHour = h.GetInt32();
                    if (syncSettings.TryGetProperty("DailyScheduleMinute", out var m))
                        _scheduleMinute = m.GetInt32();
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Warning: Could not parse appsettings.json: {ex.Message}");
        }
    }

    private void InitializeSystemTray()
    {
        _trayMenu = new ContextMenuStrip();
        _trayMenu.Items.Add("Open Window", null, (s, e) => RestoreFromTray());
        _trayMenu.Items.Add("Run Sync Now", null, async (s, e) => await ExecuteSyncAsync());
        _trayMenu.Items.Add(new ToolStripSeparator());
        _trayMenu.Items.Add("Exit", null, (s, e) => { _allowExit = true; Application.Exit(); });

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            ContextMenuStrip = _trayMenu,
            Text = "CI Yield - Daily Data Sync Hub",
            Visible = true
        };
        _notifyIcon.DoubleClick += (s, e) => RestoreFromTray();

        this.Resize += (s, e) =>
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                _notifyIcon.ShowBalloonTip(2000, "CI Yield Sync Hub", "Minimized to system tray. Double-click icon to open.", ToolTipIcon.Info);
            }
        };

        this.FormClosing += (s, e) =>
        {
            if (!_allowExit)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
                this.Hide();
                _notifyIcon.ShowBalloonTip(2000, "CI Yield Sync Hub", "Running in background to execute scheduled daily sync. Right-click tray icon to Exit.", ToolTipIcon.Info);
            }
        };
    }

    private void RestoreFromTray()
    {
        this.Show();
        this.WindowState = FormWindowState.Normal;
        this.BringToFront();
    }

    private void InitializeScheduler()
    {
        _scheduleTimer = new System.Windows.Forms.Timer
        {
            Interval = 30000 // Check every 30 seconds
        };
        _scheduleTimer.Tick += async (s, e) =>
        {
            if (!chkAutoSchedule.Checked) return;

            DateTime now = DateTime.Now;
            if (now.Hour == _scheduleHour && now.Minute == _scheduleMinute && _lastAutoSyncDate.Date != now.Date)
            {
                _lastAutoSyncDate = now.Date;
                LogMessage($"⏰ Automated Daily Schedule triggered at {now:yyyy-MM-dd HH:mm:ss}");
                await ExecuteSyncAsync();
            }
        };
        _scheduleTimer.Start();
    }

    private void DateFilter_Changed(object? sender, EventArgs e)
    {
        dtpFrom.Enabled = rbCustom.Checked;
        dtpTo.Enabled = rbCustom.Checked;
    }

    private void chkAutoSchedule_CheckedChanged(object? sender, EventArgs e)
    {
        lblStatus.Text = chkAutoSchedule.Checked
            ? $"Ready. Next auto-sync scheduled daily at {_scheduleHour:D2}:{_scheduleMinute:D2} AM."
            : "Ready. Automated daily schedule is paused.";
    }

    private void btnClearLog_Click(object? sender, EventArgs e)
    {
        txtLog.Clear();
    }

    private async void btnTestConnections_Click(object? sender, EventArgs e)
    {
        btnTestConnections.Enabled = false;
        btnSync.Enabled = false;
        LogMessage("\n--- Testing All Database Connections ---");

        var syncService = new SyncService(_destConnStr, LogMessage);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(35));

        // Test destination
        await syncService.TestConnectionAsync(_destConnStr, "Destination: BTMSQL01\\DIGITMONT_DEV.CI_LSS", cts.Token);

        // Test sources
        if (chkMLS.Checked)
            await syncService.TestConnectionAsync(_sources["MLS"].ConnectionString, "Source: MLS (digimon_lighting)", cts.Token);

        if (chkOptic.Checked)
            await syncService.TestConnectionAsync(_sources["OPTIC"].ConnectionString, "Source: OPTIC (digimon_vision)", cts.Token);

        if (chkAIS.Checked)
            await syncService.TestConnectionAsync(_sources["AIS"].ConnectionString, "Source: AIS (dbDigitmontDetection)", cts.Token);

        LogMessage("--- Test Completed ---\n");
        btnTestConnections.Enabled = true;
        btnSync.Enabled = true;
    }

    private async void btnSync_Click(object? sender, EventArgs e)
    {
        await ExecuteSyncAsync();
    }

    private async Task ExecuteSyncAsync()
    {
        btnSync.Enabled = false;
        btnTestConnections.Enabled = false;
        btnSync.Text = "⏳ SYNCING IN PROGRESS...";
        btnSync.BackColor = Color.FromArgb(230, 126, 34); // Orange
        lblStatus.Text = "Synchronizing daily yield data across plants...";

        var (startDate, endDate) = GetSelectedDateRange();
        LogMessage("\n============================================================");
        LogMessage($"[START SYNC] {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        LogMessage($"Window: {(startDate.HasValue ? $"{startDate.Value:yyyy-MM-dd} to {endDate?.ToString("yyyy-MM-dd") ?? "Now"}" : "All Time Historical")}");

        var syncService = new SyncService(_destConnStr, LogMessage);
        using var cts = new CancellationTokenSource();

        try
        {
            // 1. Ensure target table [dbo].[CI_Yield] exists
            await syncService.EnsureDestinationTableExistsAsync(cts.Token);

            var targets = new List<SyncSourceConfig>();
            if (chkMLS.Checked) targets.Add(_sources["MLS"]);
            if (chkOptic.Checked) targets.Add(_sources["OPTIC"]);
            if (chkAIS.Checked) targets.Add(_sources["AIS"]);

            int totalExtracted = 0;
            int totalUpserted = 0;

            foreach (var src in targets)
            {
                var res = await syncService.SyncSourceAsync(src, startDate, endDate, cts.Token);
                totalExtracted += res.RecordsExtracted;
                totalUpserted += res.RecordsUpserted;
            }

            LogMessage("------------------------------------------------------------");
            LogMessage($"🎉 ALL SYNC TASKS COMPLETED!");
            LogMessage($"Total Daily Yield Records Extracted: {totalExtracted:N0}");
            LogMessage($"Total Records Merged to [CI_Yield]: {totalUpserted:N0}");
            LogMessage("============================================================\n");

            lblStatus.Text = $"Last sync finished successfully at {DateTime.Now:HH:mm:ss}. Merged: {totalUpserted:N0} records.";
            btnSync.BackColor = Color.FromArgb(46, 204, 113); // Green
            btnSync.Text = "✔️ SYNC COMPLETED";
        }
        catch (Exception ex)
        {
            LogMessage($"❌ Fatal error during synchronization: {ex.Message}");
            lblStatus.Text = $"Sync failed: {ex.Message}";
            btnSync.BackColor = Color.FromArgb(231, 76, 60); // Red
            btnSync.Text = "⚠️ SYNC FAILED";
        }
        finally
        {
            btnSync.Enabled = true;
            btnTestConnections.Enabled = true;
            await Task.Delay(3000);
            btnSync.BackColor = Color.FromArgb(41, 128, 185);
            btnSync.Text = "🚀 START SYNC NOW";
        }
    }

    private (DateTime? startDate, DateTime? endDate) GetSelectedDateRange()
    {
        DateTime today = DateTime.Today;

        if (rbToday.Checked)
            return (today, today);

        if (rbYesterday.Checked)
        {
            DateTime yesterday = today.AddDays(-1);
            return (yesterday, yesterday);
        }

        if (rbLast7Days.Checked)
            return (today.AddDays(-7), today);

        if (rbLast30Days.Checked)
            return (today.AddDays(-30), today);

        if (rbAllTime.Checked)
            return (null, null);

        if (rbCustom.Checked)
            return (dtpFrom.Value.Date, dtpTo.Value.Date);

        return (today.AddDays(-7), today);
    }

    public void LogMessage(string message)
    {
        if (txtLog.InvokeRequired)
        {
            txtLog.Invoke(new Action(() => LogMessage(message)));
        }
        else
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        }
    }
}

