using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Geek.Server.Udp
{
    public class LogEvent
    {

        public static byte SEPARATOR = (byte)':';
        private readonly EndPoint source;
        private readonly string logfile;
        private readonly string msg;
        private readonly long received;

        public LogEvent(string logfile, string msg) : this(null, -1, logfile, msg)
        {
        }

        public LogEvent(EndPoint source, long received, string logfile, string msg)
        {
            this.source = source;
            this.received = received;
            this.logfile = logfile;
            this.msg = msg;
        }

        public EndPoint GetSource()
        {
            return source;
        }

        public string GetLogfile()
        {
            return logfile;
        }

        public string GetMsg()
        {
            return msg;
        }

        public long GetReceived()
        {
            return received;
        }
    }
}