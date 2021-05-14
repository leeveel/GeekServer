using System;

namespace Geek.Server
{
    public class ServerState : DBState
    {
        /// <summary>开服时间</summary>
        public DateTime OpenServerTime { get; set; }
        /// <summary>缓存开服天数</summary>
        public int CacheDaysFromOpenServer { get; set; } = 1;
    }

    public class ServerComp : StateComponent<ServerState>
    {
    }
}
