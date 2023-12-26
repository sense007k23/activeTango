using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsActiveTango
{
    public partial class Form1 : Form
    {
        private const string CorrectPin = "1234";
        private const int MinutesUntilBlock = 1;
        private TextBox pinBox;
        private Button submitButton;
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

        private ListBox todoListBox;
        private Button createTaskButton;
        private ContextMenuStrip contextMenuStrip;

        public Form1()
        {
            InitializeComponent();

            pinBox = new TextBox { Location = new Point(10, 20) };
            submitButton = new Button { Text = "Submit", Location = new Point(10, 50) };
            submitButton.Click += submitButton_Click;

            countdownLabel = new Label { Location = new Point(10, 20), Font = new Font("Arial", 16) };

            minutesBox = new TextBox { Location = new Point(10, 20) };
            updateButton = new Button { Text = "Update Timer", Location = new Point(10, 50) };
            updateButton.Click += updateButton_Click;

            closePinBox = new TextBox { Location = new Point(10, 20) };
            closeButton = new Button { Text = "Close", Location = new Point(10, 50) };
            closeButton.Click += closeButton_Click;

            var pinGroup = new GroupBox { Text = "Pin Entry", Location = new Point(50, 90), Size = new Size(200, 100) };
            pinGroup.Controls.Add(pinBox);
            pinGroup.Controls.Add(submitButton);

            var countdownGroup = new GroupBox { Text = "Countdown", Location = new Point(50, 10), Size = new Size(200, 70) };
            countdownGroup.Controls.Add(countdownLabel);

            var updateGroup = new GroupBox { Text = "Update Timer", Location = new Point(50, 190), Size = new Size(200, 100) };
            updateGroup.Controls.Add(minutesBox);
            updateGroup.Controls.Add(updateButton);

            var closeGroup = new GroupBox { Text = "Close Application", Location = new Point(50, 300), Size = new Size(200, 100) };
            closeGroup.Controls.Add(closePinBox);
            closeGroup.Controls.Add(closeButton);

            Controls.Add(pinGroup);
            Controls.Add(countdownGroup);
            Controls.Add(updateGroup);
            Controls.Add(closeGroup);

            blockScreenForm = new BlockScreenForm(MinutesUntilBlock);

            timer = new System.Windows.Forms.Timer { Interval = 1000 };
            timer.Tick += (sender, e) => UpdateCountdown();
            timer.Start();

            todoListBox = new ListBox { Location = new Point(300, 10), Size = new Size(300, 300) }; 
            createTaskButton = new Button { Text = "Create Task", Location = new Point(300, 320) };
            createTaskButton.Click += createTaskButton_Click;

            contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Items.Add("Mark as Done", null, markAsDone_Click);
            todoListBox.ContextMenuStrip = contextMenuStrip;

            Controls.Add(todoListBox);
            Controls.Add(createTaskButton);



        }

        private void createTaskButton_Click(object sender, EventArgs e)
        {
            var createTaskForm = new CreateTaskForm();
            if (createTaskForm.ShowDialog() == DialogResult.OK)
            {
                todoListBox.Items.Add(createTaskForm.Task);
            }
        }

        private void markAsDone_Click(object sender, EventArgs e)
        {
            if (todoListBox.SelectedIndex != -1)
            {
                todoListBox.Items[todoListBox.SelectedIndex] += " (Done)";
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