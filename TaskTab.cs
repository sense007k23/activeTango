using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Globalization;
using System.IO;

namespace WinFormsActiveTango
{
    public class TasksTab
    {
        private DataGridView todoDataGridView;
        private Button createTaskButton;
        private ContextMenuStrip contextMenuStrip;
        private TabPage tabPage;
        private System.Windows.Forms.Timer timer;

        public TasksTab(TabPage tabPage)
        {
            this.tabPage = tabPage;
            InitializeTasksTab();

            timer = new System.Windows.Forms.Timer { Interval = 60 * 1000 }; // 1 minute
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in todoDataGridView.Rows)
            {
                if (row.Cells["DueDate"].Value != null && row.Cells["Status"].Value != null)
                {
                    DateTime dueDate = DateTime.ParseExact(row.Cells["DueDate"].Value.ToString(), "dd-MM-yyyy hh:mm tt", CultureInfo.InvariantCulture);
                    string status = row.Cells["Status"].Value.ToString();

                    // Check if the task is overdue and the status is "Pending"
                    if (dueDate.AddMinutes(-15) < DateTime.Now && status == "Pending")
                    {
                        // Change the background color of the Status cell to light red
                        row.Cells["Status"].Style.BackColor = Color.LightCoral;
                    }
                    else
                    {
                        // Reset the background color of the Status cell
                        row.Cells["Status"].Style.BackColor = Color.White;
                    }
                }
            }
        }

