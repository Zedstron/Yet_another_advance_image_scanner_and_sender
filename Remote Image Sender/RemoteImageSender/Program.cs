using System;
using System.Collections.Generic;
using System.Linq;

namespace RemoteImageSender
{
    static class Program
    {

        [STAThread]
        static void Main()
        {
            Agent agent = new Agent("Computer_Name_Here_in_LAN");
            agent.Init();
        }
    }
}
