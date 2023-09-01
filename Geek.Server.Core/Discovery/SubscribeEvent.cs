
namespace Core.Discovery
{
    public class SubscribeEvent
    {
        public const string NetNodeStateChangeSuffix = "StateChange";
        public static string NetNodeStateChange(ServerType type)
        {
            return $"{type}{NetNodeStateChangeSuffix}";
        }
    }
}
