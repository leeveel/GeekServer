using Geek.Server.Core.Center;

namespace Geek.Server.Center.Logic
{
    public class SubscribeEvent
    {
        public const string ConfigChange = "ConfigChange";
        public string NetNodeStateChange(NodeType type)
        {
            return $"{type}StateChange";
        }
    }
}
