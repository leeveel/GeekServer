using System.Security.Cryptography;
using System.Text;
using Geek.Server.Core.Utils;
using NLog;

namespace Geek.Server.Core.Net.Http
{
    public abstract class BaseHttpHandler
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        /// <summary> 是否使用内部验证方式 </summary>
        public virtual bool CheckSign => true;

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
            md5 = checkCode1 + md5 + checkCode2;

            return md5;
        }

        public string CheckSgin(Dictionary<string, string> paramMap)
        {
            if (!CheckSign || Settings.IsDebug)
                return "";

            //内部验证
            if (!paramMap.ContainsKey("token") || !paramMap.ContainsKey("timestamp"))
            {
                LOGGER.Error("http命令未包含验证参数");
                return new HttpResult(HttpResult.Stauts.Illegal, "http命令未包含验证参数");
            }
            var sign = paramMap["token"];
            var time = paramMap["timestamp"];
            long.TryParse(time, out long timeTick);
            var span = new TimeSpan(Math.Abs(DateTime.Now.Ticks - timeTick));
            if (span.TotalMinutes > 5)//5分钟内有效
            {
                LOGGER.Error("http命令已过期");
                return new HttpResult(HttpResult.Stauts.Illegal, "http命令已过期");
            }

            var str = Settings.HttpCode + time;
            if (sign == GetStringSign(str))
                return "";
            else
                return new HttpResult(HttpResult.Stauts.Illegal, "命令验证失败");
        }

        public abstract Task<string> Action(string ip, string url, Dictionary<string, string> paramMap);
    }
}