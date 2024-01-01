using System;
using System.Data;
using System.Data.SQLite;
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
        private ListBox listBox1;
        private Button exportButton;

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

            listBox1 = new ListBox
            {
                Location = new Point(520, 300),
                Size = new Size(480, 100)
            };
            tabPage.Controls.Add(listBox1);

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 3000; // 3sec
            timer.Tick += Timer_Tick;
            timer.Start();

            exportButton = new Button { Text = "Export", Location = new Point(520, 410), Size = new Size(480, 30) };
            exportButton.Click += ExportButton_Click;
            tabPage.Controls.Add(exportButton);


        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=applications.db;Version=3;"))
            {
                conn.Open();

                string sql = "SELECT ID, Application, InFocus, Screen, DATE(Timestamp) as Date FROM Applications";

                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        using (StreamWriter writer = new StreamWriter("Applications.csv"))
                        {
                            // Write the header
                            writer.WriteLine("ID,Application,InFocus,Screen,Date");

                            // Write the data
                            while (reader.Read())
                            {
                                string id = reader["ID"].ToString();
                                string application = reader["Application"].ToString().Replace(",", ";");
                                string inFocus = reader["InFocus"].ToString();
                                string screen = reader["Screen"].ToString();
                                string date = reader["Date"].ToString();

                                writer.WriteLine($"{id},{application},{inFocus},{screen},{date}");
                            }
                        }
                    }
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            dt.Clear();

            IntPtr foregroundWindow = GetForegroundWindow();

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=applications.db;Version=3;"))
            {
                conn.Open();

                string sql = "CREATE TABLE IF NOT EXISTS Applications (ID INTEGER PRIMARY KEY AUTOINCREMENT, Application TEXT, InFocus INTEGER, Screen INTEGER, Timestamp DATETIME)";

                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    command.ExecuteNonQuery();
                }

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

                        sql = "INSERT INTO Applications (Application, InFocus, Screen, Timestamp) VALUES (@Application, @InFocus, @Screen, @Timestamp)";

                        using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                        {
                            command.Parameters.AddWithValue("@Application", process.MainWindowTitle);
                            command.Parameters.AddWithValue("@InFocus", process.MainWindowHandle == foregroundWindow ? 1 : 0);
                            command.Parameters.AddWithValue("@Screen", Array.IndexOf(Screen.AllScreens, screen) + 1);

                            // Get the current time in the Asia/Kolkata timezone
                            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
                            DateTime currentTime = TimeZoneInfo.ConvertTime(DateTime.Now, timeZone);
                            command.Parameters.AddWithValue("@Timestamp", currentTime);

                            command.ExecuteNonQuery();
                        }
                    }
                }
            }

            dataGridView1.DataSource = dt;

            ShowTopApplications();
        }

        private void ShowTopApplications()
        {
            listBox1.Items.Clear(); // Clear the ListBox

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=applications.db;Version=3;"))
            {
                conn.Open();

                string sql = "SELECT Application, SUM(InFocus) as SecondsInFocus FROM Applications WHERE InFocus = 1 GROUP BY Application ORDER BY SecondsInFocus DESC LIMIT 3";

                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string application = reader["Application"].ToString();
                            int secondsInFocus = Convert.ToInt32(reader["SecondsInFocus"]);

                            // Display the application and count
                            listBox1.Items.Add($"{application}: {secondsInFocus} seconds");
                        }
                    }
                }
            }
        }

    }
}