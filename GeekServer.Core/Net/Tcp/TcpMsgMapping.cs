using System;

namespace Geek.Server
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TcpMsgMapping : Attribute
    {
        public TcpMsgMapping(Type msgType)
        {
            Msg = msgType;
        }

        public Type Msg { get; set; }
    }
}