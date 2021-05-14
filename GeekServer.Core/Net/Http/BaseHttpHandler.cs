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
                //内部校验多一步,（可自己修改验证方式）
                //这里在md5最后再加上所有数字的和
                int sum = 0;
                for (int i = 0; i < md5.Length; ++i)
                {
                    if (md5[i] >= '0' && md5[i] <= '9')
                        sum += md5[i] - '0';
                }
                md5 += sum;
            }
            return md5;
        }

        public string CheckSgin(Dictionary<string, string> paramMap)
        {
            if (!CheckSign || Settings.Ins.IsDebug)
                return "";

            if (!paramMap.ContainsKey("sign") || !paramMap.ContainsKey("time"))
            {
                LOGGER.Error("http命令未包含验证参数");
                return new HttpResult(HttpResult.Code_Illegal, "http命令未包含验证参数");
            }
            var sign = paramMap["sign"];
            var time = paramMap["time"];
            long.TryParse(time, out long timeTick);
            var nowTick = DateTime.UtcNow.Ticks;
            if (timeTick > nowTick)
            {
                var span = new TimeSpan(timeTick - nowTick);
                if(span.TotalSeconds > 10)//提前10秒
                {
                    LOGGER.Error("http命令校验时间错误");
                    return new HttpResult(HttpResult.Code_Illegal, "http命令校验时间错误");
                }
            }else
            {
                var span = new TimeSpan(nowTick - timeTick);
                if (span.TotalSeconds > 30)//延后30秒有效
                {
                    LOGGER.Error("http命令已过期");
                    return new HttpResult(HttpResult.Code_Illegal, "http命令已过期");
                }
            }


            var str = Settings.Ins.HttpCode + time;
            if(Inner)
                str = Settings.Ins.HttpInnerCode + time;

            if (sign == GetStringSign(str, Inner))
                return "";
            else
                return new HttpResult(HttpResult.Code_Illegal, "命令验证失败");
        }

        public abstract Task<string> Action(string ip, string url, Dictionary<string, string> paramMap);
    }
}