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
            RobotClient.Init(host, port);
            InnerStart();
        }

        private async static void InnerStart()
        {
            int maxCount = RobotSetting.Ins.maxOnline;
            for (int i = 0; i < maxCount; i++)
            {
                var role = await EntityMgr.GetCompAgent<RoleCompAgent>(CreateRoleId(i));
                _ = role.Start();
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
            long actorType = (long)EntityType.Role;
            long res = (long)RobotSetting.Ins.localId << 46;//(63-17) 
            res |= actorType << 42; //(63-4-17) 
            return res | (long)index;
        }

    }
}
