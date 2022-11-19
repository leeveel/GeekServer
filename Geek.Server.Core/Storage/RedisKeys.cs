namespace Geek.Server.Core.Storage
{
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

            public class Level { }
        }

        [Value("CENTER")]
        public class Center
        {
            [Value("RANKED_MATCHES")]
            public class RankedMatches
            {
                [Value("STATUS")]
                public class Status { }
                [Value("SEASON")]
                public class Season { }
                [Value("ZONE")]
                public class Zone { }
                [Value("RANK")]
                public class Rank { }
                [Value("RANK_SURFIX")]
                public class RankSurfix { }
                [Value("POWER")]
                public class Power { }
                public class Pool { }
                public class Promote { }
                public class NextPromote { }
                public class RoleCold { }

            }

            public class TopFinal
            {
                public class Version { }

                public class Status { }

                public class NextStatus { }

                public class NextTime { }

                public class LastStatus { }

                /// <summary>
                /// (int serverId)
                /// hash
                /// </summary>
                public class Server
                {

                    public const string Group = "Group";

                    public const string Camp = "Camp";
                }

                public class LastWeek
                {
                    public class Server { }
                    public class Rank { }
                    public class Replay
                    {
                        public class Role { }
                        public class Camp
                        {
                            public class Final
                            {
                                public class TeamNum { }
                            }
                        }
                    }
                }

                /// <summary>
                /// (int groupId)
                /// set
                /// </summary>
                public class Group
                {
                    public class MainServer { }

                    public class Rank { }

                    public class RankNo { }

                    public class Camp
                    {
                        public const string PetCount = "PetCount";
                        public const string Leader = "Leader";
                        public const string Notice = "Notice";
                        public const string TotalLv = "TotalLv";
                        public const string Members = "Members";
                        public const string IslandServer = "IslandServer";
                    }

                    public class FinalResult { }
                    public class Servers { }
                }

                public class Replay
                {
                    public class Role { }
                    public class Camp
                    {
                        public class Normal
                        {
                            public class TeamNum { }
                        }
                        public class Final
                        {
                            public class TeamNum { }
                        }
                    }

                    public class LastTime { }
                }

            }

            public class Guild
            {
                public class Contend
                {
                    public class Status { }
                    public class NextStatus { }
                    public class EndTime { }
                    public class BossId { }
                    public class RoleRank { }
                    public class RoleScore { }
                    public class GuildRank { }
                    public class GuildScore { }
                    public class RankSurfix { }
                    public class JoinNum { }
                    public class CrossOpened { }
                    public class Sqrt { }
                }
            }

            public class Island
            {

                public class Common
                {
                    public const string Status = "Status";
                    public const string Version = "Version";
                    public const string Time = "Time";
                }
            }

            public class Raft
            {
                public class Status { }

                public class Day { }

                /// <summary>
                /// hash key:serverId value:mainServer
                /// </summary>
                public class MainServer { }

                /// <summary>
                /// hash key:mainServer value:mainServer+server1+server2+...
                /// </summary>
                public class Group { }

                /// <summary>
                /// hash key:serverId value:roleNum+averagePower
                /// </summary>
                public class Server { }
            }

            public class Boss
            {
                /// <summary>
                /// hash key:serverId value:mainServer
                /// </summary>
                public class MainServer { }

                /// <summary>
                /// hash key:mainServer value:mainServer+server1+server2+...
                /// </summary>
                public class Group { }
            }
        }

        public class Activity
        {
            public class DrawRank
            {

            }

            public class DrawRankNew
            {
                public class Group { }

                public class MainSever { }

                public class CurRank { }

                public class PreRank { }
            }

            public class Anniversary
            {
                public class Group { }
            }
        }

    }
}
