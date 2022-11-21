using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

//https://blogs.uuu.com.tw/Articles/post/2022/07/06/Blazor-Server-%E9%A9%97%E8%AD%89.aspx
namespace Geek.Server.Center.Web
{
    public class LoginUserInfo
    {
        public string Name { get; set; } = null!;
        public string Role { get; set; } = "admin";
        public DateTime? FirstLoginTime { get; set; }
    }

    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedSessionStorage sessionStorage; //存储在本地浏览器，session关闭后数据会删除
        private readonly ProtectedLocalStorage localStorage; //存储在本地浏览器，session关闭后数据不会删除
        private readonly ClaimsPrincipal anonymousPrincipal = new(new ClaimsIdentity());

        public CustomAuthenticationStateProvider(ProtectedSessionStorage sessionStorage, ProtectedLocalStorage localStorage)
        {
            this.sessionStorage = sessionStorage;
            this.localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                //TODO 判断local存储的值，（sessionStorage当session关闭时，数据会清理）

                var result = await sessionStorage.GetAsync<LoginUserInfo>("LoginUserInfo");
                LoginUserInfo? loginUserInfo = null;

                if (result.Success)
                {
                    loginUserInfo = result.Value;
                }

                if (loginUserInfo is null)
                {
                    return await Task.FromResult(new AuthenticationState(anonymousPrincipal));
                }

                ClaimsIdentity identity = new(new List<Claim> {
                    new Claim(ClaimTypes.Name, loginUserInfo.Name),
                    new Claim(ClaimTypes.Role, loginUserInfo.Role),
              }, "CustomAuthentication");

                ClaimsPrincipal principal = new(identity);
                return await Task.FromResult(new AuthenticationState(principal));
            }
            catch
            {
                return await Task.FromResult(new AuthenticationState(anonymousPrincipal));
            }
        }

        public async Task UpdateState(LoginUserInfo loginUserInfo = null)
        {
            ClaimsPrincipal principal;

            if (loginUserInfo is null)
            {
                await sessionStorage.DeleteAsync("LoginUserInfo");
                principal = anonymousPrincipal;
            }
            else
            {
                //await localStorage.SetAsync("LoginUserInfo", loginUserInfo);
                await sessionStorage.SetAsync("LoginUserInfo", loginUserInfo);
                ClaimsIdentity identity = new(new List<Claim>
                {
                    new Claim(ClaimTypes.Name, loginUserInfo.Name),
                    new Claim(ClaimTypes.Role, loginUserInfo.Role),
                }, "CustomAuthentication");

                principal = new ClaimsPrincipal(identity);
            }

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal!)));
        }
    }
}
