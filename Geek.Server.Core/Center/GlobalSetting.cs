namespace Geek.Server.Core.Center
{
    /// <summary>
    /// 服务器通用配置
    /// </summary>
    public class GlobalSetting
    {
        /// <summary> mongoDB登陆路径 </summary>
        public string MongoUrl { get; set; }
        /// <summary> 钉钉监控地址 </summary>
        public string MonitorUrl { get; set; }
        /// <summary> 钉钉监控key </summary>
        public string MonitorKey { get; set; }
        /// <summary> http内部命名验证 </summary>
        public string HttpInnerCode { get; set; }
        /// <summary> http外部命令验证,可能提供给sdk方 </summary>
        public string HttpCode { get; set; }
        /// <summary>语言</summary>
        public string Language { get; set; }
        /// <summary>本地数据库前缀</summary>
        public string LocalDBPrefix { get; set; }
        /// <summary>本地数据库路径</summary>
        public string LocalDBPath { get; set; }
        /// <summary>SDK类型</summary>
        public int SDKType { get; set; }
    }
}
