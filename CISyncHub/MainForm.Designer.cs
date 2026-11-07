namespace CISyncHub;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private System.Windows.Forms.Panel topHeaderPanel;
    private System.Windows.Forms.Label lblAppTitle;
    private System.Windows.Forms.Label lblAppSubtitle;
    private System.Windows.Forms.TabControl tabControl;
    private System.Windows.Forms.TabPage tabDashboard;
    private System.Windows.Forms.TabPage tabYield;
    private System.Windows.Forms.TabPage tabEmployee;
    private System.Windows.Forms.TabPage tabIECycleTime;
    private System.Windows.Forms.TabPage tabConnections;
    
    // Dashboard Tab Controls
    private System.Windows.Forms.Button btnSyncAll;
    private System.Windows.Forms.Button btnDashSyncYield;
    private System.Windows.Forms.Button btnDashSyncEmployee;
    private System.Windows.Forms.Button btnDashSyncIECycle;
    private System.Windows.Forms.Panel pnlCardYield;
    private System.Windows.Forms.Label lblCardYieldTitle;
    private System.Windows.Forms.Label lblCardYieldStatus;
    private System.Windows.Forms.Panel pnlCardEmployee;
    private System.Windows.Forms.Label lblCardEmpTitle;
    private System.Windows.Forms.Label lblCardEmpStatus;
    private System.Windows.Forms.Panel pnlCardSchedule;
    private System.Windows.Forms.Label lblCardScheduleTitle;
    private System.Windows.Forms.Label lblCardScheduleStatus;
    private System.Windows.Forms.CheckBox chkAutoScheduleDash;

    // Yield Tab Controls
    private System.Windows.Forms.GroupBox grpSources;
    private System.Windows.Forms.CheckBox chkMLS;
    private System.Windows.Forms.CheckBox chkOptic;
    private System.Windows.Forms.CheckBox chkAIS;
    private System.Windows.Forms.GroupBox grpDate;
    private System.Windows.Forms.RadioButton rbToday;
    private System.Windows.Forms.RadioButton rbYesterday;
    private System.Windows.Forms.RadioButton rbLast7Days;
    private System.Windows.Forms.RadioButton rbLast30Days;
    private System.Windows.Forms.RadioButton rbAllTime;
    private System.Windows.Forms.RadioButton rbCustom;
    private System.Windows.Forms.DateTimePicker dtpFrom;
    private System.Windows.Forms.DateTimePicker dtpTo;
    private System.Windows.Forms.Label lblTo;
    private System.Windows.Forms.Button btnSyncYield;
    private System.Windows.Forms.Button btnVerifyYieldTable;

    // Employee Tab Controls
    private System.Windows.Forms.GroupBox grpEmpSource;
    private System.Windows.Forms.Label lblEmpSourceInfo;
    private System.Windows.Forms.Label lblEmpTargetInfo;
    private System.Windows.Forms.Button btnSyncEmployee;
    private System.Windows.Forms.Label lblEmpNote;

    // IE Cycle Time Tab Controls
    private System.Windows.Forms.GroupBox grpSourcesIE;
    private System.Windows.Forms.CheckBox chkMlsIE;
    private System.Windows.Forms.CheckBox chkOpticIE;
    private System.Windows.Forms.CheckBox chkAisIE;
    private System.Windows.Forms.GroupBox grpInfoIE;
    private System.Windows.Forms.Label lblInfoIE;
    private System.Windows.Forms.Button btnSyncIECycle;
    private System.Windows.Forms.Button btnVerifyIECycleTable;

    // Connections Tab Controls
    private System.Windows.Forms.Button btnTestConnections;
    private System.Windows.Forms.GroupBox grpConnectionInfo;
    private System.Windows.Forms.Label lblConnDest;
    private System.Windows.Forms.Label lblConnMLS;
    private System.Windows.Forms.Label lblConnOptic;
    private System.Windows.Forms.Label lblConnAIS;
    private System.Windows.Forms.Label lblConnHR;
    private System.Windows.Forms.GroupBox grpScheduleSettings;
    private System.Windows.Forms.Label lblScheduleTime;
    private System.Windows.Forms.NumericUpDown nudHour;
    private System.Windows.Forms.NumericUpDown nudMinute;
    private System.Windows.Forms.CheckBox chkScheduleEnabled;

    // Bottom Log Panel
    private System.Windows.Forms.Panel bottomPanel;
    private System.Windows.Forms.Panel logHeaderPanel;
    private System.Windows.Forms.Label lblLogHeader;
    private System.Windows.Forms.Button btnClearLog;
    private System.Windows.Forms.Button btnCopyLog;
    private System.Windows.Forms.Button btnSaveLog;
    private System.Windows.Forms.TextBox txtLog;
    private System.Windows.Forms.StatusStrip statusStrip;
    private System.Windows.Forms.ToolStripStatusLabel statusLabel;

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.topHeaderPanel = new System.Windows.Forms.Panel();
        this.lblAppTitle = new System.Windows.Forms.Label();
        this.lblAppSubtitle = new System.Windows.Forms.Label();
        this.tabControl = new System.Windows.Forms.TabControl();
        this.tabDashboard = new System.Windows.Forms.TabPage();
        this.tabYield = new System.Windows.Forms.TabPage();
        this.tabEmployee = new System.Windows.Forms.TabPage();
        this.tabIECycleTime = new System.Windows.Forms.TabPage();
        this.tabConnections = new System.Windows.Forms.TabPage();
        
        this.btnSyncAll = new System.Windows.Forms.Button();
        this.btnDashSyncYield = new System.Windows.Forms.Button();
        this.btnDashSyncEmployee = new System.Windows.Forms.Button();
        this.btnDashSyncIECycle = new System.Windows.Forms.Button();
        this.pnlCardYield = new System.Windows.Forms.Panel();
        this.lblCardYieldTitle = new System.Windows.Forms.Label();
        this.lblCardYieldStatus = new System.Windows.Forms.Label();
        this.pnlCardEmployee = new System.Windows.Forms.Panel();
        this.lblCardEmpTitle = new System.Windows.Forms.Label();
        this.lblCardEmpStatus = new System.Windows.Forms.Label();
        this.pnlCardSchedule = new System.Windows.Forms.Panel();
        this.lblCardScheduleTitle = new System.Windows.Forms.Label();
        this.lblCardScheduleStatus = new System.Windows.Forms.Label();
        this.chkAutoScheduleDash = new System.Windows.Forms.CheckBox();

        this.grpSources = new System.Windows.Forms.GroupBox();
        this.chkMLS = new System.Windows.Forms.CheckBox();
        this.chkOptic = new System.Windows.Forms.CheckBox();
        this.chkAIS = new System.Windows.Forms.CheckBox();
        this.grpDate = new System.Windows.Forms.GroupBox();
        this.rbToday = new System.Windows.Forms.RadioButton();
        this.rbYesterday = new System.Windows.Forms.RadioButton();
        this.rbLast7Days = new System.Windows.Forms.RadioButton();
        this.rbLast30Days = new System.Windows.Forms.RadioButton();
        this.rbAllTime = new System.Windows.Forms.RadioButton();
        this.rbCustom = new System.Windows.Forms.RadioButton();
        this.dtpFrom = new System.Windows.Forms.DateTimePicker();
        this.dtpTo = new System.Windows.Forms.DateTimePicker();
        this.lblTo = new System.Windows.Forms.Label();
        this.btnSyncYield = new System.Windows.Forms.Button();
        this.btnVerifyYieldTable = new System.Windows.Forms.Button();

        this.grpEmpSource = new System.Windows.Forms.GroupBox();
        this.lblEmpSourceInfo = new System.Windows.Forms.Label();
        this.lblEmpTargetInfo = new System.Windows.Forms.Label();
        this.btnSyncEmployee = new System.Windows.Forms.Button();
        this.lblEmpNote = new System.Windows.Forms.Label();

        this.grpSourcesIE = new System.Windows.Forms.GroupBox();
        this.chkMlsIE = new System.Windows.Forms.CheckBox();
        this.chkOpticIE = new System.Windows.Forms.CheckBox();
        this.chkAisIE = new System.Windows.Forms.CheckBox();
        this.grpInfoIE = new System.Windows.Forms.GroupBox();
        this.lblInfoIE = new System.Windows.Forms.Label();
        this.btnSyncIECycle = new System.Windows.Forms.Button();
        this.btnVerifyIECycleTable = new System.Windows.Forms.Button();

        this.btnTestConnections = new System.Windows.Forms.Button();
        this.grpConnectionInfo = new System.Windows.Forms.GroupBox();
        this.lblConnDest = new System.Windows.Forms.Label();
        this.lblConnMLS = new System.Windows.Forms.Label();
        this.lblConnOptic = new System.Windows.Forms.Label();
        this.lblConnAIS = new System.Windows.Forms.Label();
        this.lblConnHR = new System.Windows.Forms.Label();
        this.grpScheduleSettings = new System.Windows.Forms.GroupBox();
        this.lblScheduleTime = new System.Windows.Forms.Label();
        this.nudHour = new System.Windows.Forms.NumericUpDown();
        this.nudMinute = new System.Windows.Forms.NumericUpDown();
        this.chkScheduleEnabled = new System.Windows.Forms.CheckBox();

        this.bottomPanel = new System.Windows.Forms.Panel();
        this.logHeaderPanel = new System.Windows.Forms.Panel();
        this.lblLogHeader = new System.Windows.Forms.Label();
        this.btnClearLog = new System.Windows.Forms.Button();
        this.btnCopyLog = new System.Windows.Forms.Button();
        this.btnSaveLog = new System.Windows.Forms.Button();
        this.txtLog = new System.Windows.Forms.TextBox();
        this.statusStrip = new System.Windows.Forms.StatusStrip();
        this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();

        // 
        // MainForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(960, 720);
        this.MinimumSize = new System.Drawing.Size(860, 640);
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "CI Data Sync Hub — Unified Synchronization Center";
        this.BackColor = System.Drawing.Color.FromArgb(244, 246, 249);
        this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

        // 
        // topHeaderPanel
        // 
        this.topHeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
        this.topHeaderPanel.Height = 72;
        this.topHeaderPanel.BackColor = System.Drawing.Color.FromArgb(24, 34, 46);
        this.topHeaderPanel.Padding = new System.Windows.Forms.Padding(20, 10, 20, 10);
        this.topHeaderPanel.Controls.Add(this.lblAppSubtitle);
        this.topHeaderPanel.Controls.Add(this.lblAppTitle);

        // lblAppTitle
        this.lblAppTitle.AutoSize = true;
        this.lblAppTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.lblAppTitle.ForeColor = System.Drawing.Color.White;
        this.lblAppTitle.Location = new System.Drawing.Point(18, 12);
        this.lblAppTitle.Text = "🚀 CI DATA SYNC HUB";

        // lblAppSubtitle
        this.lblAppSubtitle.AutoSize = true;
        this.lblAppSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.lblAppSubtitle.ForeColor = System.Drawing.Color.FromArgb(170, 185, 205);
        this.lblAppSubtitle.Location = new System.Drawing.Point(20, 40);
        this.lblAppSubtitle.Text = "Excelitas CI Hub — Unified Synchronization Center (Yield, Employees & IE Cycle Time)";

        // 
        // tabControl
        // 
        this.tabControl.Dock = System.Windows.Forms.DockStyle.Top;
        this.tabControl.Height = 270;
        this.tabControl.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.tabControl.Padding = new System.Drawing.Point(14, 6);
        this.tabControl.Controls.Add(this.tabDashboard);
        this.tabControl.Controls.Add(this.tabYield);
        this.tabControl.Controls.Add(this.tabEmployee);
        this.tabControl.Controls.Add(this.tabIECycleTime);
        this.tabControl.Controls.Add(this.tabConnections);

        // ==========================================
        // TAB 1: DASHBOARD
        // ==========================================
        this.tabDashboard.Text = "  ⚡ Quick Sync & Dashboard  ";
        this.tabDashboard.BackColor = System.Drawing.Color.FromArgb(248, 249, 251);
        this.tabDashboard.Padding = new System.Windows.Forms.Padding(15);

        // Card 1: Yield
        this.pnlCardYield.Location = new System.Drawing.Point(18, 18);
        this.pnlCardYield.Size = new System.Drawing.Size(280, 80);
        this.pnlCardYield.BackColor = System.Drawing.Color.White;
        this.pnlCardYield.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.lblCardYieldTitle.Text = "DAILY YIELD (MLS/OPTIC/AIS)";
        this.lblCardYieldTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
        this.lblCardYieldTitle.ForeColor = System.Drawing.Color.FromArgb(52, 73, 94);
        this.lblCardYieldTitle.Location = new System.Drawing.Point(12, 10);
        this.lblCardYieldTitle.AutoSize = true;
        this.lblCardYieldStatus.Text = "Status: Ready (Target: CI_Yield)";
        this.lblCardYieldStatus.Font = new System.Drawing.Font("Segoe UI", 8F);
        this.lblCardYieldStatus.ForeColor = System.Drawing.Color.FromArgb(39, 174, 96);
        this.lblCardYieldStatus.Location = new System.Drawing.Point(12, 38);
        this.lblCardYieldStatus.AutoSize = true;
        this.pnlCardYield.Controls.Add(this.lblCardYieldTitle);
        this.pnlCardYield.Controls.Add(this.lblCardYieldStatus);

        // Card 2: Employee
        this.pnlCardEmployee.Location = new System.Drawing.Point(315, 18);
        this.pnlCardEmployee.Size = new System.Drawing.Size(280, 80);
        this.pnlCardEmployee.BackColor = System.Drawing.Color.White;
        this.pnlCardEmployee.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.lblCardEmpTitle.Text = "EMPLOYEE MASTER (BTMTime)";
        this.lblCardEmpTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
        this.lblCardEmpTitle.ForeColor = System.Drawing.Color.FromArgb(52, 73, 94);
        this.lblCardEmpTitle.Location = new System.Drawing.Point(12, 10);
        this.lblCardEmpTitle.AutoSize = true;
        this.lblCardEmpStatus.Text = "Status: Ready (Target: CI_Employee)";
        this.lblCardEmpStatus.Font = new System.Drawing.Font("Segoe UI", 8F);
        this.lblCardEmpStatus.ForeColor = System.Drawing.Color.FromArgb(39, 174, 96);
        this.lblCardEmpStatus.Location = new System.Drawing.Point(12, 38);
        this.lblCardEmpStatus.AutoSize = true;
        this.pnlCardEmployee.Controls.Add(this.lblCardEmpTitle);
        this.pnlCardEmployee.Controls.Add(this.lblCardEmpStatus);

        // Card 3: Scheduler
        this.pnlCardSchedule.Location = new System.Drawing.Point(612, 18);
        this.pnlCardSchedule.Size = new System.Drawing.Size(300, 80);
        this.pnlCardSchedule.BackColor = System.Drawing.Color.White;
        this.pnlCardSchedule.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.lblCardScheduleTitle.Text = "DAILY AUTO-SCHEDULE";
        this.lblCardScheduleTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
        this.lblCardScheduleTitle.ForeColor = System.Drawing.Color.FromArgb(52, 73, 94);
        this.lblCardScheduleTitle.Location = new System.Drawing.Point(12, 10);
        this.lblCardScheduleTitle.AutoSize = true;
        this.lblCardScheduleStatus.Text = "Daily at 06:00 AM (Background)";
        this.lblCardScheduleStatus.Font = new System.Drawing.Font("Segoe UI", 8F);
        this.lblCardScheduleStatus.ForeColor = System.Drawing.Color.FromArgb(41, 128, 185);
        this.lblCardScheduleStatus.Location = new System.Drawing.Point(12, 38);
        this.lblCardScheduleStatus.AutoSize = true;
        this.chkAutoScheduleDash.Text = "Enabled";
        this.chkAutoScheduleDash.Checked = true;
        this.chkAutoScheduleDash.Location = new System.Drawing.Point(215, 35);
        this.chkAutoScheduleDash.AutoSize = true;
        this.chkAutoScheduleDash.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.pnlCardSchedule.Controls.Add(this.lblCardScheduleTitle);
        this.pnlCardSchedule.Controls.Add(this.lblCardScheduleStatus);
        this.pnlCardSchedule.Controls.Add(this.chkAutoScheduleDash);

        // Main Action Button: SYNC ALL
        this.btnSyncAll.Location = new System.Drawing.Point(18, 115);
        this.btnSyncAll.Size = new System.Drawing.Size(330, 52);
        this.btnSyncAll.BackColor = System.Drawing.Color.FromArgb(39, 174, 96);
        this.btnSyncAll.ForeColor = System.Drawing.Color.White;
        this.btnSyncAll.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        this.btnSyncAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnSyncAll.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnSyncAll.Text = "🚀 ONE-CLICK SYNC ALL\n(Yield, Employee & Cycle Time)";

        // Selective Buttons
        this.btnDashSyncYield.Location = new System.Drawing.Point(365, 115);
        this.btnDashSyncYield.Size = new System.Drawing.Size(170, 52);
        this.btnDashSyncYield.BackColor = System.Drawing.Color.FromArgb(41, 128, 185);
        this.btnDashSyncYield.ForeColor = System.Drawing.Color.White;
        this.btnDashSyncYield.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        this.btnDashSyncYield.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnDashSyncYield.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnDashSyncYield.Text = "📊 Sync Yield";

        this.btnDashSyncEmployee.Location = new System.Drawing.Point(550, 115);
        this.btnDashSyncEmployee.Size = new System.Drawing.Size(175, 52);
        this.btnDashSyncEmployee.BackColor = System.Drawing.Color.FromArgb(142, 68, 173);
        this.btnDashSyncEmployee.ForeColor = System.Drawing.Color.White;
        this.btnDashSyncEmployee.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        this.btnDashSyncEmployee.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnDashSyncEmployee.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnDashSyncEmployee.Text = "👥 Sync Employees";

        this.btnDashSyncIECycle.Location = new System.Drawing.Point(740, 115);
        this.btnDashSyncIECycle.Size = new System.Drawing.Size(172, 52);
        this.btnDashSyncIECycle.BackColor = System.Drawing.Color.FromArgb(211, 84, 0);
        this.btnDashSyncIECycle.ForeColor = System.Drawing.Color.White;
        this.btnDashSyncIECycle.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        this.btnDashSyncIECycle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnDashSyncIECycle.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnDashSyncIECycle.Text = "⏱️ Sync Cycle Time";

        this.tabDashboard.Controls.Add(this.pnlCardYield);
        this.tabDashboard.Controls.Add(this.pnlCardEmployee);
        this.tabDashboard.Controls.Add(this.pnlCardSchedule);
        this.tabDashboard.Controls.Add(this.btnSyncAll);
        this.tabDashboard.Controls.Add(this.btnDashSyncYield);
        this.tabDashboard.Controls.Add(this.btnDashSyncEmployee);
        this.tabDashboard.Controls.Add(this.btnDashSyncIECycle);
        this.btnDashSyncEmployee.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnDashSyncEmployee.Text = "👥 Sync Employees Only";

        this.tabDashboard.Controls.Add(this.pnlCardYield);
        this.tabDashboard.Controls.Add(this.pnlCardEmployee);
        this.tabDashboard.Controls.Add(this.pnlCardSchedule);
        this.tabDashboard.Controls.Add(this.btnSyncAll);
        this.tabDashboard.Controls.Add(this.btnDashSyncYield);
        this.tabDashboard.Controls.Add(this.btnDashSyncEmployee);

        // ==========================================
        // TAB 2: YIELD SYNC
        // ==========================================
        this.tabYield.Text = "  📊 Daily Yield Sync  ";
        this.tabYield.BackColor = System.Drawing.Color.FromArgb(248, 249, 251);
        this.tabYield.Padding = new System.Windows.Forms.Padding(15);

        // grpSources
        this.grpSources.Location = new System.Drawing.Point(18, 12);
        this.grpSources.Size = new System.Drawing.Size(420, 100);
        this.grpSources.Text = "Source Databases (Divisions)";
        this.grpSources.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.chkMLS.Text = "MLS (Lighting) - ETCBTMA03\\DIGITMONT.digimon_lighting";
        this.chkMLS.Location = new System.Drawing.Point(15, 22);
        this.chkMLS.Size = new System.Drawing.Size(390, 22);
        this.chkMLS.Checked = true;
        this.chkMLS.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.chkOptic.Text = "OPTIC / VISION - ETCBTMA03\\DIGITMONT.digimon_vision";
        this.chkOptic.Location = new System.Drawing.Point(15, 46);
        this.chkOptic.Size = new System.Drawing.Size(390, 22);
        this.chkOptic.Checked = true;
        this.chkOptic.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.chkAIS.Text = "AIS (Detection) - ETCBTMSQL01\\DIGITMONT.dbDigitmontDetection";
        this.chkAIS.Location = new System.Drawing.Point(15, 70);
        this.chkAIS.Size = new System.Drawing.Size(390, 22);
        this.chkAIS.Checked = true;
        this.chkAIS.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.grpSources.Controls.Add(this.chkMLS);
        this.grpSources.Controls.Add(this.chkOptic);
        this.grpSources.Controls.Add(this.chkAIS);

        // grpDate
        this.grpDate.Location = new System.Drawing.Point(455, 12);
        this.grpDate.Size = new System.Drawing.Size(460, 100);
        this.grpDate.Text = "Date Filter Range";
        this.grpDate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.rbToday.Text = "Today";
        this.rbToday.Location = new System.Drawing.Point(15, 22);
        this.rbToday.Size = new System.Drawing.Size(70, 20);
        this.rbToday.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.rbYesterday.Text = "Yesterday";
        this.rbYesterday.Location = new System.Drawing.Point(95, 22);
        this.rbYesterday.Size = new System.Drawing.Size(85, 20);
        this.rbYesterday.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.rbLast7Days.Text = "Last 7 Days (Default)";
        this.rbLast7Days.Location = new System.Drawing.Point(190, 22);
        this.rbLast7Days.Size = new System.Drawing.Size(140, 20);
        this.rbLast7Days.Checked = true;
        this.rbLast7Days.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.rbLast30Days.Text = "Last 30 Days";
        this.rbLast30Days.Location = new System.Drawing.Point(340, 22);
        this.rbLast30Days.Size = new System.Drawing.Size(100, 20);
        this.rbLast30Days.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.rbAllTime.Text = "All Historical";
        this.rbAllTime.Location = new System.Drawing.Point(15, 50);
        this.rbAllTime.Size = new System.Drawing.Size(100, 20);
        this.rbAllTime.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.rbCustom.Text = "Custom:";
        this.rbCustom.Location = new System.Drawing.Point(125, 50);
        this.rbCustom.Size = new System.Drawing.Size(70, 20);
        this.rbCustom.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.dtpFrom.Location = new System.Drawing.Point(200, 48);
        this.dtpFrom.Size = new System.Drawing.Size(105, 23);
        this.dtpFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
        this.dtpFrom.Enabled = false;
        this.lblTo.Text = "to";
        this.lblTo.Location = new System.Drawing.Point(312, 51);
        this.lblTo.Size = new System.Drawing.Size(20, 20);
        this.dtpTo.Location = new System.Drawing.Point(335, 48);
        this.dtpTo.Size = new System.Drawing.Size(105, 23);
        this.dtpTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
        this.dtpTo.Enabled = false;
        this.grpDate.Controls.Add(this.rbToday);
        this.grpDate.Controls.Add(this.rbYesterday);
        this.grpDate.Controls.Add(this.rbLast7Days);
        this.grpDate.Controls.Add(this.rbLast30Days);
        this.grpDate.Controls.Add(this.rbAllTime);
        this.grpDate.Controls.Add(this.rbCustom);
        this.grpDate.Controls.Add(this.dtpFrom);
        this.grpDate.Controls.Add(this.lblTo);
        this.grpDate.Controls.Add(this.dtpTo);

        // Buttons Yield
        this.btnSyncYield.Location = new System.Drawing.Point(18, 125);
        this.btnSyncYield.Size = new System.Drawing.Size(420, 46);
        this.btnSyncYield.BackColor = System.Drawing.Color.FromArgb(41, 128, 185);
        this.btnSyncYield.ForeColor = System.Drawing.Color.White;
        this.btnSyncYield.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
        this.btnSyncYield.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnSyncYield.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnSyncYield.Text = "📊 START YIELD SYNCHRONIZATION";

        this.btnVerifyYieldTable.Location = new System.Drawing.Point(455, 125);
        this.btnVerifyYieldTable.Size = new System.Drawing.Size(460, 46);
        this.btnVerifyYieldTable.BackColor = System.Drawing.Color.FromArgb(52, 73, 94);
        this.btnVerifyYieldTable.ForeColor = System.Drawing.Color.White;
        this.btnVerifyYieldTable.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        this.btnVerifyYieldTable.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnVerifyYieldTable.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnVerifyYieldTable.Text = "🔍 Verify Target Table [dbo].[CI_Yield]";

        this.tabYield.Controls.Add(this.grpSources);
        this.tabYield.Controls.Add(this.grpDate);
        this.tabYield.Controls.Add(this.btnSyncYield);
        this.tabYield.Controls.Add(this.btnVerifyYieldTable);

        // ==========================================
        // TAB 3: EMPLOYEE SYNC
        // ==========================================
        this.tabEmployee.Text = "  👥 Employee Sync  ";
        this.tabEmployee.BackColor = System.Drawing.Color.FromArgb(248, 249, 251);
        this.tabEmployee.Padding = new System.Windows.Forms.Padding(15);

        this.grpEmpSource.Location = new System.Drawing.Point(18, 12);
        this.grpEmpSource.Size = new System.Drawing.Size(897, 100);
        this.grpEmpSource.Text = "BTMTime Employee Master Information";
        this.grpEmpSource.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.lblEmpSourceInfo.Text = "Source: ETCRESSQL02 -> Database: BTMTime (Active employees where TERMINATION_STATUS is NULL)";
        this.lblEmpSourceInfo.Location = new System.Drawing.Point(15, 25);
        this.lblEmpSourceInfo.AutoSize = true;
        this.lblEmpSourceInfo.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.lblEmpTargetInfo.Text = "Target: BTMSQL01\\DIGITMONT_DEV -> Database: CI_LSS -> Table: [dbo].[CI_Employee]";
        this.lblEmpTargetInfo.Location = new System.Drawing.Point(15, 48);
        this.lblEmpTargetInfo.AutoSize = true;
        this.lblEmpTargetInfo.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.lblEmpNote.Text = "Transfer Method: Truncate destination & fast SqlBulkCopy with section & department mapping.";
        this.lblEmpNote.Location = new System.Drawing.Point(15, 71);
        this.lblEmpNote.AutoSize = true;
        this.lblEmpNote.ForeColor = System.Drawing.Color.FromArgb(120, 140, 160);
        this.lblEmpNote.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Italic);
        this.grpEmpSource.Controls.Add(this.lblEmpSourceInfo);
        this.grpEmpSource.Controls.Add(this.lblEmpTargetInfo);
        this.grpEmpSource.Controls.Add(this.lblEmpNote);

        this.btnSyncEmployee.Location = new System.Drawing.Point(18, 125);
        this.btnSyncEmployee.Size = new System.Drawing.Size(897, 46);
        this.btnSyncEmployee.BackColor = System.Drawing.Color.FromArgb(142, 68, 173);
        this.btnSyncEmployee.ForeColor = System.Drawing.Color.White;
        this.btnSyncEmployee.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
        this.btnSyncEmployee.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnSyncEmployee.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnSyncEmployee.Text = "👥 START EMPLOYEE MASTER SYNCHRONIZATION";

        this.tabEmployee.Controls.Add(this.grpEmpSource);
        this.tabEmployee.Controls.Add(this.btnSyncEmployee);

        // ==========================================
        // TAB 4: IE CYCLE TIME SYNC
        // ==========================================
        this.tabIECycleTime.Text = "  ⏱️ IE Cycle Time Sync  ";
        this.tabIECycleTime.BackColor = System.Drawing.Color.FromArgb(248, 249, 251);
        this.tabIECycleTime.Padding = new System.Windows.Forms.Padding(15);

        // grpSourcesIE
        this.grpSourcesIE.Location = new System.Drawing.Point(18, 12);
        this.grpSourcesIE.Size = new System.Drawing.Size(430, 100);
        this.grpSourcesIE.Text = "Source Databases (dmIECycleTime_Header)";
        this.grpSourcesIE.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.chkMlsIE.Text = "MLS (Lighting) - ETCBTMA03\\DIGITMONT.digimon_lighting";
        this.chkMlsIE.Location = new System.Drawing.Point(15, 22);
        this.chkMlsIE.Size = new System.Drawing.Size(400, 22);
        this.chkMlsIE.Checked = true;
        this.chkMlsIE.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.chkOpticIE.Text = "OPTIC / VISION - ETCBTMA03\\DIGITMONT.digimon_vision";
        this.chkOpticIE.Location = new System.Drawing.Point(15, 46);
        this.chkOpticIE.Size = new System.Drawing.Size(400, 22);
        this.chkOpticIE.Checked = true;
        this.chkOpticIE.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.chkAisIE.Text = "AIS (Detection) - ETCBTMSQL01\\DIGITMONT.dbDigitmontDetection";
        this.chkAisIE.Location = new System.Drawing.Point(15, 70);
        this.chkAisIE.Size = new System.Drawing.Size(400, 22);
        this.chkAisIE.Checked = true;
        this.chkAisIE.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.grpSourcesIE.Controls.Add(this.chkMlsIE);
        this.grpSourcesIE.Controls.Add(this.chkOpticIE);
        this.grpSourcesIE.Controls.Add(this.chkAisIE);

        // grpInfoIE
        this.grpInfoIE.Location = new System.Drawing.Point(460, 12);
        this.grpInfoIE.Size = new System.Drawing.Size(455, 100);
        this.grpInfoIE.Text = "Target Mapping & UPH Information";
        this.grpInfoIE.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.lblInfoIE.Location = new System.Drawing.Point(15, 22);
        this.lblInfoIE.Size = new System.Drawing.Size(425, 70);
        this.lblInfoIE.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.lblInfoIE.ForeColor = System.Drawing.Color.FromArgb(70, 80, 95);
        this.lblInfoIE.Text = "Extracts dmIECycleTime_Header (MachCode, DeviceCode, ProcCode, UPH, MachineTime, LaborTime, Capacity) and performs an idempotent upsert into CI_LSS.dbo.CI_IECycleTime_Header.";
        this.grpInfoIE.Controls.Add(this.lblInfoIE);

        // Action Buttons
        this.btnSyncIECycle.Location = new System.Drawing.Point(18, 125);
        this.btnSyncIECycle.Size = new System.Drawing.Size(540, 46);
        this.btnSyncIECycle.BackColor = System.Drawing.Color.FromArgb(211, 84, 0);
        this.btnSyncIECycle.ForeColor = System.Drawing.Color.White;
        this.btnSyncIECycle.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
        this.btnSyncIECycle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnSyncIECycle.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnSyncIECycle.Text = "⏱️ START IE CYCLE TIME SYNCHRONIZATION";

        this.btnVerifyIECycleTable.Location = new System.Drawing.Point(575, 125);
        this.btnVerifyIECycleTable.Size = new System.Drawing.Size(340, 46);
        this.btnVerifyIECycleTable.BackColor = System.Drawing.Color.FromArgb(243, 156, 18);
        this.btnVerifyIECycleTable.ForeColor = System.Drawing.Color.White;
        this.btnVerifyIECycleTable.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.btnVerifyIECycleTable.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnVerifyIECycleTable.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnVerifyIECycleTable.Text = "🔍 Verify Destination Table (CI_IECycleTime_Header)";

        this.tabIECycleTime.Controls.Add(this.grpSourcesIE);
        this.tabIECycleTime.Controls.Add(this.grpInfoIE);
        this.tabIECycleTime.Controls.Add(this.btnSyncIECycle);
        this.tabIECycleTime.Controls.Add(this.btnVerifyIECycleTable);

        // ==========================================
        // TAB 5: CONNECTIONS & SETTINGS
        // ==========================================
        this.tabConnections.Text = "  🔌 Diagnostics & Settings  ";
        this.tabConnections.BackColor = System.Drawing.Color.FromArgb(248, 249, 251);
        this.tabConnections.Padding = new System.Windows.Forms.Padding(15);

        this.grpConnectionInfo.Location = new System.Drawing.Point(18, 12);
        this.grpConnectionInfo.Size = new System.Drawing.Size(560, 165);
        this.grpConnectionInfo.Text = "Configured Server Endpoints (from appsettings.json)";
        this.grpConnectionInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        
        this.lblConnDest.Text = "1. Destination CI_LSS: BTMSQL01\\DIGITMONT_DEV (CI_Hub_User)";
        this.lblConnDest.Location = new System.Drawing.Point(15, 25);
        this.lblConnDest.AutoSize = true;
        this.lblConnDest.Font = new System.Drawing.Font("Segoe UI", 8.5F);

        this.lblConnMLS.Text = "2. Source MLS: ETCBTMA03\\DIGITMONT.digimon_lighting";
        this.lblConnMLS.Location = new System.Drawing.Point(15, 48);
        this.lblConnMLS.AutoSize = true;
        this.lblConnMLS.Font = new System.Drawing.Font("Segoe UI", 8.5F);

        this.lblConnOptic.Text = "3. Source OPTIC: ETCBTMA03\\DIGITMONT.digimon_vision";
        this.lblConnOptic.Location = new System.Drawing.Point(15, 71);
        this.lblConnOptic.AutoSize = true;
        this.lblConnOptic.Font = new System.Drawing.Font("Segoe UI", 8.5F);

        this.lblConnAIS.Text = "4. Source AIS: ETCBTMSQL01\\DIGITMONT.dbDigitmontDetection";
        this.lblConnAIS.Location = new System.Drawing.Point(15, 94);
        this.lblConnAIS.AutoSize = true;
        this.lblConnAIS.Font = new System.Drawing.Font("Segoe UI", 8.5F);

        this.lblConnHR.Text = "5. Source BTMTime: ETCRESSQL02.BTMTime (BTM_IT_RO)";
        this.lblConnHR.Location = new System.Drawing.Point(15, 117);
        this.lblConnHR.AutoSize = true;
        this.lblConnHR.Font = new System.Drawing.Font("Segoe UI", 8.5F);

        this.btnTestConnections.Location = new System.Drawing.Point(15, 137);
        this.btnTestConnections.Size = new System.Drawing.Size(320, 26);
        this.btnTestConnections.Text = "🔍 Test All 5 Server Connections";
        this.btnTestConnections.BackColor = System.Drawing.Color.FromArgb(41, 128, 185);
        this.btnTestConnections.ForeColor = System.Drawing.Color.White;
        this.btnTestConnections.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnTestConnections.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnTestConnections.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);

        this.grpConnectionInfo.Controls.Add(this.lblConnDest);
        this.grpConnectionInfo.Controls.Add(this.lblConnMLS);
        this.grpConnectionInfo.Controls.Add(this.lblConnOptic);
        this.grpConnectionInfo.Controls.Add(this.lblConnAIS);
        this.grpConnectionInfo.Controls.Add(this.lblConnHR);
        this.grpConnectionInfo.Controls.Add(this.btnTestConnections);

        // grpScheduleSettings
        this.grpScheduleSettings.Location = new System.Drawing.Point(595, 12);
        this.grpScheduleSettings.Size = new System.Drawing.Size(320, 165);
        this.grpScheduleSettings.Text = "Automated Daily Schedule";
        this.grpScheduleSettings.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

        this.chkScheduleEnabled.Text = "Enable Automated Daily Sync";
        this.chkScheduleEnabled.Checked = true;
        this.chkScheduleEnabled.Location = new System.Drawing.Point(15, 25);
        this.chkScheduleEnabled.AutoSize = true;
        this.chkScheduleEnabled.Font = new System.Drawing.Font("Segoe UI", 8.5F);

        this.lblScheduleTime.Text = "Daily Execution Time (24h):";
        this.lblScheduleTime.Location = new System.Drawing.Point(15, 60);
        this.lblScheduleTime.AutoSize = true;
        this.lblScheduleTime.Font = new System.Drawing.Font("Segoe UI", 8.5F);

        this.nudHour.Location = new System.Drawing.Point(15, 85);
        this.nudHour.Size = new System.Drawing.Size(60, 23);
        this.nudHour.Minimum = 0;
        this.nudHour.Maximum = 23;
        this.nudHour.Value = 6;

        this.nudMinute.Location = new System.Drawing.Point(85, 85);
        this.nudMinute.Size = new System.Drawing.Size(60, 23);
        this.nudMinute.Minimum = 0;
        this.nudMinute.Maximum = 59;
        this.nudMinute.Value = 0;

        this.grpScheduleSettings.Controls.Add(this.chkScheduleEnabled);
        this.grpScheduleSettings.Controls.Add(this.lblScheduleTime);
        this.grpScheduleSettings.Controls.Add(this.nudHour);
        this.grpScheduleSettings.Controls.Add(this.nudMinute);

        this.tabConnections.Controls.Add(this.grpConnectionInfo);
        this.tabConnections.Controls.Add(this.grpScheduleSettings);

        // ==========================================
        // BOTTOM PANEL (LOGS & STATUS)
        // ==========================================
        this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.bottomPanel.BackColor = System.Drawing.Color.FromArgb(244, 246, 249);
        this.bottomPanel.Padding = new System.Windows.Forms.Padding(15, 5, 15, 0);

        this.logHeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
        this.logHeaderPanel.Height = 32;
        this.logHeaderPanel.Controls.Add(this.lblLogHeader);
        this.logHeaderPanel.Controls.Add(this.btnClearLog);
        this.logHeaderPanel.Controls.Add(this.btnCopyLog);
        this.logHeaderPanel.Controls.Add(this.btnSaveLog);

        this.lblLogHeader.Text = "📋 EXECUTION LOGS & LIVE TELEMETRY";
        this.lblLogHeader.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
        this.lblLogHeader.ForeColor = System.Drawing.Color.FromArgb(52, 73, 94);
        this.lblLogHeader.Location = new System.Drawing.Point(2, 6);
        this.lblLogHeader.AutoSize = true;

        this.btnClearLog.Text = "🧹 Clear";
        this.btnClearLog.Location = new System.Drawing.Point(665, 3);
        this.btnClearLog.Size = new System.Drawing.Size(75, 25);
        this.btnClearLog.Font = new System.Drawing.Font("Segoe UI", 8F);

        this.btnCopyLog.Text = "📋 Copy";
        this.btnCopyLog.Location = new System.Drawing.Point(748, 3);
        this.btnCopyLog.Size = new System.Drawing.Size(75, 25);
        this.btnCopyLog.Font = new System.Drawing.Font("Segoe UI", 8F);

        this.btnSaveLog.Text = "💾 Save Log";
        this.btnSaveLog.Location = new System.Drawing.Point(831, 3);
        this.btnSaveLog.Size = new System.Drawing.Size(90, 25);
        this.btnSaveLog.Font = new System.Drawing.Font("Segoe UI", 8F);

        this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtLog.Multiline = true;
        this.txtLog.ReadOnly = true;
        this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtLog.BackColor = System.Drawing.Color.FromArgb(20, 24, 30);
        this.txtLog.ForeColor = System.Drawing.Color.FromArgb(220, 225, 235);
        this.txtLog.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular);

        this.statusStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.statusStrip.Items.Add(this.statusLabel);
        this.statusLabel.Text = "Ready. Standing by for command or scheduled trigger.";

        this.bottomPanel.Controls.Add(this.txtLog);
        this.bottomPanel.Controls.Add(this.logHeaderPanel);

        // Add to Form
        this.Controls.Add(this.bottomPanel);
        this.Controls.Add(this.statusStrip);
        this.Controls.Add(this.tabControl);
        this.Controls.Add(this.topHeaderPanel);
    }
}

