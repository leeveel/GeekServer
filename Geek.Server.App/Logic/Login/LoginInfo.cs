namespace Geek.Server.App.Logic.Login
{
    public class LoginInfo
    {
        public bool isReconnect;//是否是重连

        public long roleId;
        public int serverId;
        public string playerId;
        public string newName;
        public int uniId;

        public int sdkType;
        public string sdkChannel;
        public string packageName;
    }
}