using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;

public enum ServerType
{
    None = 0,
    ///<summary>游戏服</summary>
    Game,
    ///<summary>中心服</summary>
    Center,
    ///<summary>充值服</summary>
    Recharge,
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
    /// <summary>数据每秒操作上限</summary>
    public int DataFPS { get; set; }

    /// <summary>组件自动回收时间(分钟)</summary>
    public int CompRecycleTime { get; set; }
    /// <summary>actor自动回收时间(分钟)</summary>
    public int ActorRecycleTime { get; set; }

    /// <summary>最大等待登陆人数</summary>
    public int LoginQueueNum { get; set; }


    /// <summary> 服务器id 从10000开始</summary>
    public int ServerId { get; set; }

    /// <summary> 服务器名</summary>
    public string serverName;

    /// <summary> tcp端口  </summary>
    public int TcpPort { get; set; }
    /// <summary> grpc端口  </summary>
    public int GrpcPort { get; set; }
    /// <summary> 使用libuv </summary>
    public bool UseLibuv { get; set; }
    /// <summary> 本机ip </summary>
    public string LocalIp { get; set; }


    /// <summary> http端口 </summary>
    public int httpPort { get; set; }

    /// <summary> http命令验证码</summary>
    public string httpCode { get; set; }
    /// <summary> http指令路径 </summary>
    public string httpUrl { get; set; }
    /// <summary> 后台地址 </summary>
    public string backUrl { get; set; }
    /// <summary> cdk地址 </summary>
    public string cdkUrl { get; set; }


    /// <summary> mongoDB数据库名 </summary>
    public string mongoDB { get; set; }
    /// <summary> mongoDB登陆路径 </summary>
    public string mongoUrl { get; set; }


    /// <summary> 中心服http端口  </summary>
    public int centerHttpPort { get; set; }
    /// <summary> 中心服tcp端口  </summary>
    public int centerTcpPort { get; set; }
    /// <summary> 中心服grpc端口  </summary>
    public int centerGrpcPort { get; set; }

    /// <summary> 充值服端口 </summary>
    public int rechargeHttpPort { get; set; }
    public int sdkType { get; set; }

    /// <summary> 是否允许从文件中恢复 </summary>
    public bool restoreFromFile { get; set; }

    /// <summary> redis地址 </summary>
    public string redisConfig { get; set; }
    /// <summary> 后台活动拉取地址 </summary>
    public string backActivityUrl { get; set; }

    /// <summary> 钉钉监控地址 </summary>
    public string monitorUrl { get; set; }
    /// <summary> 钉钉监控key </summary>
    public string monitorKey { get; set; }

    public string dataCenter { get; set; }

    public int chooseCenterId { get; set; }

    public string mongoGlobalDB { get; set; }

    public string sdkHttpUrl { get; set; }

    public int loginQueueNum{ get; set; }

    /// <summary>语言</summary>
    public string Language { get; set; }

    static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
    public ServerType ServerType { get; private set; }
    public static Settings Ins { get; private set; }
    public static void Load(string configFilePath, ServerType serverType)
    {
        Ins = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(configFilePath));
        Ins.ServerType = serverType;

#if DEBUG
        if (Ins.IsDebug)
        {
            string localIp = Dns
                .GetHostEntry(Dns.GetHostName())
                .AddressList
                .Where(address => address.AddressFamily == AddressFamily.InterNetwork)
                .FirstOrDefault()?
                .ToString();
            if (!string.IsNullOrEmpty(localIp))
            {
                int oldServerId = Ins.ServerId;
                var arr = localIp.Split('.');

                Ins.ServerId = int.Parse(arr[^1]) + (int.Parse(arr[^2]) % 10) * 1000 + Ins.TcpPort;//ip+端口
                LOGGER.Warn(string.Format("debug mode change serverId>{0}\nip:{1},port:{2},oldServerId:{3}", Ins.ServerId, localIp, Ins.TcpPort, oldServerId));
            }

            Ins.mongoDB += "_" + Ins.ServerId;
        }
#endif
    }
}
