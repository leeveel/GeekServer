


using MongoDB.Driver;
using Newtonsoft.Json;
using NLog;
using Quartz;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Geek.Server.RedisKeys;

namespace Geek.Server
{
    public class ServerInfoUtils
    {
        #region lifeCycle
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        private static int heart = 10000;
        private static int outTime = 60000;

        //从redis重载配置时间（用于冷却）
        //private static long reloadTicks;

        public static void Init()
        {
            LOGGER.Info($"ServerInfoUtils init");
            Reload().Wait();
            SyncConfig().Wait();
/*            Registration().Wait();*/

/*            QuartTimer.Schedule<ReloadJob>(DateTimeOffset.Now.AddSeconds(Settings.Ins.reloadSeconds), TimeSpan.FromSeconds(Settings.Ins.reloadSeconds));*/
            //定时回存心跳时间
            QuartzTimer.Schedule<HeartJob>(DateTimeOffset.Now, TimeSpan.FromMilliseconds(heart));
        }

/*        private class ReloadJob : IJob
        {
            public async Task Execute(IJobExecutionContext context)
            {
                LOGGER.Debug("ServerInfoUtils reload...");
                try
                {
                    await Reload();
                }catch(Exception e)
                {
                    LOGGER.Debug("从redis加载服务器信息失败 e:" + e.Message);
                }
            }
        }*/

        private class HeartJob : IJob
        {
            public async Task Execute(IJobExecutionContext context)
            {
                //LOGGER.Debug("ServerInfoUtils heart...");
                try
                {
                    var key = RedisKeyTool.GetKey<RedisKeys.ServerInfo.ServerHeart>(Settings.Ins.ServerId);
                    _ = await RedisMgr.DB.StringSetAsync(key, DateTimeOffset.Now.ToUnixTimeMilliseconds());
                }
                catch(Exception e)
                {
                    LOGGER.Debug("发送心跳数据失败 e:" + e.Message);
                }
            }
        }

        public static void Stop()
        {
            LOGGER.Info($"ServerInfoUtils stoping");
            LOGGER.Info($"ServerInfoUtils stoped");
        }
        #endregion
        /*
                public class CommonConfig
                {
                    public string CdkUrl = "";
                    public string RedisConfig = "";
                    // 登录历史服地址
                    public string HistoryUrl = "";
                    // 后台活动拉取地址
                    public string BackActivityUrl = "";
                    // 开服活动缓存
                    public string OpenActivity = "";
                    // 钉钉监控地址
                    public string MonitorUrl = "";
                    // 钉钉监控key
                    public string MonitorKey = "";
                    // 重载间隔
                    public int ReloadSeconds = 300;
                }*/

        private static readonly ConcurrentDictionary<ServerType, ServerConfig> ServerDic = new ConcurrentDictionary<ServerType, ServerConfig>();
        private static readonly ConcurrentDictionary<int, ServerConfig> LogicDic = new ConcurrentDictionary<int, ServerConfig>();

        /// <summary>
        /// 取消定时重载 获取配置时重载
        /// </summary>
        /// <returns></returns>
        public static async Task Reload()
        {
/*            DateTime now = DateTime.Now;
            if (!force && (now - new DateTime(reloadTicks)).TotalSeconds < Settings.Ins.reloadSeconds)
            {
                return;
            }
            reloadTicks = now.Ticks;*/
            // 加载服务器配置
            var key = RedisKeyTool.GetKey<RedisKeys.ServerInfo.CenterServer>();
            if (await RedisMgr.DB.KeyExistsAsync(key))
            {
                var hash = await RedisMgr.DB.HashGetAllAsync(key);
                foreach (var kv in hash)
                {
                    ServerConfig serverConfig = JsonConvert.DeserializeObject<ServerConfig>(Encoding.UTF8.GetString(kv.Value));
                    LocalCache(serverConfig);
                }
            }
            key = RedisKeyTool.GetKey<RedisKeys.ServerInfo.LogicServer>();
            if (await RedisMgr.DB.KeyExistsAsync(key))
            {
                var hash = await RedisMgr.DB.HashGetAllAsync(key);
                foreach (var kv in hash)
                {
                    ServerConfig serverConfig = JsonConvert.DeserializeObject<ServerConfig>(Encoding.UTF8.GetString(kv.Value));
                    LocalCache(serverConfig);
                }
            }

        }
        private static void LocalCache(ServerConfig serverConfig)
        {
            var item = (ServerType)serverConfig.ServerType;
            if (item == ServerType.Game)
                LogicDic.AddOrUpdate(serverConfig.ServerId, serverConfig, (k, old) => serverConfig);
            else if (item == ServerType.Center)
            {
                if(Settings.Ins.chooseCenterId <= 0 || serverConfig.ServerId == Settings.Ins.chooseCenterId)
                {
                    ServerDic.AddOrUpdate(item, serverConfig, (k, old) => serverConfig);
                }
            }
            else
                ServerDic.AddOrUpdate(item, serverConfig, (k, old) => serverConfig);
        }

