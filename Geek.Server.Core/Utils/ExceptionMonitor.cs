using Geek.Server.Core.Net.Http;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text.Encodings.Web;

namespace Geek.Server.Core.Utils
{
    public enum ExceptionType
    {
        StartFailed = 1,
        UnhandledException,
        ActorTimeout,
    }

    public class ExceptionMonitor
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        private static DateTime nextSendTime = DateTime.Now;

        public static async Task Report(ExceptionType exp, string msg = "")
        {
            if (exp == ExceptionType.ActorTimeout)
            {
                if (DateTime.Now < nextSendTime)
                    return;

                nextSendTime = DateTime.Now.AddSeconds(60);
            }

            try
            {
                LOGGER.Info($"提交监控 exp={exp} msg={msg}");
            }
            catch (Exception) { }
            string content = $"【{Settings.ServerId}+{Settings.ServerName}】 发生异常：" + exp.ToString() + "\n" + msg;
            await Send(content);
        }

        public static async Task Send(string content)
        {
            var monitorUrl = Settings.MonitorUrl;
            var secret = Settings.MonitorKey;

            if (string.IsNullOrEmpty(monitorUrl) || string.IsNullOrEmpty(secret))
            {
                LOGGER.Error($"Dintalk.monitorUrl{monitorUrl} or MonitorKey{secret} 不合法");
                return;
            }

            //var monitorUrl = "https://oapi.dingtalk.com/robot/send?access_token=xxxxxxxxxxxxx"; 
            //xxxxxxxxxxxxx=填写你的access_token
            Console.WriteLine(DateTime.UtcNow);
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var timestamp = Convert.ToInt64(ts.TotalMilliseconds).ToString();
            Console.WriteLine(timestamp);
            // var secret = "SECf5e87c31592a77eb40eff223aa20dee54860d966b234d24a80c08a2a4ee379db";
            var stringToSign = timestamp + "\n" + secret;
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(stringToSign);
            var hmacsha256 = new HMACSHA256(keyByte);
            byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
            var sign = Convert.ToBase64String(hashmessage);
            sign = UrlEncoder.Default.Encode(sign);

            var repoerUrl = $"{monitorUrl}&timestamp={timestamp}&sign={sign}";
            var (code, res) = await HttpTools.PostAsync(repoerUrl, new JObject
            {
                ["at"] = new JObject
                {
                    ["atMobiles"] = new JObject(),
                    ["isAtAll"] = true,
                },
                ["msgtype"] = "text",
                ["text"] = content,
            });

            LOGGER.Info($"code:{code} res:{res}");
        }
    }
}
