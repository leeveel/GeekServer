using System;
using System.Threading.Tasks;

namespace Geek.Server
{
    public class ServerCompAgent : StateComponentAgent<ServerComp, ServerState>
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public override async Task Active()
        {
            await base.Active();
            if (State.OpenServerTime < new DateTime(2000, 1, 1))
            {
                State.OpenServerTime = DateTime.Now;
                LOGGER.Warn("新服开启");
            }
        }

        public const int CrossDayHour = 0;
        /// <summary>
        /// 开服天数
        /// </summary>
        public int GetDaysFromOpenServer()
        {
            var open = State.OpenServerTime;
            var now = DateTime.Now;
            if (open.Hour < CrossDayHour) //开始时间小于跨天时间，相当于昨天开服
                open = new DateTime(open.Year, open.Month, open.Day, CrossDayHour, 0, 0).AddDays(-1);
            else //开始时间大于跨天时间
                open = new DateTime(open.Year, open.Month, open.Day, CrossDayHour, 0, 0);
            return (int)Math.Ceiling((now - open).TotalDays);//向上取整
        }
    }
}