        private static async Task<T> GetConfig<T>(string key, int serverId)
        {
            return await GetLifeActor(key).SendAsync(async () =>
            {
                try
                {
                    if (await RedisMgr.DB.KeyExistsAsync(key) && await RedisMgr.DB.HashExistsAsync(key, serverId))
                    {
                        var value = await RedisMgr.DB.HashGetAsync(key, serverId);
                        var config = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(value));
                        if (config is ServerConfig sc)
                        {
                            if ((ServerType)sc.ServerType == ServerType.Game)
                                LogicDic.TryAdd(sc.ServerId, sc);
                            else
                                ServerDic.TryAdd((ServerType)sc.ServerType, sc);
                        }
                        return (T)config;
                    }
                    else
                    {
                        LOGGER.Error($"从redis获取配置失败。redis地址：{Settings.Ins.redisConfig.Split(',')[0]} key：{key} serverId：{serverId} 不存在");
                    }
                }
                catch (Exception e)
                {
                    LOGGER.Error($"从redis获取配置异常。redis地址：{Settings.Ins.redisConfig.Split(',')[0]} key：{key} serverId：{serverId} 异常：{e.Message}");
                }
                return default;
            });
        }

        /// <summary>
        /// 获取同一空间下的所有逻辑服
        /// </summary>
        public static async Task<ICollection<ServerConfig>> GetLogicConfigs()
        {
            await Reload();
            return LogicDic.Values;
        }

        /// <summary>
        /// 获取中心服id
        /// </summary>
        public static async Task<int> GetCenterServerIdAsync()
        {
            return await GetServerIdAsync(ServerType.Center);
        }

        /// <summary>
        /// 获取通用服务配置，不要传逻辑服
        /// </summary>
        public static async Task<ServerConfig> GetConfigAsync(ServerType type)
        {
            if (ServerDic.TryGetValue(type, out var config))
                return config;
            else
                await Reload();
            if (ServerDic.TryGetValue(type, out config))
                return config;
            throw new Exception($"服务器配置信息找不到 serverType:{type}");
        }

        /// <summary>
        /// 获取通用服务id
        /// </summary>
        public static async Task<int> GetServerIdAsync(ServerType type)
        {
            var config = await GetConfigAsync(type);
            return config == null ? default : config.ServerId;
        }

        public static async Task<ServerConfig> GetLocalServerConfigAsync()
        {
            return await GetServerConfig(Settings.Ins.ServerId);
        }

        /// <summary>
        /// 获取服务配置，包括通用服务和逻辑服
        /// </summary>
        public static async Task<ServerConfig> GetServerConfig(int serverId)
        {
            // 逻辑服
            if (LogicDic.TryGetValue(serverId, out var config))
                return config;

            // 功能服
            config = ServerDic.Values.Where(k => k.ServerId == serverId).FirstOrDefault();
            if (config != null) return config;

            // 一般不会走这一步
            var res = await GetConfig<ServerConfig>(RedisKeyTool.GetKey<ServerInfo.CenterServer>(), serverId);
            if(res != default)
            {
                return res;
            }
            return await GetConfig<ServerConfig>(RedisKeyTool.GetKey<ServerInfo.LogicServer>(), serverId);
        }

