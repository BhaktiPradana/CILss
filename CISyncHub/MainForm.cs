using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CISyncHub.Models;
using CISyncHub.Services;

namespace CISyncHub;

public partial class MainForm : Form
{
    private string _destConnStr = "";
    private string _sourceMLS = "";
    private string _sourceOptic = "";
    private string _sourceAIS = "";
    private string _sourceBTMTime = "";

    private bool _scheduleEnabled = true;
    private int _scheduleHour = 6;
    private int _scheduleMinute = 0;
    private DateTime? _lastScheduledRunDate = null;
    private System.Windows.Forms.Timer? _scheduleTimer;

    private NotifyIcon? _trayIcon;
    private ContextMenuStrip? _trayMenu;
    private bool _isSyncing = false;

    private YieldSyncService? _yieldService;
    private EmployeeSyncService? _empService;
    private IECycleTimeSyncService? _ieCycleService;
    private MachinesProcessSyncService? _machProcessService;
    private ProcessQuantitySyncService? _processQtyService;
    private ConnectionDiagnosticService? _diagService;

    public MainForm()
    {
        InitializeComponent();
        InitializeCustomComponents();
        LoadSettings();
        WireEvents();
    }

    private void InitializeCustomComponents()
    {
        // Setup NotifyIcon for background system tray support
        _trayMenu = new ContextMenuStrip();
        _trayMenu.Items.Add("Open CI Sync Hub", null, (s, e) => RestoreFromTray());
        _trayMenu.Items.Add(new ToolStripSeparator());
        _trayMenu.Items.Add("Sync All Now (Yield, Employee, Cycle Time & Process Qty)", null, async (s, e) => await RunSyncAllAsync());
        _trayMenu.Items.Add("Sync Yield Only", null, async (s, e) => await RunYieldSyncAsync());
        _trayMenu.Items.Add("Sync Employees Only", null, async (s, e) => await RunEmployeeSyncAsync());
        _trayMenu.Items.Add("Sync IE Cycle Time Only", null, async (s, e) => await RunIECycleSyncAsync());
        _trayMenu.Items.Add("Sync Process Quantity (QtyIn) Only", null, async (s, e) => await RunProcessQuantitySyncAsync());
        _trayMenu.Items.Add(new ToolStripSeparator());
        _trayMenu.Items.Add("Exit", null, (s, e) => ForceExit());

        _trayIcon = new NotifyIcon(this.components)
        {
            Icon = SystemIcons.Application,
            Text = "CI Sync Hub (Excelitas Batam)",
            ContextMenuStrip = _trayMenu,
            Visible = true
        };
        _trayIcon.DoubleClick += (s, e) => RestoreFromTray();

        // Setup scheduler check timer (checks every 30 seconds)
        _scheduleTimer = new System.Windows.Forms.Timer(this.components)
        {
            Interval = 30000
        };
        _scheduleTimer.Tick += OnScheduleTimerTick;
        _scheduleTimer.Start();
    }

    private void LoadSettings()
    {
        try
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            if (!File.Exists(configPath))
            {
                configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            }

            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("ConnectionStrings", out var conns))
                {
                    if (conns.TryGetProperty("Destination", out var pDest)) _destConnStr = pDest.GetString() ?? "";
                    if (conns.TryGetProperty("Source_MLS", out var pMls)) _sourceMLS = pMls.GetString() ?? "";
                    if (conns.TryGetProperty("Source_OPTIC", out var pOpt)) _sourceOptic = pOpt.GetString() ?? "";
                    if (conns.TryGetProperty("Source_AIS", out var pAis)) _sourceAIS = pAis.GetString() ?? "";
                    if (conns.TryGetProperty("Source_BTMTime", out var pHr)) _sourceBTMTime = pHr.GetString() ?? "";
                }

                if (root.TryGetProperty("SyncSettings", out var sync))
                {
                    if (sync.TryGetProperty("EnableDailySchedule", out var pEn)) _scheduleEnabled = pEn.GetBoolean();
                    if (sync.TryGetProperty("DailyScheduleHour", out var pHr)) _scheduleHour = pHr.GetInt32();
                    if (sync.TryGetProperty("DailyScheduleMinute", out var pMin)) _scheduleMinute = pMin.GetInt32();
                }

