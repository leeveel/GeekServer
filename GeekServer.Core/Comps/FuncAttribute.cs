namespace Geek.Server
{

    [AttributeUsage(AttributeTargets.Class)]
    public class FuncAttribute : Attribute
    {
        public short func;

        public FuncAttribute(short func)
        {
            this.func = func;
        }
    }
}