        private void InitializeTasksTab()
        {
            todoDataGridView = new DataGridView { Location = new Point(300, 10), Size = new Size(650, 300), AutoGenerateColumns = false, AllowUserToAddRows = true };
            todoDataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", DataPropertyName = "Name", HeaderText = "Task", Width = 250 });
            todoDataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Priority", DataPropertyName = "Priority", HeaderText = "Priority", Width = 50 });
            todoDataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "DueDate", DataPropertyName = "DueDate", HeaderText = "Due Date", Width = 150 });
            todoDataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", DataPropertyName = "Status", HeaderText = "Status", Width = 100 });
            if (todoDataGridView.Rows.Count > 0)
            {
                todoDataGridView.Sort(todoDataGridView.Columns["DueDate"], ListSortDirection.Ascending);
            }

            GroupBox taskGroupBox = new GroupBox { Text = "Task Actions", Location = new Point(300, 320), Size = new Size(200, 50) };
            tabPage.Controls.Add(taskGroupBox);

            createTaskButton = new Button { Text = "Create Task", Location = new Point(10, 20) };
            createTaskButton.Click += createTaskButton_Click;
            taskGroupBox.Controls.Add(createTaskButton);

            Button inboxTasksButton = new Button { Text = "InboxTasks", Location = new Point(110, 20) };
            inboxTasksButton.Click += refreshButton_Click;
            taskGroupBox.Controls.Add(inboxTasksButton);

            contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Items.Add("Mark as Done", null, markAsDone_Click);
            contextMenuStrip.Items.Add("Delete Task", null, deleteTask_Click);
            todoDataGridView.ContextMenuStrip = contextMenuStrip;

            tabPage.Controls.Add(todoDataGridView);

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

            todoDataGridView.CellEndEdit += todoDataGridView_CellEndEdit;
            todoDataGridView.CellFormatting += todoDataGridView_CellFormatting;
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

        private void refreshButton_Click(object sender, EventArgs e)
        {
            using (StreamReader reader = new StreamReader("TaskInbox.csv"))
            {
                reader.ReadLine(); // Skip the header row

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] fields = line.Split(',');

                    string taskName = fields[0];
                    string dueDate = fields[1];
                    string priority = fields[2];

                    // Check if the task already exists in the DataGridView
                    bool isDuplicate = false;
                    foreach (DataGridViewRow row in todoDataGridView.Rows)
                    {
                        if ((string)row.Cells["Name"].Value == taskName && (string)row.Cells["DueDate"].Value == dueDate)
                        {
                            // The task already exists, skip it
                            isDuplicate = true;
                            break;
                        }
                    }

                    if (isDuplicate)
                    {
                        // The task already exists, skip it
                        continue;
                    }

                    // The task does not exist, add it to the DataGridView and the SQLite database
                    using (SQLiteConnection conn = new SQLiteConnection("Data Source=tasks.db;Version=3;"))
                    {
                        conn.Open();

                        string sql = "INSERT INTO Tasks (Name, Priority, DueDate, Status) VALUES (@Name, @Priority, @DueDate, 'Pending')";

                        using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                        {
                            command.Parameters.AddWithValue("@Name", taskName);
                            command.Parameters.AddWithValue("@Priority", priority);
                            command.Parameters.AddWithValue("@DueDate", dueDate);

                            command.ExecuteNonQuery();

                            // Get the ID of the inserted row
                            long taskId = conn.LastInsertRowId;

                            // Add the row to the DataGridView and store the task ID in the Tag property
                            int rowIndex = todoDataGridView.Rows.Add(taskName, priority, dueDate, "Pending");
                            todoDataGridView.Rows[rowIndex].Tag = taskId;
                        }
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

        private void deleteTask_Click(object sender, EventArgs e)
        {
            if (todoDataGridView.SelectedCells.Count > 0)
            {
                int selectedrowindex = todoDataGridView.SelectedCells[0].RowIndex;
                DataGridViewRow selectedRow = todoDataGridView.Rows[selectedrowindex];
                long taskId = (long)selectedRow.Tag;

                using (SQLiteConnection conn = new SQLiteConnection("Data Source=tasks.db;Version=3;"))
                {
                    conn.Open();

                    string sql = "DELETE FROM Tasks WHERE ID = @ID";

                    using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                    {
                        command.Parameters.AddWithValue("@ID", taskId);

                        command.ExecuteNonQuery();
                    }
                }

                todoDataGridView.Rows.RemoveAt(selectedrowindex);
            }
        }

        private void todoDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow editedRow = todoDataGridView.Rows[e.RowIndex];

            // Check if the DueDate cell is empty
            if (editedRow.Cells["DueDate"].Value == null || string.IsNullOrWhiteSpace(editedRow.Cells["DueDate"].Value.ToString()))
            {
                // Set the DueDate cell to the current date and time plus 15 minutes, rounded to the nearest 5 minutes
                DateTime now = DateTime.Now;
                DateTime roundedTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute / 5 * 5, 0);
                DateTime dueDate = roundedTime.AddMinutes(15);
                editedRow.Cells["DueDate"].Value = dueDate.ToString("dd-MM-yyyy hh:mm tt");
            }

            // Check if the Status cell is empty
            if (editedRow.Cells["Status"].Value == null || string.IsNullOrWhiteSpace(editedRow.Cells["Status"].Value.ToString()))
            {
                // Set the Status cell to "Pending"
                editedRow.Cells["Status"].Value = "Pending";
            }

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=tasks.db;Version=3;"))
            {
                conn.Open();

                if (editedRow.Tag == null)
                {
                    // This is a new row, insert it into the database
                    string sql = "INSERT INTO Tasks (Name, Priority, DueDate, Status) VALUES (@Name, @Priority, @DueDate, @Status)";

                    using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                    {
                        command.Parameters.AddWithValue("@Name", editedRow.Cells["Name"].Value);
                        command.Parameters.AddWithValue("@Priority", editedRow.Cells["Priority"].Value);
                        command.Parameters.AddWithValue("@DueDate", editedRow.Cells["DueDate"].Value);
                        command.Parameters.AddWithValue("@Status", editedRow.Cells["Status"].Value);

                        command.ExecuteNonQuery();

                        // Get the ID of the inserted row
                        long taskId = conn.LastInsertRowId;

                        // Store the task ID in the Tag property
                        editedRow.Tag = taskId;
                    }
                }
                else
                {
                    // This is an existing row, update it in the database
                    long taskId = (long)editedRow.Tag;

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
        }

        private void todoDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridViewRow row = todoDataGridView.Rows[e.RowIndex];

            // Check if the DueDate and Status cells are not empty
            if (row.Cells["DueDate"].Value != null && !string.IsNullOrWhiteSpace(row.Cells["DueDate"].Value.ToString()) &&
                row.Cells["Status"].Value != null && !string.IsNullOrWhiteSpace(row.Cells["Status"].Value.ToString()))
            {
                try
                {
                    DateTime dueDate = DateTime.ParseExact(row.Cells["DueDate"].Value.ToString(), "dd-MM-yyyy hh:mm tt", CultureInfo.InvariantCulture);
                    string status = row.Cells["Status"].Value.ToString();

                    // Check if the task is overdue and the status is "Pending"
                    //if (dueDate.AddMinutes(-15) < DateTime.Now && status == "Pending")
                    //{
                    //    // Change the background color of the Status cell to light red
                    //    row.Cells["Status"].Style.BackColor = Color.LightCoral;
                    //}
                    //else
                    //{
                    //    // Reset the background color of the Status cell
                    //    row.Cells["Status"].Style.BackColor = Color.White;
                    //}
                }
                catch (FormatException)
                {
                    // The DueDate cell contains a string that is not a valid date and time
                    // Handle the error here
                }
            }
        }
    }
}