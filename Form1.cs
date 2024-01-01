using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Globalization;
using System.IO;

namespace WinFormsActiveTango
{
    public partial class Form1 : Form
    {
        private const string CorrectPin = "1234";
        private const int MinutesUntilBlock = 1;
        private Label countdownLabel;
        private TextBox minutesBox;
        private Button updateButton;
        private BlockScreenForm blockScreenForm;
        private System.Windows.Forms.Timer timer;
        private const string ClosePin = "4321";
        private TextBox closePinBox;
        private Button closeButton;
        private bool allowClose = false;
        private int secondsCounter = 0;

        private TabControl tabControl;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabPage tabPage3;

        private AnalysisTab analysisTab;
        private TasksTab tasksTab;
        private ApplicationsTab applicationsTab;
        private Button unlockButton;
        private int MinutesUntilBlock_updated = 1;

        private GroupBox timeBucketsGroupBox;
        private ListBox timeBucketsListBox;
        private Button useButton;
        private Button loadTimeBucketButton;

        public Form1()
        {
            InitializeComponent();

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=tasks.db;Version=3;"))
            {
                conn.Open();

                string sql = @"CREATE TABLE IF NOT EXISTS TimeBuckets 
               (
                   ID INTEGER PRIMARY KEY, 
                   BucketName TEXT, 
                   Time TEXT, 
                   Used INTEGER DEFAULT 0,
                   TimeStamp_UseBy TEXT,
                   TimeStamp_Used TEXT
               )";

                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    command.ExecuteNonQuery();
                }
            }


            this.Text = "Action Tango";
            this.WindowState = FormWindowState.Maximized;
            this.Icon = new Icon("favicon.ico");

            tabPage1 = new TabPage("Tasks");

            countdownLabel = new Label { Location = new Point(10, 20), Font = new Font("Arial", 16) };

            minutesBox = new TextBox { Location = new Point(10, 20) };
            updateButton = new Button { Text = "Update Timer", Location = new Point(10, 50) };
            updateButton.Click += updateButton_Click;

            closePinBox = new TextBox { Location = new Point(10, 20) };
            closeButton = new Button { Text = "Close", Location = new Point(10, 50) };
            closeButton.Click += closeButton_Click;

            unlockButton = new Button { Text = "Unlock", Location = new Point(10, 20) };
            unlockButton.Click += unlockButton_Click;
            
            var countdownGroup = new GroupBox { Text = "Countdown", Location = new Point(50, 10), Size = new Size(200, 70) };
            countdownGroup.Controls.Add(countdownLabel);

            var pinGroup = new GroupBox { Text = "Unlock Screen", Location = new Point(50, countdownGroup.Bottom + 10), Size = new Size(200, 50) };
            pinGroup.Controls.Add(unlockButton);

            // Add Time Buckets group box after countdownGroup
            timeBucketsGroupBox = new GroupBox { Text = "Time Buckets", Location = new Point(50, pinGroup.Bottom + 10), Size = new Size(200, 150) };
            tabPage1.Controls.Add(timeBucketsGroupBox);

            timeBucketsListBox = new ListBox { Location = new Point(10, 20), Size = new Size(timeBucketsGroupBox.Width - 20, 50) };
            timeBucketsGroupBox.Controls.Add(timeBucketsListBox);
            LoadTimeBuckets();

            useButton = new Button { Text = "Use", Location = new Point(10, timeBucketsListBox.Bottom + 10), Size = new Size(timeBucketsGroupBox.Width - 20, 20) };
            useButton.Click += UseButton_Click;
            timeBucketsGroupBox.Controls.Add(useButton);

            loadTimeBucketButton = new Button { Text = "Load Time Bucket", Location = new Point(10, useButton.Bottom + 10), Size = new Size(timeBucketsGroupBox.Width - 20, 20) };
            loadTimeBucketButton.Click += LoadTimeBucketButton_Click;
            timeBucketsGroupBox.Controls.Add(loadTimeBucketButton);

            var updateGroup = new GroupBox { Text = "Update Timer", Location = new Point(50, timeBucketsGroupBox.Bottom + 10), Size = new Size(200, 100) };
            updateGroup.Controls.Add(minutesBox);
            updateGroup.Controls.Add(updateButton);

            var closeGroup = new GroupBox { Text = "Close Application", Location = new Point(50, updateGroup.Bottom + 10), Size = new Size(200, 100) };
            closeGroup.Controls.Add(closePinBox);
            closeGroup.Controls.Add(closeButton);


            tabPage1.Controls.Add(countdownGroup);
            tabPage1.Controls.Add(pinGroup);
            tabPage1.Controls.Add(timeBucketsGroupBox);
            tabPage1.Controls.Add(updateGroup);
            tabPage1.Controls.Add(closeGroup);

            tabPage2 = new TabPage("Applications");

            tabControl = new TabControl
            {
                Location = new Point(10, 10),
                Size = new Size(1000, 600),
                ItemSize = new Size(100, 20) // Set the width and height of the tabs
            };
            tabControl.TabPages.Add(tabPage1);
            tabControl.TabPages.Add(tabPage2);

            Controls.Add(tabControl);

            tasksTab = new TasksTab(tabPage1);
            applicationsTab = new ApplicationsTab(tabPage2);

            tabPage3 = new TabPage("Analysis");
            tabControl.TabPages.Add(tabPage3);
            analysisTab = new AnalysisTab(tabPage3);

            Controls.Add(tabControl);

            blockScreenForm = new BlockScreenForm(MinutesUntilBlock);

