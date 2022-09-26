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

public static class Settings
{
    private static BaseSetting Ins;

    public static void Load<T>(string path, ServerType serverType) where T : BaseSetting
    {
        var configJson = File.ReadAllText(path);
        Ins = JsonConvert.DeserializeObject<T>(configJson);
        Ins.ServerType = serverType;
    }

    public static T InsAs<T>() where T : BaseSetting
    {
        return (T)Ins;
    }

    public static bool IsLocal(int serverId) => Ins.IsLocal(serverId);

    public static DateTime LauchTime
    {
        get => Ins.LauchTime;
        set => Ins.LauchTime = value;
    }

    public static bool AppRunning
    {
        get => Ins.AppRunning;
        set => Ins.AppRunning = value;
    }

    public static ServerType ServerType => Ins.ServerType;

    public static bool IsDebug => Ins.IsDebug;

    public static int ServerId => Ins.ServerId;

    public static string ServerName => Ins.ServerName;

    public static string LocalIp => Ins.LocalIp;

    public static string HttpCode => Ins.HttpCode;

    public static string HttpUrl => Ins.HttpUrl;

    public static int HttpPort => Ins.HttpPort;

    public static int TcpPort => Ins.TcpPort;

    public static int GrpcPort => Ins.GrpcPort;

    public static string MongoUrl => Ins.MongoUrl;

    public static string DbName => Ins.DbName;

    public static string Language => Ins.Language;

    public static string DataCenter => Ins.DataCenter;

    public static string CenterUrl => Ins.CenterUrl;

    public static int SDKType => Ins.SDKType;
}

public class BaseSetting
{
    public virtual bool IsLocal(int serverId)
    {
        return serverId == ServerId;
    }

    public DateTime LauchTime { get; set; }

    public volatile bool AppRunning = false;

    public ServerType ServerType { get; set; }

    #region from config
    public bool IsDebug { get; init; }

    public int ServerId { get; init; }

    public string ServerName { get; init; }

    public string LocalIp { get; init; }

    public string HttpCode { get; init; }

    public string HttpUrl { get; init; }

    public int HttpPort { get; init; }

    public int TcpPort { get; init; }

    public int GrpcPort { get; init; }

    public string MongoUrl { get; init; }

    public string DbName { get; init; }

    public string Language { get; init; }

    public string DataCenter { get; init; }

    public string CenterUrl { get; init; }

    public int SDKType { get; set; }
    #endregion
}