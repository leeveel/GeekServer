using Geek.Server.Core.Center;

namespace Geek.Server.Core.Center
{
    public class SubscribeEvent
    {
        public const string ConfigChange = "ConfigChange";
        public const string NetNodeStateChangeSuffix = "StateChange";
        public static string NetNodeStateChange(ServerType type)
        {
            return $"{type}{NetNodeStateChangeSuffix}";
        }
    }
}
