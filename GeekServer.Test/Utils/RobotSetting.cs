using Newtonsoft.Json;
using System.IO;

namespace Geek.Server.Test
{
    public class RobotSetting
    {
        public static readonly bool XBuffer_Netty = true;

        /// <summary> serverId </summary>
        public int serverId;
        /// <summary> tcp端口 </summary>
        public int tcpPort;
        /// <summary> ip地址 </summary>
        public string ipAdd;
        /// <summary> 最大在线人数 </summary>
        public int maxOnline;
        /// <summary> 本地id(用于生成Robot唯一id) </summary>
        public int localId;

        public static RobotSetting Ins { get; private set; }
        public static void Load(string configFilePath)
        {
            Ins = JsonConvert.DeserializeObject<RobotSetting>(File.ReadAllText(configFilePath));
        }

    }
}
