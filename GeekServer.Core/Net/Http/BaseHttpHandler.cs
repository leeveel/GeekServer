using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Geek.Server
{
    public abstract class BaseHttpHandler
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        /// <summary> 是否使用内部验证方式 </summary>
        public abstract bool Inner { get; }
        /// <summary> 是否使用验证sign </summary>
        public virtual bool CheckSign => true;

        public static string GetStringSign(string str, bool inner)
        {
            //取md5
            var data = Encoding.UTF8.GetBytes(str);
            byte[] md5Bytes = new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(data);
            string md5 = BitConverter.ToString(md5Bytes).Replace("-", "").ToLower();

            if (inner)
            {
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
            }
            return md5;
        }

        public string CheckSgin(Dictionary<string, string> paramMap)
        {
            if (!CheckSign || Settings.Ins.IsDebug)
                return "";

            if (Inner)
            {
                //内部验证
                if (!paramMap.ContainsKey("token") || !paramMap.ContainsKey("timeTick"))
                {
                    LOGGER.Error("http命令未包含验证参数");
                    return new HttpResult(HttpResult.Code_Illegal, "http命令未包含验证参数");
                }
                var sign = paramMap["token"];
                var time = paramMap["timeTick"];
                long.TryParse(time, out long timeTick);
                var span = new TimeSpan(Math.Abs(DateTime.Now.Ticks - timeTick));
                if (span.TotalMinutes > 5)//5分钟内有效
                {
                    LOGGER.Error("http命令已过期");
                    return new HttpResult(HttpResult.Code_Illegal, "http命令已过期");
                }

                var str = Settings.Ins.httpInnerCode + time;
                if (sign == GetStringSign(str, true))
                    return "";
                else
                    return new HttpResult(HttpResult.Code_Illegal, "命令验证失败");
            }
            else
            {
                //外部验证
                if (!paramMap.ContainsKey("sign") || paramMap.ContainsKey("code"))
                {
                    LOGGER.Error("http命令未包含验证参数");
                    return new HttpResult(HttpResult.Code_Illegal, "http命令未包含验证参数sign或者code");
                }
                var sign = paramMap["sign"];
                var code = paramMap["code"];

                var str = Settings.Ins.httpCode + code;
                if (sign == GetStringSign(str, false))
                    return "";
                else
                    return new HttpResult(HttpResult.Code_Illegal, "命令验证失败");
            }
        }

        public abstract Task<string> Action(string ip, string url, Dictionary<string, string> paramMap);
    }
}