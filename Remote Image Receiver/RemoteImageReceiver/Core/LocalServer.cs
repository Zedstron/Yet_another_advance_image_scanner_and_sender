using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace RemoteImageReceiver.Core
{
    class LocalServer
    {
        private TcpListener server = null;
        private Thread acceptThread = null;

        private Func<string, bool> setStatus;
        private Func<int, int, bool> setCount;
        private Func<Image, string, bool> SavePreviewImage;
        private Func<string[], bool> setItem;

        public bool IsRunning { get; private set; }
        public int SuccessReceiveCount { get; private set; }
        public int FailedReceiveCount { get; private set; }
        public List<TcpClient> ClientList { get; private set;  }

        public LocalServer()
        {
            ClientList = new List<TcpClient>();
            IsRunning = false;
            SuccessReceiveCount = 0;
            FailedReceiveCount = 0;
        }

        public void SetDelegates(Func<string, bool> setStatus, Func<int, int, bool> setCount, Func<Image, string, bool> SavePreviewImage, Func<string[], bool> setItem)
        {
            this.setStatus = setStatus;
            this.setCount = setCount;
            this.SavePreviewImage = SavePreviewImage;
            this.setItem = setItem;
        }

        public bool Abort()
        {
            try
            {
                if (IsRunning)
                {
                    IsRunning = false;
                    server.Stop();
                    server = null;
                    return true;
                }
                else
                    return false;
            }
            catch(Exception e)
            {
                Logger.Log(e);
                return false;
            }
        }

        private void StartReading(TcpClient Client)
        {
            while (IsRunning)
            {
                using (NetworkStream stream = Client.GetStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    while (IsRunning)
                    {
                        try
                        {
                            SuccessReceiveCount++;
                            object[] data = (object[])formatter.Deserialize(stream);
                            string name = (string)data[0];
                            Image img = (Image)data[1];
                            setItem(new string[] { SuccessReceiveCount.ToString(), name.Split('.')[0], name.Split('.')[1] });
                            SavePreviewImage(img, name);
                            setCount(SuccessReceiveCount, FailedReceiveCount);
                        }
                        catch(Exception e)
                        {
                            Logger.Log(e);
                            FailedReceiveCount++;
                            SuccessReceiveCount--;
                            setCount(SuccessReceiveCount, FailedReceiveCount);
                        }
                    }
                }
            }
        }

        private void AcceptClient()
        {
            try
            {
                TcpClient tempClient = server.AcceptTcpClient();

                ClientList.Add(tempClient);
                setStatus(tempClient.Client.RemoteEndPoint.ToString());
                StartReading(tempClient);

                if (tempClient.Connected)
                    tempClient.Close();
            }
            catch(Exception e)
            {
                Logger.Log(e);
            }
        }

        public bool Connect()
        {
            try
            {
                if (!IsRunning)
                {
                    server = new TcpListener(IPAddress.Any, 80);
                    server.Start();

                    acceptThread = new Thread(new ThreadStart(AcceptClient));
                    acceptThread.Start();
                    IsRunning = true;
                    return true;
                }
                else return false;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
        }
    }
}
