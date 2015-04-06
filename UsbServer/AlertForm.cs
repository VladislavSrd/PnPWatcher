using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UsbServer
{
    public partial class AlertForm : Form
    {
        public AlertForm()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.BackColor == Color.DeepPink)
            BackColor = Color.DarkRed;
            else
            BackColor = Color.DeepPink;

        }

        private void AlertForm_FormClosing(object sender, FormClosingEventArgs e)
        {           
            e.Cancel = true;
            this.Visible = false;
            Hide();
        }
    }
}
