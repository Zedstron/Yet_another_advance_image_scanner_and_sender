using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteImageReceiver.Core
{
    class Logger
    {
        private static List<List<object>> _list = new List<List<object>>();
        private static int MAX_COUNT = 10;

        public static void ClearAllExceptions()
        {
            _list.Clear();
        }

        public static void Log(Exception e)
        {
            if (_list.Count > MAX_COUNT)
                _list.RemoveAt(0);

            List<object> temp = new List<object>();
            temp.Add(e);
            temp.Add(DateTime.UtcNow.ToLongTimeString());
            _list.Add(temp);
        }

        public static List<List<object>> GetAllExceptions()
        {
            return _list;
        }
    }
}
