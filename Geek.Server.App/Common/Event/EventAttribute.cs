
using Geek.Server.Core.Events;

namespace Geek.Server.App.Common.Event
{
    public class EventAttribute : EventInfoAttribute
    {
        public EventAttribute(EventID eventId) : base((int)eventId)
        {
        }
    }
}
