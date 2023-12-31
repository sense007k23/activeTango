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
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=tasks.db;Version=3;"))
            {
                conn.Open();

                string sql = "SELECT * FROM TimeBuckets";

                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string bucketName = reader["BucketName"].ToString();
                            string time = reader["Time"].ToString();
                            timeBucketsListBox.Items.Add(bucketName + "(" + time + ")");
                        }
                    }
                }
            }
        }

        private void LoadTimeBucketButton_Click(object sender, EventArgs e)
        {
            string header;

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=tasks.db;Version=3;"))
            {
                conn.Open();

                // Delete all rows from the TimeBuckets table
                string sql = "DELETE FROM TimeBuckets";

                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    command.ExecuteNonQuery();
                }

                // Read the CSV file
                using (StreamReader reader = new StreamReader("TimeBuckets.csv"))
                {
                    header = reader.ReadLine(); // Keep the header

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] fields = line.Split(',');

                        string bucketName = fields[0];
                        string time = fields[1];

                        // Insert the row into the TimeBuckets table
                        sql = "INSERT INTO TimeBuckets (BucketName, Time) VALUES (@BucketName, @Time)";

                        using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                        {
                            command.Parameters.AddWithValue("@BucketName", bucketName);
                            command.Parameters.AddWithValue("@Time", time);

                            command.ExecuteNonQuery();
                        }
                    }
                }
            }

            // Overwrite the CSV file with just the header
            using (StreamWriter writer = new StreamWriter("TimeBuckets.csv"))
            {
                writer.WriteLine(header);
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
                string selectedItem = timeBucketsListBox.SelectedItem.ToString();
                string bucketName = selectedItem.Substring(0, selectedItem.IndexOf("("));
                string time = selectedItem.Substring(selectedItem.IndexOf("(") + 1, selectedItem.IndexOf(")") - selectedItem.IndexOf("(") - 1);

                // Add the selected number of minutes to the countdown timer
                MinutesUntilBlock_updated += int.Parse(time);
                blockScreenForm.UpdateTimer(MinutesUntilBlock_updated);
                timer.Stop();
                timer.Start();

                // Delete the selected item from the TimeBuckets table
                using (SQLiteConnection conn = new SQLiteConnection("Data Source=tasks.db;Version=3;"))
                {
                    conn.Open();

                    string sql = "DELETE FROM TimeBuckets WHERE BucketName = @BucketName AND Time = @Time";

                    using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                    {
                        command.Parameters.AddWithValue("@BucketName", bucketName);
                        command.Parameters.AddWithValue("@Time", time);

                        command.ExecuteNonQuery();
                    }
                }

                // Remove the selected item from the ListBox
                timeBucketsListBox.Items.Remove(selectedItem);
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