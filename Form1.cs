using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SQLite;

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

        private DataGridView todoDataGridView;
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

            todoDataGridView = new DataGridView { Location = new Point(300, 10), Size = new Size(650, 300), AutoGenerateColumns = false, AllowUserToAddRows = false };
            todoDataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", DataPropertyName = "Name", HeaderText = "Task", Width = 250 });
            todoDataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Priority", DataPropertyName = "Priority", HeaderText = "Priority", Width = 50 });
            todoDataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "DueDate", DataPropertyName = "DueDate", HeaderText = "Due Date", Width = 150 });
            todoDataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", DataPropertyName = "Status", HeaderText = "Status", Width = 100 });
            if (todoDataGridView.Rows.Count > 0)
            {
                todoDataGridView.Sort(todoDataGridView.Columns["DueDate"], ListSortDirection.Ascending);
            }

            createTaskButton = new Button { Text = "Create Task", Location = new Point(300, 320) };
            createTaskButton.Click += createTaskButton_Click;

            contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Items.Add("Mark as Done", null, markAsDone_Click);
            todoDataGridView.ContextMenuStrip = contextMenuStrip;

            Controls.Add(todoDataGridView);
            Controls.Add(createTaskButton);

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=tasks.db;Version=3;"))
            {
                conn.Open();

                string sql = "CREATE TABLE IF NOT EXISTS Tasks (ID INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, Priority TEXT, DueDate TEXT, Status TEXT)";

                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    command.ExecuteNonQuery();
                }

                sql = "SELECT * FROM Tasks";

                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int rowIndex = todoDataGridView.Rows.Add(reader["Name"], reader["Priority"], reader["DueDate"], reader["Status"]);
                            todoDataGridView.Rows[rowIndex].Tag = reader["ID"];
                        }
                    }
                }
            }

            todoDataGridView.CellEndEdit += todoDataGridView_CellEndEdit; //

        }

        private void createTaskButton_Click(object sender, EventArgs e)
        {
            var createTaskForm = new CreateTaskForm();
            if (createTaskForm.ShowDialog() == DialogResult.OK)
            {
                using (SQLiteConnection conn = new SQLiteConnection("Data Source=tasks.db;Version=3;"))
                {
                    conn.Open();

                    string sql = "INSERT INTO Tasks (Name, Priority, DueDate, Status) VALUES (@Name, @Priority, @DueDate, @Status)";

                    using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                    {
                        command.Parameters.AddWithValue("@Name", createTaskForm.Task.Name);
                        command.Parameters.AddWithValue("@Priority", createTaskForm.Task.Priority);
                        command.Parameters.AddWithValue("@DueDate", createTaskForm.Task.DueDate);
                        command.Parameters.AddWithValue("@Status", createTaskForm.Task.Status);

                        command.ExecuteNonQuery();

                        // Get the ID of the inserted row
                        long taskId = conn.LastInsertRowId;

                        // Add the row to the DataGridView and store the task ID in the Tag property
                        int rowIndex = todoDataGridView.Rows.Add(createTaskForm.Task.Name, createTaskForm.Task.Priority, createTaskForm.Task.DueDate, createTaskForm.Task.Status);
                        todoDataGridView.Rows[rowIndex].Tag = taskId;
                    }
                }
            }
        }

        private void markAsDone_Click(object sender, EventArgs e)
        {
            if (todoDataGridView.SelectedCells.Count > 0)
            {
                int selectedrowindex = todoDataGridView.SelectedCells[0].RowIndex;
                DataGridViewRow selectedRow = todoDataGridView.Rows[selectedrowindex];
                selectedRow.Cells["Status"].Value = "Done";

                using (SQLiteConnection conn = new SQLiteConnection("Data Source=tasks.db;Version=3;"))
                {
                    conn.Open();

                    string sql = "UPDATE Tasks SET Status = 'Done' WHERE ID = @ID";

                    using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                    {
                        command.Parameters.AddWithValue("@ID", (long)selectedRow.Tag);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private void todoDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow editedRow = todoDataGridView.Rows[e.RowIndex];
            long taskId = (long)editedRow.Tag;

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=tasks.db;Version=3;"))
            {
                conn.Open();

                string sql = "UPDATE Tasks SET Name = @Name, Priority = @Priority, DueDate = @DueDate, Status = @Status WHERE ID = @ID";

                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@Name", editedRow.Cells["Name"].Value);
                    command.Parameters.AddWithValue("@Priority", editedRow.Cells["Priority"].Value);
                    command.Parameters.AddWithValue("@DueDate", editedRow.Cells["DueDate"].Value);
                    command.Parameters.AddWithValue("@Status", editedRow.Cells["Status"].Value);
                    command.Parameters.AddWithValue("@ID", taskId);

                    command.ExecuteNonQuery();
                }
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