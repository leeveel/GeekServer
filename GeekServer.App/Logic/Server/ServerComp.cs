using System;

namespace Geek.Server.Logic.Server
{
    public class ServerState : DBState
    {
        // <summary>开服时间戳</summary>
        public virtual long OpenServerTimeTick { get; set; }
        /// <summary>缓存开服天数</summary>
        public virtual int CacheDaysFromOpenServer { get; set; } = 1;
    }

    public class ServerComp : StateComponent<ServerState>
    {
        public const int CrossDayHour = 0;
    }
}
