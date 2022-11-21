using MessagePack;

namespace Geek.Server.Center.Web.Data
{
    [MessagePackObject(true)]
    public class UserInfo
    {
        public string Name { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