        /// <summary>
        /// 上传本服配置
        /// </summary>
        /// <returns></returns>
        private static async Task SyncConfig()
        {
            int localServerId = Settings.Ins.ServerId;
            ServerType serverType = Settings.Ins.ServerType;

            var serverConfig = await GetServerConfig(localServerId);

            if (serverConfig == null) serverConfig = new ServerConfig
            {
                ServerId = localServerId
            };

            serverConfig.ServerName = Settings.Ins.serverName;
            serverConfig.Ip = Settings.Ins.LocalIp;
            serverConfig.HttpPort = Tools.GetHttpPort();
            serverConfig.GrpcPort = Tools.GetGrpcPort();
            serverConfig.ServerType = (int)serverType;
            serverConfig.DbName = Tools.GetDbName();

            await UpdateServerConfig(serverConfig);
        }

        public static async Task UpdateServerConfig(ServerConfig serverConfig)
        {
            LocalCache(serverConfig);

            string configStr = JsonConvert.SerializeObject(serverConfig);
            string key = (ServerType)serverConfig.ServerType switch
            {
                ServerType.Center => RedisKeyTool.GetKey<ServerInfo.CenterServer>(),
                ServerType.Game => RedisKeyTool.GetKey<ServerInfo.LogicServer>(),
                _ => RedisKeyTool.GetKey<ServerInfo.CenterServer>()
            };
            LOGGER.Info($"配置同步到redis，key:[{key}] value: [{configStr}]");

            bool success = await RedisMgr.DB.HashSetAsync(key, serverConfig.ServerId, configStr);
            LOGGER.Info($"配置同步结果：{success}");
        }

        public static async Task<bool> IsAlive(int serverId)
        {
            var key = RedisKeyTool.GetKey<RedisKeys.ServerInfo.ServerHeart>(serverId);
            if(await RedisMgr.DB.KeyExistsAsync(key))
            {
                var heartTime = await RedisMgr.DB.StringGetAsync(key);
                return DateTimeOffset.Now.ToUnixTimeMilliseconds() <= (long)heartTime + outTime;
            }
            return false;
        }

        #region lifeActor
        private static readonly ConcurrentDictionary<string, WorkerActor> LifeDic = new ConcurrentDictionary<string, WorkerActor>();

        private static WorkerActor GetLifeActor(string key)
        {
            LifeDic.TryGetValue(key, out var lifeActor);
            lock (LifeDic)
            {
                LifeDic.TryGetValue(key, out lifeActor);
                if (lifeActor == null)
                {
                    lifeActor = new WorkerActor();
                    LifeDic[key] = lifeActor;
                }
            }
            return lifeActor;
        }
        #endregion

        #region Tools
        private static class Tools
        {
            public static int GetHttpPort()
            {
                return Settings.Ins.ServerType switch
                {
                    ServerType.Center => Settings.Ins.centerHttpPort,
                    ServerType.Recharge => Settings.Ins.rechargeHttpPort,
                    _ => Settings.Ins.httpPort
                };
            }

            public static string GetDbName()
            {
                return Settings.Ins.ServerType switch
                {
                    ServerType.Center => Settings.Ins.mongoGlobalDB,
                    _ => Settings.Ins.mongoDB,
                };
            }

            public static int GetGrpcPort()
            {
                return Settings.Ins.ServerType switch
                {
                    ServerType.Center => Settings.Ins.centerGrpcPort,
                    _ => Settings.Ins.GrpcPort
                };
            }
        }
        #endregion
    }

    public class ServerConfig
    {
        public string ServerName;
        public int ServerId;
        public string Ip;
        public int HttpPort;
        public int GrpcPort;
        public int ServerType;
        public string DbName;
        public int RegisterNum;
        public int DeviceCount;
        public DateTime StartTime;
        public int WorldLevel;
        public int ActiveNumLast30Day;
    }
}
