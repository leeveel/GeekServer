using ICSharpCode.SharpZipLib.Zip;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geek.Server
{
    public static class RedisMgr
    {
        private static ConnectionMultiplexer redis;

        public static void Init()
        {
            string configString = Settings.Ins.redisConfig;
            var options = ConfigurationOptions.Parse(configString);
            options.AllowAdmin = true;// 允许使用range | keys *等危险操作
            options.CheckCertificateRevocation = false;
            redis = ConnectionMultiplexer.Connect(options);
            RedisKeyTool.Init();
        }

        /// <summary>
        /// db no need to store
        /// </summary>
        public static IDatabase DB { get { return redis.GetDatabase(); } }
        public static IConnectionMultiplexer Connection { get { return redis; } }
    }

    public static class RedisMgrExt
    {
        public static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public static async Task DeleteAllKey(this IConnectionMultiplexer connection, string keyStr)
        {
            LOGGER.Warn($"Delete All Redis Key [{keyStr}]");
            var db = connection.GetDatabase();
            LOGGER.Debug($"删除Key {keyStr}");
            keyStr += "*";
            var endPoints = connection.GetEndPoints();
            LOGGER.Debug($"获得EndPoint {endPoints.Length}");
            foreach (var ep in endPoints)
            {
                LOGGER.Debug($"节点 [{ep.Serialize()}] 找到以下Key");
                var server = connection.GetServer(ep);
                var keys = server.Keys(pattern: keyStr);
                foreach (var key in keys)
                {
                    LOGGER.Debug(key.ToString());
                    //db.KeyDelete(key);
                    await db.KeyDeleteAsync(key);
                }
            }
        }

        public static List<RedisKey> GetAllKey(this IConnectionMultiplexer connection, string keyStr)
        {
            var list = new List<RedisKey>();
            LOGGER.Warn($"Get All Redis Key [{keyStr}]");
            keyStr += "*";
            var endPoints = connection.GetEndPoints();
            LOGGER.Debug($"获得EndPoint {endPoints.Length}");
            foreach (var ep in endPoints)
            {
                LOGGER.Debug($"节点 [{ep.Serialize()}] 找到以下Key");
                var server = connection.GetServer(ep);
                var keys = server.Keys(pattern: keyStr);
                foreach (var key in keys)
                {
                    LOGGER.Debug(key.ToString());
                }
                list.AddRange(keys);
            }
            return list;
        }

    }

    public class KeyUndefineException : Exception
    {
        public KeyUndefineException(Type type) : base("使用了未定义的Key类型 > " + type.FullName) { }
    }

    public static class RedisKeyTool
    {
        private static Dictionary<string, string> keyValueDic = new Dictionary<string, string>();
        private static Dictionary<string, string> typeDic = new Dictionary<string, string>();
        private static ConcurrentDictionary<Type, string> cacheKeys = new ConcurrentDictionary<Type, string>();

        private static readonly Type rootType = typeof(RedisKeys);
        private static readonly string DATA_CENTER = Settings.Ins.dataCenter;

        public static void Init()
        {
            typeDic.Clear();
            cacheKeys.Clear();

            keyValueDic.Add(rootType.Name, DATA_CENTER);
            typeDic.Add(rootType.Name, DATA_CENTER);
            var allTypes = rootType.Assembly.GetTypes();
            foreach (var type in allTypes)
            {
                if (type.FullName.Contains(rootType.FullName))
                {
                    var names = type.FullName.Split('+');
                    if (names.Length <= 1) continue;
                    typeDic[names[^1]] = names[^2];
                    bool hasValue = false;
                    foreach (var att in System.Attribute.GetCustomAttributes(type))
                    {
                        if (att is Value val)
                        {
                            keyValueDic[names[^1]] = val.value;
                            hasValue = true;
                            break;
                        }
                        else
                            continue;
                    }
                    if (!hasValue)
                        keyValueDic[names[^1]] = names[^1];
                }

            }
        }

        /// <summary>
        /// 获得RedisKey对应的值
        /// </summary>
        /// <typeparam name="T">在RedisKeys中定义的类型<br/>提供未定义的类型将抛出异常</typeparam>
        /// <param name="paramList">key的后续可变参数，按给定的顺序进行拼接</param>
        /// <exception cref="KeyUndefineException"> 提供的泛型T未定义在RedisKeys中 </exception>
        /// <returns>泛型T对应的RedisKey的string值</returns>
        public static string GetKey<T>(params object[] paramList)
        {
            var type = typeof(T);

            var typeName = type.Name;

            if (!typeDic.ContainsKey(typeName))
                throw new KeyUndefineException(type);

            var endFlag = false;
            var paramKey = "";

            for (int i = paramList.Length - 1; i >= 0; i--)
            {
                if (endFlag)
                {
                    paramKey = paramList[i] + ":" + paramKey;
                }
                else
                {
                    paramKey = paramList[i].ToString();
                    endFlag = true;
                }
            }

            endFlag = false;

            if (!cacheKeys.ContainsKey(type))
            {
                var key = "";
                while (typeName != rootType.Name)
                {
                    if (!typeDic.ContainsKey(typeName)) break;

                    var keyValue = keyValueDic[typeName];

                    if (endFlag)
                    {
                        key = keyValue + ":" + key;
                    }
                    else
                    {
                        key = keyValue;
                        endFlag = true;
                    }
                    typeName = typeDic[typeName];
                }

                if (endFlag)
                {
                    key = keyValueDic[rootType.Name] + ":" + key;
                }
                else
                {
                    key = keyValueDic[rootType.Name];
                    endFlag = true;
                }
                cacheKeys[type] = key;
            }

            if (paramList.Length <= 0)
                return cacheKeys[type];
            return cacheKeys[type] + ":" + paramKey;
        }



    }

    /// <summary>
    /// 该特性用于兼容旧版本的redis key, 可以实现key类名与值不同;<br/>
    /// 不使用该特性时, key的值为类名, <br/>
    /// 使用该特性时, key的值为value
    /// </summary>
    class Value : System.Attribute
    {
        public string value;

        public Value(string value)
        {
            this.value = value;
        }
    }

    /// <summary>
    /// 在此处进行redis key的定义<br/>
    /// 通过定义内部类的方式，实现key的分类管理<br/>
    /// 调用时, 通过 <c>RedisKeyTool.GetKey&lt;类名&gt;()</c> 获取key的string值<br/>
    /// 可以通过 <c>[Value(值)]</c> 特性设置key的真实值<br/>
    /// 若不使用特性, 则key的值为类名<br/>
    /// RedisKeys对应的值为 <c>Settings.Ins.dataCenter</c>
    /// </summary>
    public class RedisKeys
    {
        [Value("GUILD")]
        public class Guild
        {
            [Value("NAME")]
            public class Name { }

            [Value("LEVEL")]
            public class Level { }
        }


        [Value("ROLE")]
        public class Role
        {
            [Value("SNAPSHOT")]
            public class Snapshot { }

            [Value("GM_PLAYER")]
            public class GMPlayer { }

            [Value("FIRST_RANK")]
            public class FirstRank { }
        }

        [Value("RANK")]
        public class Rank { }

        [Value("RANK_SURFIX")]
        public class RankSurfix { }


        [Value("SERVER")]
        public class Server
        {
            [Value("DAY")]
            public class Day { }
        }

        [Value("CENTER")]
        public class Center
        {
            [Value("RANKED_MATCHES")]
            public class RankedMatches
            {
                [Value("ZONE")]
                public class Zone { }
                [Value("RANK")]
                public class RankedMatchesRank { }
                [Value("POWER")]
                public class Power { }
            }
        }

        [Value("SERVER_INFO")]
        public class ServerInfo
        {
            [Value("CENTER_SERVER")]
            public class CenterServer{ }
            [Value("LOGIC_SERVER")]
            public class LogicServer { }
            [Value("SERVER_HEART")]
            public class ServerHeart { }
        }
    }

    [Obsolete("该类已弃用, 使用 RedisKeyTool.GetKey<T>() 获取 RedisKey; 使用时, 需先在 RedisKeys 中定义对应的 Key")]
    public static class RedisKeyDef
    {
        //private static readonly string DATA_CENTER = Settings.Ins.dataCenter;

        #region server
        public static string ServerDayKey()
        {
            return RedisKeyTool.GetKey<RedisKeys.Server.Day>();
        }
        #endregion

        #region role snapshot
        /// <summary>
        /// 玩家快照，string json
        /// </summary>
        //private static readonly string ROLE_SNAPSHOT = $"{DATA_CENTER}:ROLE:SNAPSHOT";

        public static string RoleSnapshotKey(long roleId)
        {
            return RedisKeyTool.GetKey<RedisKeys.Role.Snapshot>(roleId);//$"{ROLE_SNAPSHOT}:{roleId}";
        }

        #endregion

        #region rank
        //public static readonly string RANK = $"{DATA_CENTER}:RANK";
        //public static readonly string RANK_SURFIX = $"{DATA_CENTER}:RANK_SURFIX";
        public static readonly string RANK = RedisKeyTool.GetKey<RedisKeys.Rank>();
        public static readonly string RANK_SURFIX = RedisKeyTool.GetKey<RedisKeys.RankSurfix>();
        #endregion
    }



}
