namespace YieldSyncApp;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    
    // Main panels
    private System.Windows.Forms.Panel topPanel;
    private System.Windows.Forms.Panel configPanel;
    private System.Windows.Forms.Panel logPanel;
    private System.Windows.Forms.Panel statusStripPanel;

    // Header controls
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblSubtitle;

    // Source selection controls
    private System.Windows.Forms.GroupBox grpSources;
    private System.Windows.Forms.CheckBox chkMLS;
    private System.Windows.Forms.CheckBox chkOptic;
    private System.Windows.Forms.CheckBox chkAIS;

    // Date range controls
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

    // Scheduler controls
    private System.Windows.Forms.GroupBox grpSchedule;
    private System.Windows.Forms.CheckBox chkAutoSchedule;
    private System.Windows.Forms.Label lblScheduleInfo;

    // Buttons
    private System.Windows.Forms.Button btnSync;
    private System.Windows.Forms.Button btnTestConnections;
    private System.Windows.Forms.Button btnClearLog;

    // Log control
    private System.Windows.Forms.TextBox txtLog;

    // Status label
    private System.Windows.Forms.Label lblStatus;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.topPanel = new System.Windows.Forms.Panel();
        this.lblTitle = new System.Windows.Forms.Label();
        this.lblSubtitle = new System.Windows.Forms.Label();

        this.configPanel = new System.Windows.Forms.Panel();
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
        this.lblTo = new System.Windows.Forms.Label();
        this.dtpTo = new System.Windows.Forms.DateTimePicker();

        this.grpSchedule = new System.Windows.Forms.GroupBox();
        this.chkAutoSchedule = new System.Windows.Forms.CheckBox();
        this.lblScheduleInfo = new System.Windows.Forms.Label();

        this.btnSync = new System.Windows.Forms.Button();
        this.btnTestConnections = new System.Windows.Forms.Button();
        this.btnClearLog = new System.Windows.Forms.Button();

        this.logPanel = new System.Windows.Forms.Panel();
        this.txtLog = new System.Windows.Forms.TextBox();

        this.statusStripPanel = new System.Windows.Forms.Panel();
        this.lblStatus = new System.Windows.Forms.Label();

        this.topPanel.SuspendLayout();
        this.configPanel.SuspendLayout();
        this.grpSources.SuspendLayout();
        this.grpDate.SuspendLayout();
        this.grpSchedule.SuspendLayout();
        this.logPanel.SuspendLayout();
        this.statusStripPanel.SuspendLayout();
        this.SuspendLayout();

        // 
        // MainForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.FromArgb(24, 25, 30);
        this.ClientSize = new System.Drawing.Size(980, 680);
        this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.ForeColor = System.Drawing.Color.White;
        this.MinimumSize = new System.Drawing.Size(900, 650);
        this.Name = "MainForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "CI Yield - Daily Data Synchronization Hub";

        // 
        // topPanel
        // 
        this.topPanel.Controls.Add(this.lblTitle);
        this.topPanel.Controls.Add(this.lblSubtitle);
        this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
        this.topPanel.Location = new System.Drawing.Point(0, 0);
        this.topPanel.Name = "topPanel";
        this.topPanel.Padding = new System.Windows.Forms.Padding(25, 20, 25, 10);
        this.topPanel.Size = new System.Drawing.Size(980, 85);
        this.topPanel.TabIndex = 0;

        // 
        // lblTitle
        // 
        this.lblTitle.AutoSize = true;
        this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(236, 240, 241);
        this.lblTitle.Location = new System.Drawing.Point(20, 10);
        this.lblTitle.Name = "lblTitle";
        this.lblTitle.Size = new System.Drawing.Size(530, 32);
        this.lblTitle.Text = "CI Yield - Daily Data Synchronization Hub";

        // 
        // lblSubtitle
        // 
        this.lblSubtitle.AutoSize = true;
        this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(149, 165, 166);
        this.lblSubtitle.Location = new System.Drawing.Point(23, 46);
        this.lblSubtitle.Name = "lblSubtitle";
        this.lblSubtitle.Size = new System.Drawing.Size(730, 17);
        this.lblSubtitle.Text = "Daily DeviceCode First Pass Yield (FPY) & Overall Yield Aggregator -> [BTMSQL01\\DIGITMONT_DEV].[CI_LSS].[dbo].[CI_Yield]";

        // 
        // configPanel
        // 
        this.configPanel.Controls.Add(this.grpSources);
        this.configPanel.Controls.Add(this.grpDate);
        this.configPanel.Controls.Add(this.grpSchedule);
        this.configPanel.Controls.Add(this.btnSync);
        this.configPanel.Controls.Add(this.btnTestConnections);
        this.configPanel.Controls.Add(this.btnClearLog);
        this.configPanel.Dock = System.Windows.Forms.DockStyle.Top;
        this.configPanel.Location = new System.Drawing.Point(0, 85);
        this.configPanel.Name = "configPanel";
        this.configPanel.Padding = new System.Windows.Forms.Padding(25, 0, 25, 10);
        this.configPanel.Size = new System.Drawing.Size(980, 220);
        this.configPanel.TabIndex = 1;

        // 
        // grpSources
        // 
        this.grpSources.Controls.Add(this.chkMLS);
        this.grpSources.Controls.Add(this.chkOptic);
        this.grpSources.Controls.Add(this.chkAIS);
        this.grpSources.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.grpSources.ForeColor = System.Drawing.Color.FromArgb(52, 152, 219);
        this.grpSources.Location = new System.Drawing.Point(25, 5);
        this.grpSources.Name = "grpSources";
        this.grpSources.Size = new System.Drawing.Size(320, 140);
        this.grpSources.TabIndex = 0;
        this.grpSources.TabStop = false;
        this.grpSources.Text = " 📦 Source Plant Databases ";

        // 
        // chkMLS
        // 
        this.chkMLS.AutoSize = true;
        this.chkMLS.Checked = true;
        this.chkMLS.CheckState = System.Windows.Forms.CheckState.Checked;
        this.chkMLS.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.chkMLS.ForeColor = System.Drawing.Color.White;
        this.chkMLS.Location = new System.Drawing.Point(15, 28);
        this.chkMLS.Name = "chkMLS";
        this.chkMLS.Size = new System.Drawing.Size(262, 19);
        this.chkMLS.TabIndex = 0;
        this.chkMLS.Text = "MLS (Lighting) - ETCBTMA03\\digimon_lighting";
        this.chkMLS.UseVisualStyleBackColor = true;

        // 
        // chkOptic
        // 
        this.chkOptic.AutoSize = true;
        this.chkOptic.Checked = true;
        this.chkOptic.CheckState = System.Windows.Forms.CheckState.Checked;
        this.chkOptic.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.chkOptic.ForeColor = System.Drawing.Color.White;
        this.chkOptic.Location = new System.Drawing.Point(15, 60);
        this.chkOptic.Name = "chkOptic";
        this.chkOptic.Size = new System.Drawing.Size(248, 19);
        this.chkOptic.TabIndex = 1;
        this.chkOptic.Text = "OPTIC / VISION - ETCBTMA03\\digimon_vision";
        this.chkOptic.UseVisualStyleBackColor = true;

        // 
        // chkAIS
        // 
        this.chkAIS.AutoSize = true;
        this.chkAIS.Checked = true;
        this.chkAIS.CheckState = System.Windows.Forms.CheckState.Checked;
        this.chkAIS.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.chkAIS.ForeColor = System.Drawing.Color.White;
        this.chkAIS.Location = new System.Drawing.Point(15, 92);
        this.chkAIS.Name = "chkAIS";
        this.chkAIS.Size = new System.Drawing.Size(273, 19);
        this.chkAIS.TabIndex = 2;
        this.chkAIS.Text = "AIS (Detection) - ETCBTMSQL01\\dbDigitmontDet";
        this.chkAIS.UseVisualStyleBackColor = true;

        // 
        // grpDate
        // 
        this.grpDate.Controls.Add(this.rbToday);
        this.grpDate.Controls.Add(this.rbYesterday);
        this.grpDate.Controls.Add(this.rbLast7Days);
        this.grpDate.Controls.Add(this.rbLast30Days);
        this.grpDate.Controls.Add(this.rbAllTime);
        this.grpDate.Controls.Add(this.rbCustom);
        this.grpDate.Controls.Add(this.dtpFrom);
        this.grpDate.Controls.Add(this.lblTo);
        this.grpDate.Controls.Add(this.dtpTo);
        this.grpDate.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.grpDate.ForeColor = System.Drawing.Color.FromArgb(46, 204, 113);
        this.grpDate.Location = new System.Drawing.Point(360, 5);
        this.grpDate.Name = "grpDate";
        this.grpDate.Size = new System.Drawing.Size(370, 140);
        this.grpDate.TabIndex = 1;
        this.grpDate.TabStop = false;
        this.grpDate.Text = " 📅 Daily Accumulation Window ";

        // 
        // rbToday
        // 
        this.rbToday.AutoSize = true;
        this.rbToday.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.rbToday.ForeColor = System.Drawing.Color.White;
        this.rbToday.Location = new System.Drawing.Point(15, 26);
        this.rbToday.Name = "rbToday";
        this.rbToday.Size = new System.Drawing.Size(56, 19);
        this.rbToday.TabIndex = 0;
        this.rbToday.Text = "Today";
        this.rbToday.UseVisualStyleBackColor = true;
        this.rbToday.CheckedChanged += new System.EventHandler(this.DateFilter_Changed);

        // 
        // rbYesterday
        // 
        this.rbYesterday.AutoSize = true;
        this.rbYesterday.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.rbYesterday.ForeColor = System.Drawing.Color.White;
        this.rbYesterday.Location = new System.Drawing.Point(90, 26);
        this.rbYesterday.Name = "rbYesterday";
        this.rbYesterday.Size = new System.Drawing.Size(76, 19);
        this.rbYesterday.TabIndex = 1;
        this.rbYesterday.Text = "Yesterday";
        this.rbYesterday.UseVisualStyleBackColor = true;
        this.rbYesterday.CheckedChanged += new System.EventHandler(this.DateFilter_Changed);

        // 
        // rbLast7Days
        // 
        this.rbLast7Days.AutoSize = true;
        this.rbLast7Days.Checked = true;
        this.rbLast7Days.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.rbLast7Days.ForeColor = System.Drawing.Color.FromArgb(46, 204, 113);
        this.rbLast7Days.Location = new System.Drawing.Point(180, 26);
        this.rbLast7Days.Name = "rbLast7Days";
        this.rbLast7Days.Size = new System.Drawing.Size(86, 19);
        this.rbLast7Days.TabIndex = 2;
        this.rbLast7Days.TabStop = true;
        this.rbLast7Days.Text = "Last 7 Days";
        this.rbLast7Days.UseVisualStyleBackColor = true;
        this.rbLast7Days.CheckedChanged += new System.EventHandler(this.DateFilter_Changed);

        // 
        // rbLast30Days
        // 
        this.rbLast30Days.AutoSize = true;
        this.rbLast30Days.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.rbLast30Days.ForeColor = System.Drawing.Color.White;
        this.rbLast30Days.Location = new System.Drawing.Point(275, 26);
        this.rbLast30Days.Name = "rbLast30Days";
        this.rbLast30Days.Size = new System.Drawing.Size(89, 19);
        this.rbLast30Days.TabIndex = 3;
        this.rbLast30Days.Text = "Last 30 Days";
        this.rbLast30Days.UseVisualStyleBackColor = true;
        this.rbLast30Days.CheckedChanged += new System.EventHandler(this.DateFilter_Changed);

        // 
        // rbAllTime
        // 
        this.rbAllTime.AutoSize = true;
        this.rbAllTime.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.rbAllTime.ForeColor = System.Drawing.Color.White;
        this.rbAllTime.Location = new System.Drawing.Point(15, 60);
        this.rbAllTime.Name = "rbAllTime";
        this.rbAllTime.Size = new System.Drawing.Size(126, 19);
        this.rbAllTime.TabIndex = 4;
        this.rbAllTime.Text = "All Time (Full Sync)";
        this.rbAllTime.UseVisualStyleBackColor = true;
        this.rbAllTime.CheckedChanged += new System.EventHandler(this.DateFilter_Changed);

        // 
        // rbCustom
        // 
        this.rbCustom.AutoSize = true;
        this.rbCustom.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.rbCustom.ForeColor = System.Drawing.Color.White;
        this.rbCustom.Location = new System.Drawing.Point(160, 60);
        this.rbCustom.Name = "rbCustom";
        this.rbCustom.Size = new System.Drawing.Size(107, 19);
        this.rbCustom.TabIndex = 5;
        this.rbCustom.Text = "Custom Range:";
        this.rbCustom.UseVisualStyleBackColor = true;
        this.rbCustom.CheckedChanged += new System.EventHandler(this.DateFilter_Changed);

        // 
        // dtpFrom
        // 
        this.dtpFrom.CalendarFont = new System.Drawing.Font("Segoe UI", 9F);
        this.dtpFrom.CustomFormat = "yyyy-MM-dd";
        this.dtpFrom.Enabled = false;
        this.dtpFrom.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.dtpFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
        this.dtpFrom.Location = new System.Drawing.Point(15, 95);
        this.dtpFrom.Name = "dtpFrom";
        this.dtpFrom.Size = new System.Drawing.Size(130, 23);
        this.dtpFrom.TabIndex = 6;

        // 
        // lblTo
        // 
        this.lblTo.AutoSize = true;
        this.lblTo.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblTo.ForeColor = System.Drawing.Color.FromArgb(189, 195, 199);
        this.lblTo.Location = new System.Drawing.Point(155, 98);
        this.lblTo.Name = "lblTo";
        this.lblTo.Size = new System.Drawing.Size(18, 15);
        this.lblTo.TabIndex = 7;
        this.lblTo.Text = "to";

        // 
        // dtpTo
        // 
        this.dtpTo.CalendarFont = new System.Drawing.Font("Segoe UI", 9F);
        this.dtpTo.CustomFormat = "yyyy-MM-dd";
        this.dtpTo.Enabled = false;
        this.dtpTo.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.dtpTo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
        this.dtpTo.Location = new System.Drawing.Point(180, 95);
        this.dtpTo.Name = "dtpTo";
        this.dtpTo.Size = new System.Drawing.Size(130, 23);
        this.dtpTo.TabIndex = 8;

        // 
        // grpSchedule
        // 
        this.grpSchedule.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
        this.grpSchedule.Controls.Add(this.chkAutoSchedule);
        this.grpSchedule.Controls.Add(this.lblScheduleInfo);
        this.grpSchedule.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.grpSchedule.ForeColor = System.Drawing.Color.FromArgb(243, 156, 18);
        this.grpSchedule.Location = new System.Drawing.Point(745, 5);
        this.grpSchedule.Name = "grpSchedule";
        this.grpSchedule.Size = new System.Drawing.Size(210, 140);
        this.grpSchedule.TabIndex = 2;
        this.grpSchedule.TabStop = false;
        this.grpSchedule.Text = " ⏰ Automation ";

        // 
        // chkAutoSchedule
        // 
        this.chkAutoSchedule.AutoSize = true;
        this.chkAutoSchedule.Checked = true;
        this.chkAutoSchedule.CheckState = System.Windows.Forms.CheckState.Checked;
        this.chkAutoSchedule.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.chkAutoSchedule.ForeColor = System.Drawing.Color.White;
        this.chkAutoSchedule.Location = new System.Drawing.Point(15, 30);
        this.chkAutoSchedule.Name = "chkAutoSchedule";
        this.chkAutoSchedule.Size = new System.Drawing.Size(148, 19);
        this.chkAutoSchedule.TabIndex = 0;
        this.chkAutoSchedule.Text = "Daily Auto-Sync (06:00)";
        this.chkAutoSchedule.UseVisualStyleBackColor = true;
        this.chkAutoSchedule.CheckedChanged += new System.EventHandler(this.chkAutoSchedule_CheckedChanged);

        // 
        // lblScheduleInfo
        // 
        this.lblScheduleInfo.Font = new System.Drawing.Font("Segoe UI", 8F);
        this.lblScheduleInfo.ForeColor = System.Drawing.Color.FromArgb(189, 195, 199);
        this.lblScheduleInfo.Location = new System.Drawing.Point(12, 60);
        this.lblScheduleInfo.Name = "lblScheduleInfo";
        this.lblScheduleInfo.Size = new System.Drawing.Size(185, 65);
        this.lblScheduleInfo.TabIndex = 1;
        this.lblScheduleInfo.Text = "Runs automatically every day at 06:00 AM. Minimize application to keep running in background tray.";

        // 
        // btnSync
        // 
        this.btnSync.BackColor = System.Drawing.Color.FromArgb(41, 128, 185);
        this.btnSync.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnSync.FlatAppearance.BorderSize = 0;
        this.btnSync.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnSync.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.btnSync.ForeColor = System.Drawing.Color.White;
        this.btnSync.Location = new System.Drawing.Point(25, 160);
        this.btnSync.Name = "btnSync";
        this.btnSync.Size = new System.Drawing.Size(260, 48);
        this.btnSync.TabIndex = 3;
        this.btnSync.Text = "🚀 START SYNC NOW";
        this.btnSync.UseVisualStyleBackColor = false;
        this.btnSync.Click += new System.EventHandler(this.btnSync_Click);

        // 
        // btnTestConnections
        // 
        this.btnTestConnections.BackColor = System.Drawing.Color.FromArgb(52, 73, 94);
        this.btnTestConnections.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnTestConnections.FlatAppearance.BorderSize = 0;
        this.btnTestConnections.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnTestConnections.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.btnTestConnections.ForeColor = System.Drawing.Color.White;
        this.btnTestConnections.Location = new System.Drawing.Point(300, 160);
        this.btnTestConnections.Name = "btnTestConnections";
        this.btnTestConnections.Size = new System.Drawing.Size(190, 48);
        this.btnTestConnections.TabIndex = 4;
        this.btnTestConnections.Text = "🔍 TEST CONNECTIONS";
        this.btnTestConnections.UseVisualStyleBackColor = false;
        this.btnTestConnections.Click += new System.EventHandler(this.btnTestConnections_Click);

        // 
        // btnClearLog
        // 
        this.btnClearLog.BackColor = System.Drawing.Color.FromArgb(44, 62, 80);
        this.btnClearLog.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnClearLog.FlatAppearance.BorderSize = 0;
        this.btnClearLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnClearLog.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.btnClearLog.ForeColor = System.Drawing.Color.FromArgb(189, 195, 199);
        this.btnClearLog.Location = new System.Drawing.Point(505, 160);
        this.btnClearLog.Name = "btnClearLog";
        this.btnClearLog.Size = new System.Drawing.Size(120, 48);
        this.btnClearLog.TabIndex = 5;
        this.btnClearLog.Text = "🗑️ Clear Log";
        this.btnClearLog.UseVisualStyleBackColor = false;
        this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);

        // 
        // logPanel
        // 
        this.logPanel.BackColor = System.Drawing.Color.FromArgb(34, 36, 42);
        this.logPanel.Controls.Add(this.txtLog);
        this.logPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.logPanel.Location = new System.Drawing.Point(0, 305);
        this.logPanel.Name = "logPanel";
        this.logPanel.Padding = new System.Windows.Forms.Padding(25, 5, 25, 5);
        this.logPanel.Size = new System.Drawing.Size(980, 340);
        this.logPanel.TabIndex = 2;

        // 
        // txtLog
        // 
        this.txtLog.BackColor = System.Drawing.Color.FromArgb(18, 19, 22);
        this.txtLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtLog.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.txtLog.ForeColor = System.Drawing.Color.FromArgb(46, 204, 113);
        this.txtLog.Multiline = true;
        this.txtLog.Name = "txtLog";
        this.txtLog.ReadOnly = true;
        this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtLog.TabIndex = 0;

        // 
        // statusStripPanel
        // 
        this.statusStripPanel.BackColor = System.Drawing.Color.FromArgb(15, 16, 18);
        this.statusStripPanel.Controls.Add(this.lblStatus);
        this.statusStripPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.statusStripPanel.Location = new System.Drawing.Point(0, 645);
        this.statusStripPanel.Name = "statusStripPanel";
        this.statusStripPanel.Padding = new System.Windows.Forms.Padding(25, 0, 25, 0);
        this.statusStripPanel.Size = new System.Drawing.Size(980, 35);
        this.statusStripPanel.TabIndex = 3;

        // 
        // lblStatus
        // 
        this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(189, 195, 199);
        this.lblStatus.Location = new System.Drawing.Point(25, 0);
        this.lblStatus.Name = "lblStatus";
        this.lblStatus.Size = new System.Drawing.Size(930, 35);
        this.lblStatus.TabIndex = 0;
        this.lblStatus.Text = "Ready. Next auto-sync scheduled at 06:00 AM.";
        this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        // 
        // Assembly
        // 
        this.Controls.Add(this.logPanel);
        this.Controls.Add(this.statusStripPanel);
        this.Controls.Add(this.configPanel);
        this.Controls.Add(this.topPanel);

        this.topPanel.ResumeLayout(false);
        this.topPanel.PerformLayout();
        this.configPanel.ResumeLayout(false);
        this.grpSources.ResumeLayout(false);
        this.grpSources.PerformLayout();
        this.grpDate.ResumeLayout(false);
        this.grpDate.PerformLayout();
        this.grpSchedule.ResumeLayout(false);
        this.grpSchedule.PerformLayout();
        this.logPanel.ResumeLayout(false);
        this.logPanel.PerformLayout();
        this.statusStripPanel.ResumeLayout(false);
        this.ResumeLayout(false);
    }
}

