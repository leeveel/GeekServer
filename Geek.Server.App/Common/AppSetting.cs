using Geek.Server.Core.Utils;

namespace Geek.Server.App.Common
{
    public class AppSetting : BaseSetting
    {
        public readonly List<int> Servers = new();
        public override bool IsLocal(int serverId)
        {
            return base.IsLocal(serverId) || Servers.Contains(serverId);
        }
    }
}
