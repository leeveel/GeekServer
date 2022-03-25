
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Geek.Server
{
    public class HttpTools
    {
        public static readonly HttpClient HttpClient = new HttpClient();

        static HttpTools(){
            HttpClient.Timeout = TimeSpan.FromMilliseconds(5000L);
        }

        public static async Task<string> GetServerHttpUrl(int serverId)
        {
            //ServerConfig serverConfig = await ConsulUtils.GetServerConfig(serverId);
            ServerConfig serverConfig = await ServerInfoUtils.GetServerConfig(serverId);
            if (serverConfig == null)
                return "";
            var httpUrl = $"http://{serverConfig.Ip}:{serverConfig.HttpPort}{Settings.Ins.httpUrl}";
            return httpUrl;
        }
    }
}
