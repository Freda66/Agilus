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

    public class LogManager
    {
        private const int MAX_LOG_COUNT = 5000;
        private List<Log> logs = new List<Log>();

        public void AddLog(Log log)
        {
            if (logs.Count > MAX_LOG_COUNT) logs.RemoveAt(0);
            logs.Add(log);
        } 

        public void AddLog(string type, string message)
        {
            AddLog(new Log(type, message));
        }

        public List<string> GetDisplayableLogs(bool orderDescending = true)
        {
            var logsToString = new List<string>();

            if (orderDescending)
            {
                foreach (var log in logs.OrderByDescending(m => m.Time))
                    logsToString.Add(log.ToString());
            }
            else
            {
                foreach (var log in logs)
                    logsToString.Add(log.ToString());
            }

            return logsToString;
        }

    }


}