
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Geek.Server
{
    public enum ExceptionType
    {
        StartFailed = 1,
        UnhandledException,
        ActorTimeout,
    }

    public class ExceptionMonitor
    {
        static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        private static DateTime nextSendTime = DateTime.Now;
        public static async Task Report(ExceptionType exp, string msg = "")
        {
            if (Settings.Ins.IsDebug)
                return;

            if(exp == ExceptionType.ActorTimeout)
            {
                if(DateTime.Now < nextSendTime)
                    return;

                nextSendTime = DateTime.Now.AddSeconds(60);
            }

            LOGGER.Info($"提交监控 exp={exp} msg={msg}");
            string content = $"【{Settings.Ins.ServerId}+{Settings.Ins.serverName}】 发生异常：" + exp.ToString() + "\n" + msg;
            await Send(content);
        }


        /// <summary>
        /// 通知钉钉
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static async Task Send(string content)
        {
            var monitorUrl = Settings.Ins.monitorUrl;
            var secret = Settings.Ins.monitorKey;

            if(!string.IsNullOrEmpty(monitorUrl) || !string.IsNullOrEmpty(secret))
                return;

            var json = new
            {
                at = new
                {
                    atMobiles = new { },
                    isAtAll = true
                },
                msgtype = "text",
                text = new
                {
                    content
                }
            };

            HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(json));
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            //Console.WriteLine(DateTime.UtcNow);
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var timestamp = Convert.ToInt64(ts.TotalMilliseconds).ToString();
            //Console.WriteLine(timestamp);
            var stringToSign = timestamp + "\n" + secret;
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(stringToSign);
            var hmacsha256 = new HMACSHA256(keyByte);
            byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
            var sign = Convert.ToBase64String(hashmessage);
            sign = UrlEncoder.Default.Encode(sign);

            var repoerUrl = $"{monitorUrl}&timestamp={timestamp}&sign={sign}";
            var res = await HttpTools.HttpClient.PostAsync(repoerUrl, httpContent);

            var str = await res.Content.ReadAsStringAsync();
            LOGGER.Info(str);
        }
    }
}
