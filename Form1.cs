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

        private TasksTab tasksTab;
        private ApplicationsTab applicationsTab;
        private Button unlockButton;
        private int MinutesUntilBlock_updated = 1;

        public Form1()
        {
            InitializeComponent();

            this.Text = "Action Tango";
            this.WindowState = FormWindowState.Maximized;
            this.Icon = new Icon("favicon.ico");
                       

            countdownLabel = new Label { Location = new Point(10, 20), Font = new Font("Arial", 16) };

            minutesBox = new TextBox { Location = new Point(10, 20) };
            updateButton = new Button { Text = "Update Timer", Location = new Point(10, 50) };
            updateButton.Click += updateButton_Click;

            closePinBox = new TextBox { Location = new Point(10, 20) };
            closeButton = new Button { Text = "Close", Location = new Point(10, 50) };
            closeButton.Click += closeButton_Click;

            unlockButton = new Button { Text = "Unlock", Location = new Point(10, 20) };
            unlockButton.Click += unlockButton_Click;

            
            var pinGroup = new GroupBox { Text = "Unlock Screen", Location = new Point(50, 90), Size = new Size(200, 50) };
            pinGroup.Controls.Add(unlockButton);

            var countdownGroup = new GroupBox { Text = "Countdown", Location = new Point(50, 10), Size = new Size(200, 70) };
            countdownGroup.Controls.Add(countdownLabel);

            var updateGroup = new GroupBox { Text = "Update Timer", Location = new Point(50, 190), Size = new Size(200, 100) };
            updateGroup.Controls.Add(minutesBox);
            updateGroup.Controls.Add(updateButton);

            var closeGroup = new GroupBox { Text = "Close Application", Location = new Point(50, 300), Size = new Size(200, 100) };
            closeGroup.Controls.Add(closePinBox);
            closeGroup.Controls.Add(closeButton);

            tabPage1 = new TabPage("Tasks");
            tabPage1.Controls.Add(pinGroup);
            tabPage1.Controls.Add(countdownGroup);
            tabPage1.Controls.Add(updateGroup);
            tabPage1.Controls.Add(closeGroup);

            tabPage2 = new TabPage("Applications");

            tabControl = new TabControl
            {
                Location = new Point(10, 10),
                Size = new Size(1000, 500),
                ItemSize = new Size(100, 20) // Set the width and height of the tabs
            };
            tabControl.TabPages.Add(tabPage1);
            tabControl.TabPages.Add(tabPage2);

            Controls.Add(tabControl);

            tasksTab = new TasksTab(tabPage1);
            applicationsTab = new ApplicationsTab(tabPage2);

            blockScreenForm = new BlockScreenForm(MinutesUntilBlock);

            timer = new System.Windows.Forms.Timer { Interval = 1000 };
            timer.Tick += (sender, e) => UpdateCountdown();
            timer.Start();

           
        }

        private void unlockButton_Click(object sender, EventArgs e)
        {
            UnlockScreenForm pinEntryForm = new UnlockScreenForm(MinutesUntilBlock_updated);
            

            if (pinEntryForm.ShowDialog() == DialogResult.OK)            {
                
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
                blockScreenForm.UpdateTimer(newMinutes);
                timer.Stop();
                timer.Start();
            }
            else
            {
                MessageBox.Show("Invalid input. Please enter a number.");
                minutesBox.Clear();
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