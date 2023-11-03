
using Geek.Server.Core.Utils;
using Newtonsoft.Json;
using NLog;
using System.Net;

public enum ServerType
{
    None = 0,
    ///<summary>游戏服</summary>
    Game,
    ///<summary>服务发现服</summary>
    Discovery,
    ///<summary>充值服</summary>
    Recharge,
    ///<summary>网关服</summary>
    Gate,
    ///<summary>远程备份</summary>
    Backup
}

public static class Settings
{
    public static BaseSetting Ins { get; private set; }

    public static void Load<T>(string path, ServerType serverType) where T : BaseSetting
    {
        var configJson = File.ReadAllText(path);
        Ins = JsonConvert.DeserializeObject<T>(configJson);
        Ins.ServerType = serverType;
        if (Ins.ServerId < IdGenerator.MIN_SERVER_ID || Ins.ServerId > IdGenerator.MAX_SERVER_ID)
        {
            throw new Exception($"ServerId不合法{Ins.ServerId},需要在[{IdGenerator.MIN_SERVER_ID},{IdGenerator.MAX_SERVER_ID}]范围之内");
        }
    }

    public static T InsAs<T>() where T : BaseSetting
    {
        return (T)Ins;
    }
}

public class BaseSetting
{
    static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
    public virtual bool IsLocal(int serverId)
    {
        return serverId == ServerId;
    }

    public DateTime LauchTime { get; set; }

    CancellationTokenSource AppExitSource = new();
    public CancellationToken AppExitToken => AppExitSource.Token;

    bool _appRunning;
    public bool AppRunning
    {
        get => _appRunning;
        set
        {
            lock (AppExitSource)
            {
                if (AppExitSource.IsCancellationRequested)
                {
                    if (value)
                    {
                        LOGGER.Error("AppRunning已经被设置为退出，不能再次开启...");
                    }
                    _appRunning = false;
                    return;
                }
                _appRunning = value;
                if (!value && !AppExitSource.IsCancellationRequested)
                {
                    LOGGER.Info("Set AppRunning false...");
                    AppExitSource.Cancel();
                }
            }
        }
    }

    public ServerType ServerType { get; set; }
    public bool IsDebug { get; init; }
    public int ServerId { get; init; }
    public string ServerName { get; init; }
    public string LocalIp { get; init; }
    public string HttpCode { get; set; }
    public string HttpInnerCode { get; set; }
    public string HttpUrl { get; set; }
    public int HttpPort { get; init; }
    public int InnerPort { get; init; }
    public int OuterPort { get; init; }
    public int RpcPort { get; set; }
    public string MongoUrl { get; set; }
    public string MongoDBName { get; init; }
    public string LocalDBPrefix { get; set; }
    public string LocalDBPath { get; set; }
    public string Language { get; set; }
    public string CenterUrl { get; init; }
    public float SyncStateToCenterInterval { get; init; }
    public int SDKType { get; set; }
    /// <summary> 钉钉监控地址 </summary>
    public string MonitorUrl { get; set; }
    /// <summary> 钉钉监控key </summary>
    public string MonitorKey { get; set; }
    public int DBModel { get; set; }
    public string DiscoveryServerUrl { get; set; }
}
