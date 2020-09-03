using RemoteImageReceiver.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RemoteImageReceiver.Forms
{
    public partial class LoggerOutput : Form
    {
        List<List<object>> _list = null;

        public LoggerOutput()
        {
            InitializeComponent();
            _list = Logger.GetAllExceptions();
        }

        private void LoggerOutput_Load(object sender, EventArgs e)
        {
            Populate();
        }

        private void Populate()
        {
            listBox1.Items.Clear();
            textBox1.Text = textBox2.Text = String.Empty;
            foreach(List<object> e in _list)
            {
                listBox1.Items.Add(e[1].ToString());
            }
            label1.Text = "Total Exceptions : " + listBox1.Items.Count;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Populate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Logger.ClearAllExceptions();
            Populate();
            MessageBox.Show("Successfully Done!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(listBox1.SelectedIndex < _list.Count)
            {
                textBox1.Text = ((Exception)_list[listBox1.SelectedIndex][0]).Message;
                textBox1.Text = ((Exception)_list[listBox1.SelectedIndex][0]).ToString();
            }
        }
    }
}
