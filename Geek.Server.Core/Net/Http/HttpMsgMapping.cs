using System;

namespace Geek.Server
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HttpMsgMapping : Attribute
    {
        public string Cmd;
        public HttpMsgMapping(string cmd)
        {
            this.Cmd = cmd;
        }
    }
}
