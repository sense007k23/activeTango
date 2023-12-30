using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WinFormsActiveTango
{
    public class AnalysisTab
    {
        private DataGridView analysisDataGridView;
        private Chart pieChart;
        private TabPage tabPage;
        private ComboBox dateFilterComboBox;

        public AnalysisTab(TabPage tabPage)
        {
            this.tabPage = tabPage;
            InitializeAnalysisTab();
        }

        private void InitializeAnalysisTab()
        {
            analysisDataGridView = new DataGridView { Location = new Point(10, 40), Size = new Size(500, 500), AutoGenerateColumns = false };
            analysisDataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Date", Width = 100 });
            analysisDataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "Category", Width = 150 });
            analysisDataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "MinutesSpent", HeaderText = "Minutes Spent", Width = 250 });

            tabPage.Controls.Add(analysisDataGridView);

            pieChart = new Chart { Location = new Point(520, 10), Size = new Size(460, 480) };
            pieChart.ChartAreas.Add(new ChartArea());
            pieChart.Series.Add(new Series { ChartType = SeriesChartType.Pie, Label = "#VALX: #VAL", IsValueShownAsLabel = true, LegendText = "#VALX: #PERCENT{P0}" });
            pieChart.Legends.Add(new Legend());

            tabPage.Controls.Add(pieChart);

            dateFilterComboBox = new ComboBox { Location = new Point(10, 10), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            dateFilterComboBox.SelectedIndexChanged += DateFilterComboBox_SelectedIndexChanged;
            tabPage.Controls.Add(dateFilterComboBox);

            LoadData();
        }

        private void LoadData(string dateFilter = null)
        {
            analysisDataGridView.Rows.Clear();
            pieChart.Series[0].Points.Clear();
            dateFilterComboBox.Items.Clear();

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=tasks.db;Version=3;"))
            {
                conn.Open();

                string sql;

                if (!string.IsNullOrEmpty(dateFilter) && dateFilter != "All")
                {
                    sql = "SELECT DATE(Timestamp) as Date, Category, SUM(MinutesSpent) as TotalMinutesSpent FROM UnlockScreenResponses WHERE DATE(Timestamp) = @DateFilter GROUP BY Date, Category";
                }
                else
                {
                    sql = "SELECT DATE(Timestamp) as Date, Category, SUM(MinutesSpent) as TotalMinutesSpent FROM UnlockScreenResponses GROUP BY Date, Category";
                }

                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    if (!string.IsNullOrEmpty(dateFilter) && dateFilter != "All")
                    {
                        command.Parameters.AddWithValue("@DateFilter", dateFilter);
                    }

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            analysisDataGridView.Rows.Add(reader["Date"], reader["Category"], reader["TotalMinutesSpent"]);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(dateFilter) && dateFilter != "All")
                {
                    sql = "SELECT Category, SUM(MinutesSpent) as TotalMinutesSpent FROM UnlockScreenResponses WHERE DATE(Timestamp) = @DateFilter GROUP BY Category";

                    using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                    {
                        command.Parameters.AddWithValue("@DateFilter", dateFilter);

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                pieChart.Series[0].Points.AddXY(reader["Category"], reader["TotalMinutesSpent"]);
                            }
                        }
                    }
                }
                else if (string.IsNullOrEmpty(dateFilter) || dateFilter == "All")
                {
                    sql = "SELECT Category, SUM(MinutesSpent) as TotalMinutesSpent FROM UnlockScreenResponses GROUP BY Category";

                    using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                pieChart.Series[0].Points.AddXY(reader["Category"], reader["TotalMinutesSpent"]);
                            }
                        }
                    }
                }

                sql = "SELECT DISTINCT DATE(Timestamp) as Date FROM UnlockScreenResponses ORDER BY Date";

                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dateFilterComboBox.Items.Add(reader["Date"].ToString());
                        }
                    }
                }

                dateFilterComboBox.SelectedIndexChanged -= DateFilterComboBox_SelectedIndexChanged;
                dateFilterComboBox.Items.Insert(0, "All");
                dateFilterComboBox.SelectedIndex = 0;
                dateFilterComboBox.SelectedIndexChanged += DateFilterComboBox_SelectedIndexChanged;
            }
        }

        private void DateFilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadData(dateFilterComboBox.SelectedItem.ToString());
        }
    }
}