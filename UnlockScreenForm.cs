using System;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsActiveTango
{
    public partial class UnlockScreenForm : Form
    {
       

        private TextBox taskTextBox;
        private RadioButton[] focusLevelRadioButtons;
        private Button submitButton;
        private TextBox minutesSpentTextBox;
        private ListBox categoryListBox;

        public UnlockScreenForm(int minutesUntilBlock)
        {
            InitializeComponent();

            this.Size = new Size(500, 600); // Adjust the size of the form

            Label minutesSpentLabel = new Label { Text = "Minute Spent", Location = new Point(10, 10), Size = new Size(200, 13) };
            Controls.Add(minutesSpentLabel);

            minutesSpentTextBox = new TextBox { Location = new Point(10, minutesSpentLabel.Bottom+10), Size = new Size(20, 20), ReadOnly = true };
            minutesSpentTextBox.Text = minutesUntilBlock.ToString();
            Controls.Add(minutesSpentTextBox);

            Label taskLabel = new Label { Text = "What task you worked on?", Location = new Point(10, minutesSpentTextBox.Bottom+10), Size = new Size(200, 13) };
            Controls.Add(taskLabel);

            taskTextBox = new TextBox { Location = new Point(10, taskLabel.Bottom + 10), Size = new Size(460, 100), Multiline = true };
            Controls.Add(taskTextBox);

            Label categoryLabel = new Label { Text = "Category", Location = new Point(10, taskTextBox.Bottom + 10), Size = new Size(200, 13) };
            Controls.Add(categoryLabel);

            categoryListBox = new ListBox { Location = new Point(10, categoryLabel.Bottom + 10), Size = new Size(460, 100) };
            categoryListBox.Items.AddRange(new string[] { "Work", "Personal", "Trade", "Others" });
            Controls.Add(categoryListBox);

            Label focusLevelLabel = new Label { Text = "Focus level scale of (1 to 5)", Location = new Point(10, categoryListBox.Bottom + 10), Size = new Size(200, 13) };
            Controls.Add(focusLevelLabel);

            focusLevelRadioButtons = new RadioButton[5];
            for (int i = 0; i < 5; i++)
            {
                focusLevelRadioButtons[i] = new RadioButton
                {
                    Text = (i + 1).ToString(),
                    Location = new Point(10, focusLevelLabel.Bottom + 10 + i * 30), // Adjust the spacing between the radio buttons
                    Size = new Size(35, 17)
                };
                Controls.Add(focusLevelRadioButtons[i]);
            }           

            submitButton = new Button { Text = "Submit", Location = new Point(100, focusLevelRadioButtons[4].Bottom + 10), Size = new Size(75, 23) };
            submitButton.Click += SubmitButton_Click;
            Controls.Add(submitButton);

           

        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=tasks.db;Version=3;"))
            {
                conn.Open();

                string sql = "CREATE TABLE IF NOT EXISTS UnlockScreenResponses (ID INTEGER PRIMARY KEY AUTOINCREMENT, Task TEXT, Category TEXT, FocusLevel INTEGER, MinutesSpent INTEGER, Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP)";

                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    command.ExecuteNonQuery();
                }

                sql = "INSERT INTO UnlockScreenResponses (Task, Category, FocusLevel, MinutesSpent) VALUES (@Task, @Category, @FocusLevel, @MinutesSpent)";

                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@Task", taskTextBox.Text);
                    string category = categoryListBox.SelectedItem != null ? categoryListBox.SelectedItem.ToString() : "Other";
                    command.Parameters.AddWithValue("@Category", category);
                    command.Parameters.AddWithValue("@FocusLevel", focusLevelRadioButtons.ToList().FindIndex(rb => rb.Checked) + 1);
                    command.Parameters.AddWithValue("@MinutesSpent", minutesSpentTextBox.Text);

                    command.ExecuteNonQuery();
                }
            }

            // Close the form
            DialogResult = DialogResult.OK;
        }
    }
}