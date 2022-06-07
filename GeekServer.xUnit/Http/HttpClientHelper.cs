using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace Geek.Server.xUnit
{
    public class HttpClientHelper
    {
        private static string GetStringSign(string str, bool inner)
        {
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
            md5 = checkCode1 + md5 + checkCode2;
            return md5;
        }

        public static string GetToken(long timestamp)
        {
            var str = "geek_server_http_code" + timestamp;
            var token = GetStringSign(str, true);
            return token;
        }

        /// <summary>
        /// get请求
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<HttpResult> GetRequst(string url)
        {
            if (url.StartsWith("https"))
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = httpClient.GetAsync(url).Result;

            HttpResult result = null;
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<HttpResult>(json);
            }
            return new HttpResult() { Code = "Failed", Msg = "调用失败" };
        }

        public static async Task<T> GetRequst<T>(string url) where T : HttpResult, new()
        {
            if (url.StartsWith("https"))
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = httpClient.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(json);
            }
            return new T() { Code = "Failed", Msg = "调用失败" };
        }


        /// <summary>
        /// post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData">post数据</param>
        /// <returns></returns>
        public static async Task<HttpResult> PostRequest(string url, string postData)
        {
            if (url.StartsWith("https"))
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            HttpContent httpContent = new StringContent(postData);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpClient httpClient = new HttpClient();
            //httpClient.Timeout = TimeSpan.FromSeconds(30);

            try
            {
                HttpResponseMessage response = await httpClient.PostAsync(url, httpContent);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<HttpResult>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return new HttpResult() { Code = "Failed", Msg = "调用失败" };
        }

        public static async Task<T> PostRequest<T>(string url, string postData) where T : HttpResult, new()
        {
            if (url.StartsWith("https"))
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            HttpContent httpContent = new StringContent(postData);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpClient httpClient = new HttpClient();
            //httpClient.Timeout = TimeSpan.FromSeconds(30);

            try
            {
                HttpResponseMessage response = await httpClient.PostAsync(url, httpContent);
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(res);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return new T() { Code = "Failed", Msg = "调用失败" };
        }

    }

    public class HttpResult
    {
        public string Code { get; set; }
        public string Msg { get; set; }
        public Dictionary<string, string> ExtraMap { get; set; }
    }

    public class HttpTestRes : HttpResult
    {
        public class Info
        {
            public int Age { get; set; }
            public string Name { get; set; }
        }

        public int A { get; set; }
        public string B { get; set; }

        public Info TestInfo { get; set; }
    }

}
