using Geek.Server.Core.Actors;

namespace Geek.Server.Core.Comps
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
