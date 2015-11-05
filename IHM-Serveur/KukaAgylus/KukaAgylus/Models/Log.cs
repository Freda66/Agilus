using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KukaAgylus.Models
{
    public class Log
    {
        public string Message { get; private set; }

        public string LogType { get; private set; }

        public DateTime Time { get; private set; }

        public Log(string logType, string message)
        {
            this.Message = message;
            this.LogType = logType;
            this.Time = DateTime.Now;
        }

        public override string ToString()
        {
            return string.Format("[{0}] {1}: {2}", LogType.ToUpper(), Time.ToLongTimeString(), Message);
        }

    }


}