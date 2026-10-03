using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseSyncApp;

public partial class Form1 : Form
{
    private readonly string sourceConnStr = "Server=ETCRESSQL02;Database=BTMTime;User Id=BTM_IT_RO;Password=ITreadonly!;";
    private readonly string destConnStr = "Server=BTMSQL01\\DIGITMONT_DEV;Database=CI_LSS;User Id=CI_Hub_User;Password=CiHubB4tam@excelitas;";

    public Form1()
    {
        InitializeComponent();
        LogMessage("System Ready. Awaiting user command...");
    }

    // Method to draw a subtle 3D shadow/border around controls
    private void Draw3DBorder(object sender, PaintEventArgs e)
    {
        Control control = (Control)sender;
        Graphics g = e.Graphics;
        
        // Draw bottom-right shadow (Darker)
        Pen shadowPen = new Pen(Color.FromArgb(15, 15, 18), 3);
        g.DrawLine(shadowPen, 0, control.Height - 1, control.Width, control.Height - 1);
        g.DrawLine(shadowPen, control.Width - 1, 0, control.Width - 1, control.Height);

        // Draw top-left highlight (Lighter)
        Pen highlightPen = new Pen(Color.FromArgb(70, 70, 80), 1);
        g.DrawLine(highlightPen, 0, 0, control.Width, 0);
        g.DrawLine(highlightPen, 0, 0, 0, control.Height);

        shadowPen.Dispose();
        highlightPen.Dispose();
    }

    private async void btnSync_Click(object sender, EventArgs e)
    {
        btnSync.Enabled = false;
        btnSync.Text = "⏳ SYNCING...";
        btnSync.BackColor = Color.FromArgb(230, 126, 34); // Change to Orange while processing
        txtLog.Clear();
        LogMessage("Starting synchronization process...");

        try
        {
            await Task.Run(() => SyncData());
            LogMessage("✅ Synchronization completed successfully!");
            btnSync.BackColor = Color.FromArgb(46, 204, 113); // Change to Green on success
            btnSync.Text = "✔️ SYNC COMPLETE";
        }
        catch (Exception ex)
        {
            LogMessage("❌ An error occurred: " + ex.Message);
            btnSync.BackColor = Color.FromArgb(231, 76, 60); // Change to Red on error
            btnSync.Text = "⚠️ SYNC FAILED";
        }
        finally
        {
            btnSync.Enabled = true;
            // Reset button style after 3 seconds
            await Task.Delay(3000);
            btnSync.BackColor = Color.FromArgb(52, 152, 219);
            btnSync.Text = "🚀 START SYNC";
        }
    }

    private void SyncData()
    {
        DataTable dtEmployee = new DataTable();

        LogMessage("Reading employee data from source server [BTMTime]...");
        using (SqlConnection sourceConn = new SqlConnection(sourceConnStr))
        {
            string query = @"
                SELECT 
                    EMP_NO, 
                    EMP_NAME, 
                    DEPARTMENT_CODE, 
                    COST_CENTRE_CODE, 
                    SECTION_CODE, 
                    WORKGROUP, 
                    PATTERN_CODE
                FROM EMPLOYEE
                WHERE TERMINATION_STATUS IS NULL OR LTRIM(RTRIM(TERMINATION_STATUS)) = ''";

            using (SqlCommand cmd = new SqlCommand(query, sourceConn))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    sourceConn.Open();
                    da.Fill(dtEmployee);
                }
            }
        }
        
        LogMessage($"Found {dtEmployee.Rows.Count} active employees.");
        if (dtEmployee.Rows.Count == 0) 
        {
            LogMessage("No records to sync. Aborting.");
            return;
        }

        LogMessage("Connecting to destination server [CI_LSS]...");
        using (SqlConnection destConn = new SqlConnection(destConnStr))
        {
            destConn.Open();

            LogMessage("Truncating old records in CI_Employee table...");
            using (SqlCommand cmdClear = new SqlCommand("TRUNCATE TABLE CI_Employee", destConn))
            {
                cmdClear.ExecuteNonQuery();
            }

            LogMessage("Initiating SqlBulkCopy transfer...");
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(destConn))
            {
                bulkCopy.DestinationTableName = "CI_Employee";
                bulkCopy.BulkCopyTimeout = 600;
                
                bulkCopy.ColumnMappings.Add("EMP_NO", "EMP_NO");
                bulkCopy.ColumnMappings.Add("EMP_NAME", "EMP_NAME");
                bulkCopy.ColumnMappings.Add("DEPARTMENT_CODE", "DEPARTMENT_CODE");
                bulkCopy.ColumnMappings.Add("COST_CENTRE_CODE", "COST_CENTRE_CODE");
                bulkCopy.ColumnMappings.Add("SECTION_CODE", "SECTION_CODE");
                bulkCopy.ColumnMappings.Add("WORKGROUP", "WORKGROUP");
                bulkCopy.ColumnMappings.Add("PATTERN_CODE", "PATTERN_CODE");

                bulkCopy.WriteToServer(dtEmployee);
            }
        }
    }

    private void LogMessage(string message)
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
