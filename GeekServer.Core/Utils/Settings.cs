using System;
using System.IO;
using Newtonsoft.Json;

public enum ServerType
{
    None = 0,
    ///<summary>登陆服</summary>
    Login,
    ///<summary>游戏服</summary>
    Game,
    ///<summary>中心服</summary>
    Center,
    ///<summary>充值服</summary>
    Recharge,
    ///<summary>聊天服</summary>
    Chat,
}

public class Settings
{
    /// <summary>是否正常运行中(除开起服/关服准备)</summary>
    public volatile bool AppRunning;
    /// <summary>起服时间</summary>
    public DateTime StartServerTime { get; set; }
    /// <summary> 开发模式 </summary>
    public bool IsDebug { get; set; }



    /// <summary>数据回存最大时间(秒)</summary>
    public int TimerSaveMax { get; set; }
    /// <summary>数据回存最小时间(秒)</summary>
    public int TimerSaveMin { get; set; }
    /// <summary>组件自动回收时间(分钟)</summary>
    public int CompRecycleTime { get; set; }
    /// <summary>actor自动回收时间(分钟)</summary>
    public int ActorRecycleTime { get; set; }



    /// <summary> 服务器id 从10000开始</summary>
    public int ServerId { get; set; }

    /// <summary> tcp端口  </summary>
    public int TcpPort { get; set; }
    /// <summary> 使用libuv </summary>
    public bool UseLibuv { get; set; }



    /// <summary> http端口 </summary>
    public int HttpPort { get; set; }

    /// <summary> http内部命名验证 </summary>
    public string HttpInnerCode { get; set; }
    /// <summary> http外部命令验证,可能提供给sdk方 </summary>
    public string HttpCode { get; set; }
    /// <summary> http指令路径 </summary>
    public string HttpUrl { get; set; }


    /// <summary> mongoDB数据库名 </summary>
    public string MongoDB { get; set; }
    /// <summary> mongoDB登陆路径 </summary>
    public string MongoUrl { get; set; }

    /// <summary>语言</summary>
    public string Language { get; set; }

    public ServerType ServerType { get; private set; }
    public static Settings Ins { get; private set; }
    public static void Load(string configFilePath, ServerType serverType)
    {
        Ins = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(configFilePath));
        Ins.ServerType = serverType;
    }
}
