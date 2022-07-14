using System;

namespace Geek.Server.Proto
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class SerializeAttribute : Attribute
    {
        public int Id { get; private set; }
        public SerializeAttribute(int id)
        {
            Id = id;
        }
    }
}
