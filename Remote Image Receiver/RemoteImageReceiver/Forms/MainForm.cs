using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using RemoteImageReceiver.Core;
using RemoteImageReceiver.Forms;

namespace RemoteImageReceiver
{     
    public partial class MainForm : Form
    {
        LocalServer _server;
        private delegate void SetStatusColor(Color color);
        private delegate bool SetPreviewImage(Image img);
        private delegate bool SetStatusCount(int success, int error);
        private delegate bool SetDataAmount(string value);
        private delegate bool SetListItem(string[] items);

        private string imageSavePath = null;
        private long total_data = 0;

        public MainForm()
        {
            InitializeComponent();
            imageSavePath = Environment.CurrentDirectory + "\\Images\\";
            _server = new LocalServer();
            _server.SetDelegates(SetStatus, setCount, SavePreviewImage, setItem);
        }

        

        private void setColor(Color color)
        {
            if (this.statusStrip1.InvokeRequired)
            {
                SetStatusColor d = new SetStatusColor(setColor);
                this.Invoke(d, new object[] { color });
            }
            else
            {
                toolStripStatusLabel1.ForeColor = color;
            }
        }

        private bool setImage(Image img)
        {
            if (this.pictureBox1.InvokeRequired)
            {
                SetPreviewImage d = new SetPreviewImage(setImage);
                this.Invoke(d, new object[] { img });
                return true;
            }
            else
            {
                pictureBox1.Image = img;
                return true;
            }
        }

        private bool setItem(string[] items)
        {
            if (this.listView1.InvokeRequired)
            {
                SetListItem d = new SetListItem(setItem);
                this.Invoke(d, new object[] { items });
                return true;
            }
            else
            {
                listView1.Items.Add(new ListViewItem(items));
                return true;
            }
        }

        private bool setCount(int success, int error)
        {
            if (this.statusStrip1.InvokeRequired)
            {
                SetStatusCount d = new SetStatusCount(setCount);
                this.Invoke(d, new object[] { success, error });
                return true;
            }
            else
            {
                toolStripStatusLabel5.Text = "Files Success/Failed : " + success + "/" + error;
                return true;
            }
        }

        private bool setDataAmount(string value)
        {
            if (this.statusStrip1.InvokeRequired)
            {
                SetDataAmount d = new SetDataAmount(setDataAmount);
                this.Invoke(d, new object[] { value });
                return true;
            }
            else
            {
                toolStripStatusLabel7.Text = "Data Volume : " + value;
                return true;
            }
        }

        private bool SetStatus(string ip)
        {
            try
            {
                toolStripStatusLabel3.Text = "Host : " + ip;
                toolStripStatusLabel1.Text = "Status : Connected!";
                setColor(Color.Green);
                return true;
            }
            catch(Exception e)
            {
                Logger.Log(e);
                return false;
            }
        }

        private bool SavePreviewImage(Image img, string name)
        {
            try
            {
                if (checkBox3.Checked)
                {
                    img.Save(imageSavePath + name);
                    name = imageSavePath + name;
                }
                else
                {
                    img.Save(imageSavePath + "Image_" + _server.SuccessReceiveCount + name.Split('.')[1]);
                    name = imageSavePath + "Image_" + _server.SuccessReceiveCount + name.Split('.')[1];
                }

                if (checkBox1.Checked)
                    setImage(img);

                total_data += new FileInfo(name).Length;
                setDataAmount(Utilities.FormatSize(total_data));
                return true;
            }
            catch(Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
        }



        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (TcpClient tempClient in _server.ClientList)
            {
                if (tempClient.Connected) 
                {
                    tempClient.Close();
                }
            }

            Environment.Exit(0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(!_server.IsRunning)
            {
                if (_server.Connect())
                {
                    button1.Text = "Stop Server";
                    button1.ForeColor = Color.Red;
                    button1.Image = Image.FromFile(Environment.CurrentDirectory + "\\assets\\stop.png");
                }
                else
                    MessageBox.Show("We cannot start server right now, it seems there is a technical error check the error logs", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (_server.Abort())
                {
                    button1.Text = "Start Server";
                    button1.ForeColor = Color.Green;
                    button1.Image = Image.FromFile(Environment.CurrentDirectory + "\\assets\\start.png");
                }
                else
                    MessageBox.Show("Server abort error, it may be possible that it is already stoped or you can check the logs if some thing really went wrong!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                imageSavePath = dialog.SelectedPath;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            button1.ForeColor = Color.Green;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            button2.Enabled = !checkBox2.Checked;
            if (checkBox2.Checked)
            {
                imageSavePath = Environment.CurrentDirectory + "\\Images\\";
            }
        }

        private void showLastErrorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Logger.GetAllExceptions().Count > 0)
            {
                DialogResult dr = MessageBox.Show(((Exception)Logger.GetAllExceptions()[Logger.GetAllExceptions().Count - 1][0]).Message + Environment.NewLine + "Do You want to view error Detail?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if(dr.Equals(DialogResult.Yes))
                    MessageBox.Show(((Exception)Logger.GetAllExceptions()[Logger.GetAllExceptions().Count - 1][0]).ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
                MessageBox.Show("Hurrah! We got no errors till now!", "Good News", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void showAllErrorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoggerOutput _logger = new LoggerOutput();
            _logger.Show();
        }
    }
}
