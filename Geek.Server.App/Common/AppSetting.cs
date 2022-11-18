namespace Geek.Server.App.Common
{
    public class AppSetting : BaseSetting
    {
        public readonly List<int> Servers = new();

        public bool AllowOpen { get; set; } = true;

        public override bool IsLocal(int serverId)
        {
            return base.IsLocal(serverId) || Servers.Contains(serverId);
        }
    }
}
