namespace Geek.Server.Core.Net.BaseHandler
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
