
namespace Geek.Server
{
    public class EventAttribute : EventInfoAttribute
    {
        public EventAttribute(EventID eventId) : base((int)eventId)
        {
        }
    }
}
