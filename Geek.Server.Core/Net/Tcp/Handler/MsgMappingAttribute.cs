namespace Geek.Server.Core.Net.Tcp.Handler
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MsgMapping : Attribute
    {
        public Type Msg { get; }

        public MsgMapping(Type msg)
        {
            Msg = msg;
        }
    }
}
