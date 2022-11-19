using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace Geek.Server.Core.Net.Http
{
    public static class HttpTools
    {

        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 单个服务HttpClient应当共享一个实例，该实例是多线程安全的
        /// </summary>
        public static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(10) };

        /// <summary>
        /// 根据服务器id获取服务器http地址
        /// </summary>
        //public static async Task<string> GetServerHttpUrl(NetNode node)
        //{
        //    if (node == null)
        //        return "";
        //    return $"http://{node.Ip}:{node.HttpPort}{node.HttpUrl}";
        //}

        public static async Task<(HttpStatusCode statusCode, string content)> PostAsync(string url, JObject json = null, bool inner = false)
        {
            if (json == null)
                json = new JObject();

            if (inner)
            {
                var time = DateTime.Now.Ticks;
                json[TOKEN_KEY] = GetStringSign(Settings.HttpCode + time);
                json[TIME_KEY] = time;
            }

            try
            {
                var stringContent = new StringContent(json.ToString());
                stringContent.Headers.ContentType = JSON_CONTENT_TYPE;
                var response = await Client.PostAsync(url, stringContent);
                var content = await response.Content.ReadAsStringAsync();
                return (response.StatusCode, content);
            }
            catch (Exception e)
            {
                Log.Error($"http post exception, url:{url} param:{json} e:{e}");
                throw;
            }
        }

        public static async Task<(HttpStatusCode statusCode, string content)> GetAsync(string url, JObject json = null, bool inner = false)
        {
            if (!url.Contains('?'))
            {
                url += '?';
            }
            if (json != null && json.HasValues)
            {
                bool flag = !url.Contains('&');
                foreach (var item in json)
                {
                    var k = item.Key;
                    var v = item.Value;
                    if (flag)
                    {
                        flag = false;
                        url += $"{k}={v}";
                    }
                    else
                    {
                        url += $"&{k}={v}";
                    }
                }
            }
            if (inner)
            {
                var time = DateTime.Now.Ticks;
                string sign = GetStringSign(Settings.HttpCode + time);
                url += $"&{TOKEN_KEY}={sign}&{TIME_KEY}={time}";
            }
            try
            {
                var response = await Client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                return (response.StatusCode, content);
            }
            catch (Exception e)
            {
                Log.Error($"http get exception, url:{url} e:{e}");
                throw;
            }
        }

        public static string ParamStr(Dictionary<string, string> paramDic)
        {
            return paramDic == null ? "" : string.Join('&', paramDic.Select(kv => $"{kv.Key}={kv.Value}"));
        }

        private static readonly MediaTypeHeaderValue JSON_CONTENT_TYPE = new("application/json");

        public const string TOKEN_KEY = "token";
        public const string TIME_KEY = "timeTick";

        public static string GetStringSign(string str)
        {
            //取md5
            var data = Encoding.UTF8.GetBytes(str);
            byte[] md5Bytes = MD5.Create().ComputeHash(data);
            string md5 = BitConverter.ToString(md5Bytes).Replace("-", "").ToLower();

            int checkCode1 = 0;//校验码
            int checkCode2 = 0;
            for (int i = 0; i < md5.Length; ++i)
            {
                if (md5[i] >= 'a')
                    checkCode1 += md5[i];
                else
                    checkCode2 += md5[i];
            }
            md5 += checkCode1 + md5 + checkCode2;

            return md5;
        }
    }
}
