using Geek.Server.Center.Logic;
using Geek.Server.Center.Web.Data;
using NLog;

namespace Geek.Server.Center.Web.Service
{
    public class LoginService
    {
        protected static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        private DBService dbService;
        public LoginService(DBService dbService)
        {
            this.dbService = dbService;
        }

        public async Task<string> Verify(string userNane, string password)
        {
            LOGGER.Debug($"请求验证:{userNane} {password}");
            var user = dbService.GetData<UserInfo>(userNane);
            if (user == null)
            {
                return "不能发现用户";
            }
            if (user.Password != password)
                return "密码错误";
            return null;
        }
    }
}
