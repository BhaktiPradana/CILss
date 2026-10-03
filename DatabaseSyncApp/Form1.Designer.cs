namespace DatabaseSyncApp;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.Button btnSync;
    private System.Windows.Forms.TextBox txtLog;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblSubtitle;
    private System.Windows.Forms.Panel mainPanel;
    private System.Windows.Forms.Panel logPanel;

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
        this.btnSync = new System.Windows.Forms.Button();
        this.txtLog = new System.Windows.Forms.TextBox();
        this.lblTitle = new System.Windows.Forms.Label();
        this.lblSubtitle = new System.Windows.Forms.Label();
        this.mainPanel = new System.Windows.Forms.Panel();
        this.logPanel = new System.Windows.Forms.Panel();
        
        this.mainPanel.SuspendLayout();
        this.logPanel.SuspendLayout();
        this.SuspendLayout();

        // 
        // Form1
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.FromArgb(24, 25, 30);
        this.ClientSize = new System.Drawing.Size(900, 600);
        this.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.ForeColor = System.Drawing.Color.White;
        this.Name = "Form1";
        this.Text = "CI Capacity - Data Sync Hub";
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

        // 
        // lblTitle
        // 
        this.lblTitle.AutoSize = true;
        this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(236, 240, 241);
        this.lblTitle.Location = new System.Drawing.Point(30, 30);
        this.lblTitle.Name = "lblTitle";
        this.lblTitle.Size = new System.Drawing.Size(400, 54);
        this.lblTitle.Text = "Data Synchronization";
        // 
        // lblSubtitle
        // 
        this.lblSubtitle.AutoSize = true;
        this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(149, 165, 166);
        this.lblSubtitle.Location = new System.Drawing.Point(35, 90);
        this.lblSubtitle.Name = "lblSubtitle";
        this.lblSubtitle.Size = new System.Drawing.Size(500, 25);
        this.lblSubtitle.Text = "Sync employee active records from BTMTime to CI_LSS Database.";

        // 
        // btnSync
        // 
        this.btnSync.BackColor = System.Drawing.Color.FromArgb(52, 152, 219);
        this.btnSync.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnSync.FlatAppearance.BorderSize = 0;
        this.btnSync.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(41, 128, 185);
        this.btnSync.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(31, 97, 141);
        this.btnSync.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnSync.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.btnSync.ForeColor = System.Drawing.Color.White;
        this.btnSync.Location = new System.Drawing.Point(35, 140);
        this.btnSync.Name = "btnSync";
        this.btnSync.Size = new System.Drawing.Size(220, 55);
        this.btnSync.TabIndex = 0;
        this.btnSync.Text = "🚀 START SYNC";
        this.btnSync.UseVisualStyleBackColor = false;
        this.btnSync.Click += new System.EventHandler(this.btnSync_Click);
        // Paint event for 3D shadow effect on button
        this.btnSync.Paint += new System.Windows.Forms.PaintEventHandler(this.Draw3DBorder);

        // 
        // logPanel
        // 
        this.logPanel.BackColor = System.Drawing.Color.FromArgb(34, 36, 42);
        this.logPanel.Controls.Add(this.txtLog);
        this.logPanel.Location = new System.Drawing.Point(35, 220);
        this.logPanel.Name = "logPanel";
        this.logPanel.Padding = new System.Windows.Forms.Padding(10);
        this.logPanel.Size = new System.Drawing.Size(810, 330);
        this.logPanel.TabIndex = 2;
        // Paint event for 3D raised border on log panel
        this.logPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.Draw3DBorder);

        // 
        // txtLog
        // 
        this.txtLog.BackColor = System.Drawing.Color.FromArgb(20, 21, 25);
        this.txtLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtLog.Font = new System.Drawing.Font("Consolas", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.txtLog.ForeColor = System.Drawing.Color.FromArgb(46, 204, 113);
        this.txtLog.Multiline = true;
        this.txtLog.Name = "txtLog";
        this.txtLog.ReadOnly = true;
        this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtLog.TabIndex = 1;

        // 
        // Layout
        // 
        this.Controls.Add(this.lblTitle);
        this.Controls.Add(this.lblSubtitle);
        this.Controls.Add(this.btnSync);
        this.Controls.Add(this.logPanel);

        this.mainPanel.ResumeLayout(false);
        this.logPanel.ResumeLayout(false);
        this.logPanel.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
