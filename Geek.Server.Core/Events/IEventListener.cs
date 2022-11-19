
using Geek.Server.Core.Hotfix.Agent;
using Geek.Server.Core.Utils;

namespace Geek.Server.Core.Events
{

    /// <summary>
    /// 每个实例其实都是单例的
    /// </summary>
    public interface IEventListener
    {
        Task HandleEvent(ICompAgent agent, Event evt);

        Type AgentType { get; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class EventInfoAttribute : Attribute
    {
        public int EventId { get; }

        public EventInfoAttribute(int eventId)
        {
            EventId = eventId;
        }
    }

    public struct Event
    {
        public static Event NULL = new Event();
        public int EventId;
        public Param Data;
    }
}
