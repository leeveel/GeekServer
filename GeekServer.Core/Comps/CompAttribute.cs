namespace Geek.Server
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CompAttribute : Attribute
    {
        public CompAttribute(ActorType type)
        {
            ActorType = type;
        }

        public ActorType ActorType { get; }

    }
}
