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

            blockScreenForm = new BlockScreenForm(MinutesUntilBlock);

            timer = new System.Windows.Forms.Timer { Interval = 1000 }; // Update every second
            timer.Tick += (sender, e) => UpdateCountdown();
            timer.Start();
        }

        private void UpdateCountdown()
        {
            var remainingTime = blockScreenForm.BlockTime - DateTime.Now;
            countdownLabel.Text = remainingTime.ToString(@"hh\:mm\:ss");
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
