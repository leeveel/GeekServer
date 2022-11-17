namespace Geek.Server
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
