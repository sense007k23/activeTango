﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace WinFormsActiveTango
{
    public partial class BlockScreenForm : Form
    {
        public DateTime BlockTime { get; private set; }
        private System.Windows.Forms.Timer timer;

        public BlockScreenForm(int minutesUntilBlock)
        {
            InitializeComponent();
            ResetBlockTime(minutesUntilBlock);

            timer = new System.Windows.Forms.Timer { Interval = minutesUntilBlock * 60 * 1000 };
            timer.Tick += (sender, e) => BlockSecondDisplay();
            timer.Start();
        }

        public void UpdateTimer(int minutesUntilBlock)
        {
            ResetBlockTime(minutesUntilBlock);
            timer.Interval = minutesUntilBlock * 60 * 1000;
        }

        public void ResetBlockTime(int minutesUntilBlock)
        {
            BlockTime = DateTime.Now.AddMinutes(minutesUntilBlock);
            if (timer !=  null) { 
                timer.Stop();
                timer.Start();
            }
        }

        public void BlockSecondDisplay()
        {
            if (Screen.AllScreens.Length > 1)
            {
                var secondScreen = Screen.AllScreens[1];
                StartPosition = FormStartPosition.Manual;
                Location = secondScreen.WorkingArea.Location;
                Size = new Size(secondScreen.WorkingArea.Width, secondScreen.WorkingArea.Height);
                WindowState = FormWindowState.Maximized;
                FormBorderStyle = FormBorderStyle.None;
                TopMost = true;
                Show();
            }
        }
    }
}
