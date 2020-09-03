using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RemoteImageReceiver
{
    public partial class SplashScreen : Form
    {
        private Timer _timer = null;

        public SplashScreen()
        {
            InitializeComponent();  
        }

        private void SplashScreen_Load(object sender, EventArgs e)
        {
            Focus();
            _timer = new Timer();
            _timer.Interval = 3500;
            _timer.Tick += Time_Ellapsed;
            _timer.Start();
        }

        private void Time_Ellapsed(object sender, EventArgs e)
        {
            _timer.Stop();
            Close();
        }
    }
}
