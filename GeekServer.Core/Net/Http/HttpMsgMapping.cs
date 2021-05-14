using System;

namespace Geek.Server
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HttpMsgMapping : Attribute
    {
        public string cmd;
        public HttpMsgMapping(string cmd)
        {
            this.cmd = cmd;
        }
    }
}
