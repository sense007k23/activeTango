using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsActiveTango
{
    public partial class Form1 : Form
    {
        private const string CorrectPin = "1234";
        private const int MinutesUntilBlock = 1; // Change this to the desired number of minutes
        private TextBox pinBox;
        private Button submitButton;
        private Label countdownLabel;
        private TextBox minutesBox;
        private Button updateButton;
        private BlockScreenForm blockScreenForm;
        private System.Windows.Forms.Timer timer;

        public Form1()
        {
            InitializeComponent();

            pinBox = new TextBox { Location = new System.Drawing.Point(50, 50) };
            submitButton = new Button { Text = "Submit", Location = new System.Drawing.Point(50, 80) };
            submitButton.Click += submitButton_Click;

            countdownLabel = new Label { Location = new System.Drawing.Point(50, 110) };
            Controls.Add(countdownLabel);

            Controls.Add(pinBox);
            Controls.Add(submitButton);

            minutesBox = new TextBox { Location = new System.Drawing.Point(50, 140) };
            updateButton = new Button { Text = "Update Timer", Location = new System.Drawing.Point(50, 170) };
            updateButton.Click += updateButton_Click;

            Controls.Add(minutesBox);
            Controls.Add(updateButton);


            blockScreenForm = new BlockScreenForm(MinutesUntilBlock);

            timer = new System.Windows.Forms.Timer { Interval = 1000 }; // Update every second
            timer.Tick += (sender, e) => UpdateCountdown();
            timer.Start();
        }

        private int secondsCounter = 0;
        private void UpdateCountdown()
        {
            var remainingTime = blockScreenForm.BlockTime - DateTime.Now;
            countdownLabel.Text = remainingTime.ToString(@"hh\:mm\:ss");

            if (remainingTime.TotalMinutes < 1)
            {
                secondsCounter++;
                if (secondsCounter >= 15)
                {
                    if (!blockScreenForm.Visible) // Check if the blocker window is not visible
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

        private void submitButton_Click(object sender, EventArgs e)
        {
            if (pinBox.Text == CorrectPin)
            {
                blockScreenForm.Hide();
                blockScreenForm.ResetBlockTime(MinutesUntilBlock);
                timer.Stop();
                timer.Start();
            }
            else
            {
                MessageBox.Show("Incorrect pin. Please try again.");
                pinBox.Clear();
            }
        }
    }

}
