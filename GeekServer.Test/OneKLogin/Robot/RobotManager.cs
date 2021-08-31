using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geek.Server.Test
{
    public class RobotManager
    {
        private static readonly RobotManager singleton = new RobotManager();
        public static RobotManager Singleton
        {
            get { return singleton; }
        }

        public static void Start()
        {
            string host = RobotSetting.Ins.ipAdd;
            int port = RobotSetting.Ins.tcpPort;
            var handlerList = new List<Type>
            {
                typeof(TcpClientEncoder),
                typeof(TcpClientDecoder),
                typeof(RobotTcpHandler)
            };
            RobotClient.Init(host, port, handlerList);
            InnerStart();
        }

        private async static void InnerStart()
        {
            int maxCount = RobotSetting.Ins.maxOnline;
            for (int i = 0; i < maxCount; i++)
            {
                await ActorMgr.GetOrNew(CreateRoleId(i));
                await Task.Delay(5);
            }
        }

        /// <summary>
        /// 避免每次都生成新的id
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private static long CreateRoleId(int index)
        {
            long actorType = (long)ActorType.Role;
            long res = (long)RobotSetting.Ins.localId << 49;//(63-14) serverId 前14位[最大16383]
            res |= actorType << 42; //(63-14-7) actorType[最大127]
            return res | (long)index;
        }

    }
}
