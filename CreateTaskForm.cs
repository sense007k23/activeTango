using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsActiveTango
{
    public partial class CreateTaskForm : Form
    {
        public Task Task { get; private set; }

        private TextBox taskNameBox;
        private ComboBox priorityBox;
        private DateTimePicker dueDatePicker;
        private ComboBox hourBox;
        private ComboBox minuteBox;
        private ComboBox amPmBox;
        private Button okButton;
        private Button cancelButton;

        public CreateTaskForm()
        {
            InitializeComponent();
            this.Text = "Creat Action Tango Tasks";

            taskNameBox = new TextBox { Location = new Point(10, 10) };
            priorityBox = new ComboBox { Location = new Point(10, 40), DropDownStyle = ComboBoxStyle.DropDownList };
            priorityBox.Items.AddRange(new string[] { "P1", "P2", "P3" });
            dueDatePicker = new DateTimePicker { Location = new Point(10, 70), Format = DateTimePickerFormat.Short };

            hourBox = new ComboBox { Location = new Point(10, 100), Size = new Size(50, 0), DropDownStyle = ComboBoxStyle.DropDownList };
            hourBox.Items.AddRange(new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" });
            minuteBox = new ComboBox { Location = new Point(70, 100), Size = new Size(50, 0), DropDownStyle = ComboBoxStyle.DropDownList };
            minuteBox.Items.AddRange(new string[] { "00", "05", "10", "15", "20", "25", "30", "35", "40", "45", "50", "55" });
            amPmBox = new ComboBox { Location = new Point(130, 100), Size = new Size(50, 0), DropDownStyle = ComboBoxStyle.DropDownList };
            amPmBox.Items.AddRange(new string[] { "AM", "PM" });

            okButton = new Button { Text = "OK", Location = new Point(10, 130) };
            okButton.Click += okButton_Click;
            cancelButton = new Button { Text = "Cancel", Location = new Point(90, 130) };
            cancelButton.Click += cancelButton_Click;

            Controls.Add(taskNameBox);
            Controls.Add(priorityBox);
            Controls.Add(dueDatePicker);
            Controls.Add(hourBox);
            Controls.Add(minuteBox);
            Controls.Add(amPmBox);
            Controls.Add(okButton);
            Controls.Add(cancelButton);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(taskNameBox.Text))
            {
                MessageBox.Show("Task name is required.");
                return;
            }

            if (priorityBox.SelectedIndex == -1)
            {
                MessageBox.Show("Priority is required.");
                return;
            }

            if (hourBox.SelectedIndex == -1 || minuteBox.SelectedIndex == -1 || amPmBox.SelectedIndex == -1)
            {
                MessageBox.Show("Due time is required.");
                return;
            }

            DateTime dueTime = DateTime.Parse($"{hourBox.SelectedItem}:{minuteBox.SelectedItem} {amPmBox.SelectedItem}");
            DateTime dueDateTime = dueDatePicker.Value.Date + dueTime.TimeOfDay;
            Task = new Task { Name = taskNameBox.Text, Priority = priorityBox.SelectedItem.ToString(), DueDate = dueDateTime.ToString("dd-MM-yyyy hh:mm tt"), Status = "Pending" };
            DialogResult = DialogResult.OK;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}