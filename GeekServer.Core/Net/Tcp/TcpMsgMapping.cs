using System;

namespace Geek.Server
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MsgMapping : Attribute
    {
        public MsgMapping(Type msgType)
        {
            Msg = msgType;
        }

        public Type Msg { get; set; }
    }
}