            timer = new System.Windows.Forms.Timer { Interval = 1000 };
            timer.Tick += (sender, e) => UpdateCountdown();
            timer.Start();

            
        }

        private void LoadTimeBuckets()
        {
            timeBucketsListBox.Items.Clear();

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=tasks.db;Version=3;"))
            {
                conn.Open();

                string sql = "SELECT * FROM TimeBuckets WHERE Used = 0 AND date(TimeStamp_UseBy) = date('now')";

                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TimeBucket timeBucket = new TimeBucket
                            {
                                ID = (long)reader["ID"],
                                BucketName = reader["BucketName"].ToString(),
                                Time = reader["Time"].ToString()
                            };
                            timeBucketsListBox.Items.Add(timeBucket);
                        }
                    }
                }
            }
        }

        private void LoadTimeBucketButton_Click(object sender, EventArgs e)
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=tasks.db;Version=3;"))
            {
                conn.Open();

                try
                {
                    // Read the CSV file
                    using (StreamReader reader = new StreamReader("TimeBuckets.csv"))
                    {
                        reader.ReadLine(); // Skip the header

                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            string[] fields = line.Split(',');

                            string id = fields[0];
                            string bucketName = fields[1];
                            string time = fields[2];
                            string timeStamp_UseBy = fields[3];

                            // Check if the row already exists in the TimeBuckets table
                            string sql = "SELECT COUNT(*) FROM TimeBuckets WHERE ID = @ID";

                            using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                            {
                                command.Parameters.AddWithValue("@ID", id);

                                int count = Convert.ToInt32(command.ExecuteScalar());

                                if (count == 0)
                                {
                                    // The row does not exist, insert it into the TimeBuckets table
                                    sql = "INSERT INTO TimeBuckets (ID, BucketName, Time, TimeStamp_UseBy) VALUES (@ID, @BucketName, @Time, @TimeStamp_UseBy)";

                                    using (SQLiteCommand insertCommand = new SQLiteCommand(sql, conn))
                                    {
                                        insertCommand.Parameters.AddWithValue("@ID", id);
                                        insertCommand.Parameters.AddWithValue("@BucketName", bucketName);
                                        insertCommand.Parameters.AddWithValue("@Time", time);
                                        insertCommand.Parameters.AddWithValue("@TimeStamp_UseBy", timeStamp_UseBy);

                                        insertCommand.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }
                }
                catch (IOException)
                {
                    MessageBox.Show("The file is in use by another process. Please close the file and try again.");
                    return;
                }
            }

            // Reload the time buckets into the ListBox
            LoadTimeBuckets();
        }

        private void unlockButton_Click(object sender, EventArgs e)
        {
            UnlockScreenForm pinEntryForm = new UnlockScreenForm(MinutesUntilBlock_updated);

            if (pinEntryForm.ShowDialog() == DialogResult.OK)
            {
                blockScreenForm.Hide();
                blockScreenForm.ResetBlockTime(MinutesUntilBlock);
                MinutesUntilBlock_updated = 1;
                timer.Stop();
                timer.Start();
            }
        }

        private void UpdateCountdown()
        {
            var remainingTime = blockScreenForm.BlockTime - DateTime.Now;           
            countdownLabel.Text = remainingTime.ToString(@"hh\:mm\:ss");

            if (remainingTime.TotalMinutes < 1)
            {
                secondsCounter++;
                if (secondsCounter >= 15)
                {
                    if (!blockScreenForm.Visible)
                    {
                        System.Media.SoundPlayer player = new System.Media.SoundPlayer("notification1.wav");
                        player.Play();
                    }
                    secondsCounter = 0;
                }
            }
            else
            {
                secondsCounter = 0;
            }
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            if (int.TryParse(minutesBox.Text, out int newMinutes))
            {
                MinutesUntilBlock_updated = (blockScreenForm.BlockTime - DateTime.Now).Minutes;
                MinutesUntilBlock_updated += newMinutes;
                blockScreenForm.UpdateTimer(MinutesUntilBlock_updated);
                timer.Stop();
                timer.Start();
            }
            else
            {
                MessageBox.Show("Invalid input. Please enter a number.");
                minutesBox.Clear();
            }
        }

        private void UseButton_Click(object sender, EventArgs e)
        {
            if (timeBucketsListBox.SelectedItem != null)
            {
                TimeBucket selectedTimeBucket = (TimeBucket)timeBucketsListBox.SelectedItem;

                // Add the selected number of minutes to the countdown timer
                MinutesUntilBlock_updated += int.Parse(selectedTimeBucket.Time);
                blockScreenForm.UpdateTimer(MinutesUntilBlock_updated);
                timer.Stop();
                timer.Start();

                // Mark the selected item as used in the TimeBuckets table
                using (SQLiteConnection conn = new SQLiteConnection("Data Source=tasks.db;Version=3;"))
                {
                    conn.Open();

                    string sql = "UPDATE TimeBuckets SET Used = 1, TimeStamp_Used = @TimeStamp_Used WHERE ID = @ID";

                    using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                    {
                        command.Parameters.AddWithValue("@ID", selectedTimeBucket.ID);
                        command.Parameters.AddWithValue("@TimeStamp_Used", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        command.ExecuteNonQuery();
                    }
                }

                // Reload the time buckets
                LoadTimeBuckets();
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            if (closePinBox.Text == ClosePin)
            {
                allowClose = true;
                Application.Exit();
            }
            else
            {
                MessageBox.Show("Incorrect pin. Please try again.");
                closePinBox.Clear();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!allowClose)
            {
                e.Cancel = true;
            }
            base.OnFormClosing(e);
        }
    }
}