namespace Geek.Server.Core.Center
{
    /// <summary>
    /// 服务器通用配置
    /// </summary>
    public class GlobalSetting
    {
        /// <summary>数据回存最大时间(秒)</summary>
        public int TimerSaveMax { get; set; }
        /// <summary>数据回存最小时间(秒)</summary>
        public int TimerSaveMin { get; set; }
        /// <summary> mongoDB数据库名 </summary>
        public string MongoDB { get; set; }
        /// <summary> mongoDB登陆路径 </summary>
        public string MongoUrl { get; set; }
        /// <summary> 钉钉监控地址 </summary>
        public string MonitorUrl { get; set; }
        /// <summary> 钉钉监控key </summary>
        public string MonitorKey { get; set; }

        /// <summary> redis地址 </summary>
        public string RedisUrl { get; set; }
        /// <summary> http内部命名验证 </summary>
        public string HttpInnerCode { get; set; }
        /// <summary> http外部命令验证,可能提供给sdk方 </summary>
        public string HttpCode { get; set; }
        /// <summary> http指令路径 </summary>
        public string HttpUrl { get; set; }
        /// <summary>语言</summary>
        public string Language { get; set; }
    }
}
