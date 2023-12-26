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
        private BlockScreenForm blockScreenForm;

        public Form1()
        {
            InitializeComponent();

            pinBox = new TextBox { Location = new System.Drawing.Point(50, 50) };
            submitButton = new Button { Text = "Submit", Location = new System.Drawing.Point(50, 80) };
            submitButton.Click += submitButton_Click;

            Controls.Add(pinBox);
            Controls.Add(submitButton);

            blockScreenForm = new BlockScreenForm();

            var timer = new System.Windows.Forms.Timer { Interval = MinutesUntilBlock * 60 * 1000 };
            timer.Tick += (sender, e) => blockScreenForm.BlockSecondDisplay();
            timer.Start();
        }

        private void submitButton_Click(object sender, EventArgs e)
        {
            if (pinBox.Text == CorrectPin)
            {
                blockScreenForm.Hide();
            }
            else
            {
                MessageBox.Show("Incorrect pin. Please try again.");
                pinBox.Clear();
            }
        }
    }

}
