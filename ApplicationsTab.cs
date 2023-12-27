using System;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinFormsActiveTango
{
    public class ApplicationsTab
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        private DataTable dt;
        private DataGridView dataGridView1;
        private TabPage tabPage;

        public ApplicationsTab(TabPage tabPage)
        {
            this.tabPage = tabPage;
            InitializeApplicationsTab();
        }

        private void InitializeApplicationsTab()
        {
            dt = new DataTable();
            dt.Columns.Add("Application", typeof(string));
            dt.Columns.Add("In Focus", typeof(bool));
            dt.Columns.Add("Screen", typeof(int));

            dataGridView1 = new DataGridView
            {
                Location = new Point(10, 10),
                Size = new Size(500, 500),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            tabPage.Controls.Add(dataGridView1);

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 3000; // 1 minute
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            dt.Clear();

            IntPtr foregroundWindow = GetForegroundWindow();

            foreach (Process process in Process.GetProcesses())
            {
                if (!string.IsNullOrEmpty(process.MainWindowTitle))
                {
                    DataRow row = dt.NewRow();
                    row["Application"] = process.MainWindowTitle;
                    row["In Focus"] = process.MainWindowHandle == foregroundWindow;

                    // Get the screen that the application's main window is located on
                    Screen screen = Screen.FromHandle(process.MainWindowHandle);
                    row["Screen"] = Array.IndexOf(Screen.AllScreens, screen) + 1;

                    dt.Rows.Add(row);
                }
            }

            dataGridView1.DataSource = dt;
        }
    }
}