                LogMessage("Configuration loaded successfully from appsettings.json.");
            }
            else
            {
                LogMessage("⚠️ appsettings.json not found! Using hardcoded fallback connection strings.");
                _destConnStr = "Server=BTMSQL01\\DIGITMONT_DEV;Database=CI_LSS;User Id=CI_Hub_User;Password=CiHubB4tam@excelitas;TrustServerCertificate=True;";
                _sourceMLS = "Server=ETCBTMA03\\DIGITMONT;Database=digimon_lighting;Integrated Security=SSPI;TrustServerCertificate=True;";
                _sourceOptic = "Server=ETCBTMA03\\DIGITMONT;Database=digimon_vision;Integrated Security=SSPI;TrustServerCertificate=True;";
                _sourceAIS = "Server=ETCBTMSQL01\\DIGITMONT;Database=dbDigitmontDetection;Integrated Security=SSPI;TrustServerCertificate=True;";
                _sourceBTMTime = "Server=ETCRESSQL02;Database=BTMTime;User Id=BTM_IT_RO;Password=ITreadonly!;TrustServerCertificate=True;";
            }

            _yieldService = new YieldSyncService(_destConnStr, LogMessage);
            _empService = new EmployeeSyncService(_destConnStr, LogMessage);
            _ieCycleService = new IECycleTimeSyncService(_destConnStr, LogMessage);
            _machProcessService = new MachinesProcessSyncService(_destConnStr, LogMessage);
            _processQtyService = new ProcessQuantitySyncService(_destConnStr, LogMessage);
            _diagService = new ConnectionDiagnosticService(LogMessage);

            // Update UI fields
            nudHour.Value = Math.Clamp(_scheduleHour, 0, 23);
            nudMinute.Value = Math.Clamp(_scheduleMinute, 0, 59);
            chkScheduleEnabled.Checked = _scheduleEnabled;
            chkAutoScheduleDash.Checked = _scheduleEnabled;
            UpdateScheduleCardStatus();

            dtpFrom.Value = DateTime.Today.AddDays(-7);
            dtpTo.Value = DateTime.Today;
            dtpFromPQ.Value = DateTime.Today.AddDays(-7);
            dtpToPQ.Value = DateTime.Today;
        }
        catch (Exception ex)
        {
            LogMessage($"❌ Error loading settings: {ex.Message}");
        }
    }

    private void WireEvents()
    {
        // Dashboard
        btnSyncAll.Click += async (s, e) => await RunSyncAllAsync();
        btnDashSyncYield.Click += async (s, e) => await RunYieldSyncAsync();
        btnDashSyncEmployee.Click += async (s, e) => await RunEmployeeSyncAsync();
        btnDashSyncIECycle.Click += async (s, e) => await RunIECycleSyncAsync();
        btnDashSyncProcessQty.Click += async (s, e) => await RunProcessQuantitySyncAsync();
        chkAutoScheduleDash.CheckedChanged += (s, e) =>
        {
            chkScheduleEnabled.Checked = chkAutoScheduleDash.Checked;
            _scheduleEnabled = chkAutoScheduleDash.Checked;
            UpdateScheduleCardStatus();
        };

        // Yield Tab
        btnSyncYield.Click += async (s, e) => await RunYieldSyncAsync();
        btnVerifyYieldTable.Click += async (s, e) => await VerifyYieldTableAsync();
        rbToday.CheckedChanged += (s, e) => OnDatePresetChanged();
        rbYesterday.CheckedChanged += (s, e) => OnDatePresetChanged();
        rbLast7Days.CheckedChanged += (s, e) => OnDatePresetChanged();
        rbLast30Days.CheckedChanged += (s, e) => OnDatePresetChanged();
        rbAllTime.CheckedChanged += (s, e) => OnDatePresetChanged();
        rbCustom.CheckedChanged += (s, e) => OnDatePresetChanged();

        // Employee Tab
        btnSyncEmployee.Click += async (s, e) => await RunEmployeeSyncAsync();

        // IE Cycle Time Tab
        btnSyncIECycle.Click += async (s, e) => await RunIECycleSyncAsync();
        btnVerifyIECycleTable.Click += async (s, e) => await VerifyIECycleTableAsync();

        // Process Quantity Tab
        btnSyncProcessQty.Click += async (s, e) => await RunProcessQuantitySyncAsync();
        btnVerifyProcessQtyTable.Click += async (s, e) => await VerifyProcessQuantityTableAsync();
        rbTodayPQ.CheckedChanged += (s, e) => OnDatePresetPQChanged();
        rbYesterdayPQ.CheckedChanged += (s, e) => OnDatePresetPQChanged();
        rbLast7DaysPQ.CheckedChanged += (s, e) => OnDatePresetPQChanged();
        rbLast30DaysPQ.CheckedChanged += (s, e) => OnDatePresetPQChanged();
        rbAllTimePQ.CheckedChanged += (s, e) => OnDatePresetPQChanged();
        rbCustomPQ.CheckedChanged += (s, e) => OnDatePresetPQChanged();

        // Diagnostics Tab
        btnTestConnections.Click += async (s, e) => await RunDiagnosticsAsync();
        chkScheduleEnabled.CheckedChanged += (s, e) =>
        {
            chkAutoScheduleDash.Checked = chkScheduleEnabled.Checked;
            _scheduleEnabled = chkScheduleEnabled.Checked;
            UpdateScheduleCardStatus();
        };
        nudHour.ValueChanged += (s, e) => { _scheduleHour = (int)nudHour.Value; UpdateScheduleCardStatus(); };
        nudMinute.ValueChanged += (s, e) => { _scheduleMinute = (int)nudMinute.Value; UpdateScheduleCardStatus(); };

        // Log buttons
        btnClearLog.Click += (s, e) => txtLog.Clear();
        btnCopyLog.Click += (s, e) =>
        {
            if (!string.IsNullOrEmpty(txtLog.Text))
            {
                Clipboard.SetText(txtLog.Text);
                MessageBox.Show("Execution logs copied to clipboard.", "CI Sync Hub", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        };
        btnSaveLog.Click += (s, e) => SaveLogToFile();

        // Form events
        this.Resize += (s, e) =>
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                _trayIcon?.ShowBalloonTip(2000, "CI Sync Hub", "Application is running in the background tray.", ToolTipIcon.Info);
            }
        };
    }

    private void OnDatePresetChanged()
    {
        if (rbToday.Checked)
        {
            dtpFrom.Value = DateTime.Today;
            dtpTo.Value = DateTime.Today;
            dtpFrom.Enabled = false;
            dtpTo.Enabled = false;
        }
        else if (rbYesterday.Checked)
        {
            dtpFrom.Value = DateTime.Today.AddDays(-1);
            dtpTo.Value = DateTime.Today.AddDays(-1);
            dtpFrom.Enabled = false;
            dtpTo.Enabled = false;
        }
        else if (rbLast7Days.Checked)
        {
            dtpFrom.Value = DateTime.Today.AddDays(-7);
            dtpTo.Value = DateTime.Today;
            dtpFrom.Enabled = false;
            dtpTo.Enabled = false;
        }
        else if (rbLast30Days.Checked)
        {
            dtpFrom.Value = DateTime.Today.AddDays(-30);
            dtpTo.Value = DateTime.Today;
            dtpFrom.Enabled = false;
            dtpTo.Enabled = false;
        }
        else if (rbAllTime.Checked)
        {
            dtpFrom.Enabled = false;
            dtpTo.Enabled = false;
        }
        else if (rbCustom.Checked)
        {
            dtpFrom.Enabled = true;
            dtpTo.Enabled = true;
        }
    }

    private void OnDatePresetPQChanged()
    {
        if (rbTodayPQ.Checked)
        {
            dtpFromPQ.Value = DateTime.Today;
            dtpToPQ.Value = DateTime.Today;
            dtpFromPQ.Enabled = false;
            dtpToPQ.Enabled = false;
        }
        else if (rbYesterdayPQ.Checked)
        {
            dtpFromPQ.Value = DateTime.Today.AddDays(-1);
            dtpToPQ.Value = DateTime.Today.AddDays(-1);
            dtpFromPQ.Enabled = false;
            dtpToPQ.Enabled = false;
        }
        else if (rbLast7DaysPQ.Checked)
        {
            dtpFromPQ.Value = DateTime.Today.AddDays(-7);
            dtpToPQ.Value = DateTime.Today;
            dtpFromPQ.Enabled = false;
            dtpToPQ.Enabled = false;
        }
        else if (rbLast30DaysPQ.Checked)
        {
            dtpFromPQ.Value = DateTime.Today.AddDays(-30);
            dtpToPQ.Value = DateTime.Today;
            dtpFromPQ.Enabled = false;
            dtpToPQ.Enabled = false;
        }
        else if (rbAllTimePQ.Checked)
        {
            dtpFromPQ.Enabled = false;
            dtpToPQ.Enabled = false;
        }
        else if (rbCustomPQ.Checked)
        {
            dtpFromPQ.Enabled = true;
            dtpToPQ.Enabled = true;
        }
    }

    private void UpdateScheduleCardStatus()
    {
        if (_scheduleEnabled)
        {
            lblCardScheduleStatus.Text = $"Active (Daily @ {_scheduleHour:D2}:{_scheduleMinute:D2})";
            lblCardScheduleStatus.ForeColor = Color.FromArgb(46, 204, 113);
        }
        else
        {
            lblCardScheduleStatus.Text = "Disabled";
            lblCardScheduleStatus.ForeColor = Color.FromArgb(231, 76, 60);
        }
    }

    private async void OnScheduleTimerTick(object? sender, EventArgs e)
    {
        if (!_scheduleEnabled || _isSyncing) return;

        var now = DateTime.Now;
        if (now.Hour == _scheduleHour && now.Minute == _scheduleMinute)
        {
            if (_lastScheduledRunDate.HasValue && _lastScheduledRunDate.Value.Date == now.Date)
            {
                return; // Already executed today
            }

            _lastScheduledRunDate = now;
            LogMessage($"⏰ Daily schedule trigger fired at {now:yyyy-MM-dd HH:mm:ss}!");
            _trayIcon?.ShowBalloonTip(3000, "CI Sync Hub", "Daily scheduled sync is executing...", ToolTipIcon.Info);

            await RunSyncAllAsync();
        }
    }

    public async Task RunSyncAllAsync()
    {
        if (_isSyncing)
        {
            MessageBox.Show("A synchronization process is already running.", "Operation In Progress", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SetSyncingState(true, "Synchronizing All Data (Employees + Daily Yield)...");

        try
        {
            LogMessage("============================================================");
            LogMessage($"[START FULL SYNC] Triggered at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            LogMessage("============================================================");

            // Step 1: Employee Master Sync
            await RunEmployeeSyncInternalAsync();

            // Step 2: Yield Sync (Default Last 7 Days for safety)
            await RunYieldSyncInternalAsync(DateTime.Today.AddDays(-7), DateTime.Today, true, true, true);

            // Step 3: IE Cycle Time Sync (MLS, OPTIC, AIS)
            await RunIECycleSyncInternalAsync(true, true, true);

            // Step 4: Process Quantity Sync (MLS, OPTIC, AIS: Machine QtyIn)
            await RunProcessQuantitySyncInternalAsync(DateTime.Today.AddDays(-7), DateTime.Today, true, true, true);

            LogMessage("============================================================");
            LogMessage($"🎉 [FULL SYNC COMPLETE] All operations finished at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            LogMessage("============================================================");

            _trayIcon?.ShowBalloonTip(3000, "CI Sync Hub", "Full synchronization completed successfully!", ToolTipIcon.Info);
        }
        catch (Exception ex)
        {
            LogMessage($"❌ Full Sync failed: {ex.Message}");
        }
        finally
        {
            SetSyncingState(false, "Ready. Full synchronization finished.");
        }
    }

    public async Task RunYieldSyncAsync()
    {
        if (_isSyncing)
        {
            MessageBox.Show("A synchronization process is already running.", "Operation In Progress", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DateTime? start = null;
        DateTime? end = null;

        if (rbToday.Checked)
        {
            start = DateTime.Today;
            end = DateTime.Today;
        }
        else if (rbYesterday.Checked)
        {
            start = DateTime.Today.AddDays(-1);
            end = DateTime.Today.AddDays(-1);
        }
        else if (rbLast7Days.Checked)
        {
            start = DateTime.Today.AddDays(-7);
            end = DateTime.Today;
        }
        else if (rbLast30Days.Checked)
        {
            start = DateTime.Today.AddDays(-30);
            end = DateTime.Today;
        }
        else if (rbCustom.Checked)
        {
            start = dtpFrom.Value.Date;
            end = dtpTo.Value.Date;
        }

        bool syncMls = chkMLS.Checked;
        bool syncOptic = chkOptic.Checked;
        bool syncAis = chkAIS.Checked;

        if (!syncMls && !syncOptic && !syncAis)
        {
            MessageBox.Show("Please select at least one source division (MLS, OPTIC, or AIS).", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SetSyncingState(true, "Synchronizing Daily Yield...");
        try
        {
            await RunYieldSyncInternalAsync(start, end, syncMls, syncOptic, syncAis);
        }
        finally
        {
            SetSyncingState(false, "Ready. Yield sync finished.");
        }
    }

    private async Task RunYieldSyncInternalAsync(DateTime? start, DateTime? end, bool syncMls, bool syncOptic, bool syncAis)
    {
        if (_yieldService == null) return;

        var sources = new List<SyncSourceConfig>();

        if (syncMls)
        {
            sources.Add(new SyncSourceConfig
            {
                Key = "MLS",
                Name = "Digitmont MLS",
                Division = "MLS",
                ConnectionString = _sourceMLS,
                ServerName = @"ETCBTMA03\DIGITMONT",
                DatabaseName = "digimon_lighting"
            });
        }

        if (syncOptic)
        {
            sources.Add(new SyncSourceConfig
            {
                Key = "OPTIC",
                Name = "Digitmont OPTIC",
                Division = "OPTIC",
                ConnectionString = _sourceOptic,
                ServerName = @"ETCBTMA03\DIGITMONT",
                DatabaseName = "digimon_vision"
            });
        }

        if (syncAis)
        {
            sources.Add(new SyncSourceConfig
            {
                Key = "AIS",
                Name = "Digitmont AIS",
                Division = "AIS",
                ConnectionString = _sourceAIS,
                ServerName = @"ETCBTMSQL01\DIGITMONT",
                DatabaseName = "dbDigitmontDetection"
            });
        }

        int totalExtracted = 0;
        int totalUpserted = 0;
        int successCount = 0;

        await _yieldService.EnsureDestinationTableExistsAsync();

        foreach (var source in sources)
        {
            var res = await _yieldService.SyncSourceAsync(source, start, end);
            if (res.Success)
            {
                successCount++;
                totalExtracted += res.RecordsExtracted;
                totalUpserted += res.RecordsUpserted;
            }
        }

        lblCardYieldStatus.Text = $"Last Sync: {DateTime.Now:HH:mm:ss} ({totalUpserted:N0} records)";
        lblCardYieldStatus.ForeColor = successCount == sources.Count ? Color.FromArgb(46, 204, 113) : Color.FromArgb(230, 126, 34);

        LogMessage($"Yield Sync Summary: {successCount}/{sources.Count} divisions completed. Extracted {totalExtracted:N0}, Upserted {totalUpserted:N0}.");
    }

    public async Task RunEmployeeSyncAsync()
    {
        if (_isSyncing)
        {
            MessageBox.Show("A synchronization process is already running.", "Operation In Progress", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SetSyncingState(true, "Synchronizing Employee Master...");
        try
        {
            await RunEmployeeSyncInternalAsync();
        }
        finally
        {
            SetSyncingState(false, "Ready. Employee sync finished.");
        }
    }

    private async Task RunEmployeeSyncInternalAsync()
    {
        if (_empService == null) return;

        await _empService.EnsureDestinationTableExistsAsync();
        var res = await _empService.SyncEmployeesAsync(_sourceBTMTime);

        if (res.Success)
        {
            lblCardEmpStatus.Text = $"Last Sync: {DateTime.Now:HH:mm:ss} ({res.TotalEmployeesSynced:N0} emps)";
            lblCardEmpStatus.ForeColor = Color.FromArgb(46, 204, 113);
        }
        else
        {
            lblCardEmpStatus.Text = "Last Sync: Failed";
            lblCardEmpStatus.ForeColor = Color.FromArgb(231, 76, 60);
        }
    }

    public async Task RunIECycleSyncAsync()
    {
        if (_isSyncing)
        {
            MessageBox.Show("A synchronization process is already running.", "Operation In Progress", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        bool syncMls = chkMlsIE.Checked;
        bool syncOptic = chkOpticIE.Checked;
        bool syncAis = chkAisIE.Checked;

        if (!syncMls && !syncOptic && !syncAis)
        {
            MessageBox.Show("Please select at least one division (MLS, OPTIC, or AIS) to sync IE Cycle Time.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        SetSyncingState(true, "Synchronizing IE Cycle Time...");
        try
        {
            await RunIECycleSyncInternalAsync(syncMls, syncOptic, syncAis);
            _trayIcon?.ShowBalloonTip(3000, "CI Sync Hub", "IE Cycle Time sync finished successfully!", ToolTipIcon.Info);
        }
        catch (Exception ex)
        {
            LogMessage($"❌ IE Cycle Time Sync failed: {ex.Message}");
        }
        finally
        {
            SetSyncingState(false, "Ready. IE Cycle Time synchronization finished.");
        }
    }

    private async Task RunIECycleSyncInternalAsync(bool syncMls, bool syncOptic, bool syncAis)
    {
        if (_ieCycleService == null)
            _ieCycleService = new IECycleTimeSyncService(_destConnStr, LogMessage);
        if (_machProcessService == null)
            _machProcessService = new MachinesProcessSyncService(_destConnStr, LogMessage);

        LogMessage("------------------------------------------------------------");
        LogMessage($"[START] IE Cycle Time & Machine Matrix Sync starting at {DateTime.Now:yyyy-MM-dd HH:mm:ss}...");
        LogMessage("------------------------------------------------------------");

        // Ensure destination tables exist & schema is verified
        await _ieCycleService.EnsureDestinationTableExistsAsync();
        await _machProcessService.EnsureDestinationTableExistsAsync();

        var sources = new List<SyncSourceConfig>();

        if (syncMls && !string.IsNullOrWhiteSpace(_sourceMLS))
        {
            sources.Add(new SyncSourceConfig
            {
                Key = "MLS",
                Name = "MLS Lighting",
                Division = "MLS",
                ConnectionString = _sourceMLS,
                ServerName = "ETCBTMA03\\DIGITMONT",
                DatabaseName = "digimon_lighting"
            });
        }

        if (syncOptic && !string.IsNullOrWhiteSpace(_sourceOptic))
        {
            sources.Add(new SyncSourceConfig
            {
                Key = "OPTIC",
                Name = "OPTIC / VISION",
                Division = "OPTIC",
                ConnectionString = _sourceOptic,
                ServerName = "ETCBTMA03\\DIGITMONT",
                DatabaseName = "digimon_vision"
            });
        }

        if (syncAis && !string.IsNullOrWhiteSpace(_sourceAIS))
        {
            sources.Add(new SyncSourceConfig
            {
                Key = "AIS",
                Name = "AIS Detection",
                Division = "AIS",
                ConnectionString = _sourceAIS,
                ServerName = "ETCBTMSQL01\\DIGITMONT",
                DatabaseName = "dbDigitmontDetection"
            });
        }

        int totalExtractedCycle = 0;
        int totalUpsertedCycle = 0;
        int totalExtractedMach = 0;
        int totalUpsertedMach = 0;

        foreach (var src in sources)
        {
            var resCycle = await _ieCycleService.SyncSourceAsync(src);
            if (resCycle.Success)
            {
                totalExtractedCycle += resCycle.RecordsExtracted;
                totalUpsertedCycle += resCycle.RecordsUpserted;
            }

            var resMach = await _machProcessService.SyncSourceAsync(src);
            if (resMach.Success)
            {
                totalExtractedMach += resMach.RecordsExtracted;
                totalUpsertedMach += resMach.RecordsUpserted;
            }
        }

        LogMessage("------------------------------------------------------------");
        LogMessage($"[SUMMARY] IE Cycle Time Sync: {totalExtractedCycle:N0} extracted, {totalUpsertedCycle:N0} upserted.");
        LogMessage($"[SUMMARY] Machine Matrix (dmMachinesProcess): {totalExtractedMach:N0} extracted, {totalUpsertedMach:N0} upserted.");
        LogMessage("------------------------------------------------------------");
    }

    public async Task VerifyIECycleTableAsync()
    {
        try
        {
            if (_ieCycleService == null)
                _ieCycleService = new IECycleTimeSyncService(_destConnStr, LogMessage);
            if (_machProcessService == null)
                _machProcessService = new MachinesProcessSyncService(_destConnStr, LogMessage);

            await _ieCycleService.EnsureDestinationTableExistsAsync();
            await _machProcessService.EnsureDestinationTableExistsAsync();
            MessageBox.Show("Tables [dbo].[CI_IECycleTime_Header] and [dbo].[CI_MachinesProcess] in destination database CI_LSS are verified and ready.", "Tables Verified", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            LogMessage($"❌ Table verification failed: {ex.Message}");
            MessageBox.Show($"Verification failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public async Task VerifyYieldTableAsync()
    {
        if (_yieldService == null) return;
        try
        {
            await _yieldService.EnsureDestinationTableExistsAsync();
            MessageBox.Show("Table [dbo].[CI_Yield] and associated indexes are verified successfully.", "Verification", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Verification failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public async Task RunProcessQuantitySyncAsync()
    {
        if (_isSyncing)
        {
            MessageBox.Show("A synchronization process is already running.", "Operation In Progress", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DateTime? start = null;
        DateTime? end = null;

        if (rbTodayPQ.Checked)
        {
            start = DateTime.Today;
            end = DateTime.Today;
        }
        else if (rbYesterdayPQ.Checked)
        {
            start = DateTime.Today.AddDays(-1);
            end = DateTime.Today.AddDays(-1);
        }
        else if (rbLast7DaysPQ.Checked)
        {
            start = DateTime.Today.AddDays(-7);
            end = DateTime.Today;
        }
        else if (rbLast30DaysPQ.Checked)
        {
            start = DateTime.Today.AddDays(-30);
            end = DateTime.Today;
        }
        else if (rbAllTimePQ.Checked)
        {
            start = null;
            end = null;
        }
        else if (rbCustomPQ.Checked)
        {
            start = dtpFromPQ.Value.Date;
            end = dtpToPQ.Value.Date;
        }

        bool syncMls = chkMlsPQ.Checked;
        bool syncOptic = chkOpticPQ.Checked;
        bool syncAis = chkAisPQ.Checked;

        if (!syncMls && !syncOptic && !syncAis)
        {
            MessageBox.Show("Please select at least one source division (MLS, OPTIC, or AIS).", "No Source Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SetSyncingState(true, "Synchronizing Process Quantity (Machine QtyIn)...");

        try
        {
            await RunProcessQuantitySyncInternalAsync(start, end, syncMls, syncOptic, syncAis);
            MessageBox.Show("Process Quantity (Machine QtyIn) synchronization completed successfully!", "Sync Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            LogMessage($"❌ Process Quantity synchronization failed: {ex.Message}");
            MessageBox.Show($"Synchronization error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetSyncingState(false, "Ready. Process Quantity sync finished.");
        }
    }

    private async Task RunProcessQuantitySyncInternalAsync(DateTime? start, DateTime? end, bool syncMls, bool syncOptic, bool syncAis)
    {
        if (_processQtyService == null)
            _processQtyService = new ProcessQuantitySyncService(_destConnStr, LogMessage);

        await _processQtyService.EnsureDestinationTableExistsAsync();

        var sources = new List<SyncSourceConfig>();

        if (syncMls && !string.IsNullOrWhiteSpace(_sourceMLS))
        {
            sources.Add(new SyncSourceConfig
            {
                Key = "MLS",
                Name = "MLS Lighting",
                Division = "MLS",
                ConnectionString = _sourceMLS,
                ServerName = "ETCBTMA03\\DIGITMONT",
                DatabaseName = "digimon_lighting"
            });
        }

        if (syncOptic && !string.IsNullOrWhiteSpace(_sourceOptic))
        {
            sources.Add(new SyncSourceConfig
            {
                Key = "OPTIC",
                Name = "OPTIC / VISION",
                Division = "OPTIC",
                ConnectionString = _sourceOptic,
                ServerName = "ETCBTMA03\\DIGITMONT",
                DatabaseName = "digimon_vision"
            });
        }

        if (syncAis && !string.IsNullOrWhiteSpace(_sourceAIS))
        {
            sources.Add(new SyncSourceConfig
            {
                Key = "AIS",
                Name = "AIS Detection",
                Division = "AIS",
                ConnectionString = _sourceAIS,
                ServerName = "ETCBTMSQL01\\DIGITMONT",
                DatabaseName = "dbDigitmontDetection"
            });
        }

        int totalExtracted = 0;
        int totalUpserted = 0;

        foreach (var src in sources)
        {
            var res = await _processQtyService.SyncSourceAsync(src, start, end);
            if (res.Success)
            {
                totalExtracted += res.RecordsExtracted;
                totalUpserted += res.RecordsUpserted;
            }
        }

        LogMessage("------------------------------------------------------------");
        LogMessage($"[SUMMARY] Process Quantity (Machine QtyIn) Sync: {totalExtracted:N0} extracted, {totalUpserted:N0} upserted across all selected sources.");
        LogMessage("------------------------------------------------------------");
    }

    public async Task VerifyProcessQuantityTableAsync()
    {
        try
        {
            if (_processQtyService == null)
                _processQtyService = new ProcessQuantitySyncService(_destConnStr, LogMessage);

            await _processQtyService.EnsureDestinationTableExistsAsync();
            MessageBox.Show("Table [dbo].[ProcessQuantity] and associated indexes in destination database CI_LSS are verified and ready.", "Table Verified", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            LogMessage($"❌ ProcessQuantity table verification failed: {ex.Message}");
            MessageBox.Show($"Verification failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public async Task RunDiagnosticsAsync()
    {
        if (_diagService == null) return;

        SetSyncingState(true, "Running Database Endpoint Diagnostics...");
        try
        {
            LogMessage("============================================================");
            LogMessage("[DIAGNOSTIC TEST] Testing connectivity to all 5 database endpoints...");
            LogMessage("============================================================");

            bool dest = await _diagService.TestConnectionAsync(_destConnStr, "Destination (CI_LSS)");
            bool mls = await _diagService.TestConnectionAsync(_sourceMLS, "Source MLS (digimon_lighting)");
            bool optic = await _diagService.TestConnectionAsync(_sourceOptic, "Source OPTIC (digimon_vision)");
            bool ais = await _diagService.TestConnectionAsync(_sourceAIS, "Source AIS (dbDigitmontDetection)");
            bool hr = await _diagService.TestConnectionAsync(_sourceBTMTime, "Source HR (BTMTime)");

            LogMessage("------------------------------------------------------------");
            LogMessage($"Connectivity Diagnostic Results: Destination={dest}, MLS={mls}, OPTIC={optic}, AIS={ais}, HR={hr}");
            LogMessage("============================================================");
        }
        finally
        {
            SetSyncingState(false, "Ready. Diagnostics complete.");
        }
    }

    private void SetSyncingState(bool syncing, string statusText)
    {
        _isSyncing = syncing;
        statusLabel.Text = statusText;

        btnSyncAll.Enabled = !syncing;
        btnDashSyncYield.Enabled = !syncing;
        btnDashSyncEmployee.Enabled = !syncing;
        btnDashSyncIECycle.Enabled = !syncing;
        btnDashSyncProcessQty.Enabled = !syncing;
        btnSyncYield.Enabled = !syncing;
        btnSyncEmployee.Enabled = !syncing;
        btnSyncIECycle.Enabled = !syncing;
        btnSyncProcessQty.Enabled = !syncing;
        btnTestConnections.Enabled = !syncing;
        btnVerifyYieldTable.Enabled = !syncing;
        btnVerifyIECycleTable.Enabled = !syncing;
        btnVerifyProcessQtyTable.Enabled = !syncing;

        this.Cursor = syncing ? Cursors.WaitCursor : Cursors.Default;
    }

    private void LogMessage(string message)
    {
        if (txtLog.IsDisposed) return;

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string formatted = $"[{timestamp}] {message}";

        if (txtLog.InvokeRequired)
        {
            try
            {
                txtLog.BeginInvoke(new Action(() => AppendLog(formatted)));
            }
            catch { }
        }
        else
        {
            AppendLog(formatted);
        }
    }

    private void AppendLog(string line)
    {
        txtLog.AppendText(line + Environment.NewLine);
        txtLog.SelectionStart = txtLog.Text.Length;
        txtLog.ScrollToCaret();
    }

    private void SaveLogToFile()
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "Text Files (*.log;*.txt)|*.log;*.txt|All Files (*.*)|*.*",
            FileName = $"CISyncHub_Log_{DateTime.Now:yyyyMMdd_HHmmss}.log"
        };
        if (sfd.ShowDialog() == DialogResult.OK)
        {
            File.WriteAllText(sfd.FileName, txtLog.Text);
            MessageBox.Show("Log successfully saved.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void RestoreFromTray()
    {
        this.Show();
        this.WindowState = FormWindowState.Normal;
        this.BringToFront();
        this.Activate();
    }

    private void ForceExit()
    {
        _scheduleTimer?.Stop();
        if (_trayIcon != null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }
        Application.Exit();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            // Prompt user whether to minimize to tray or quit
            var res = MessageBox.Show(
                "Do you want to minimize CI Sync Hub to the system tray to keep scheduled tasks running?\n\n• Yes: Minimize to background tray\n• No: Exit application completely\n• Cancel: Return to app",
                "CI Sync Hub",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (res == DialogResult.Yes)
            {
                e.Cancel = true;
                this.Hide();
                _trayIcon?.ShowBalloonTip(2000, "CI Sync Hub", "Application is minimized to tray.", ToolTipIcon.Info);
                return;
            }
            else if (res == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
        }

        _scheduleTimer?.Stop();
        if (_trayIcon != null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }
        base.OnFormClosing(e);
    }
}

