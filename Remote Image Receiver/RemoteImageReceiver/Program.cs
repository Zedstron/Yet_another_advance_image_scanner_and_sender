using System;
using System.Windows.Forms;

namespace RemoteImageReceiver
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            SplashScreen s = new SplashScreen();
            s.ShowDialog();

            Application.Run(new MainForm());
        }
    }
}
