using Geek.Server;
using NLog;
using NLog.Fluent;
using Quartz.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Geek.Server.Gateway.MessageHandler
{
    public class MsgHanderFactory
    {
        static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        static Dictionary<int, BaseHander> handerMap = new();
        public static void Init()
        {
            IEnumerable<Type> handlers = from h in Assembly.GetEntryAssembly().GetTypes()
                                         where (h.GetCustomAttributes<MsgMapping>().Any() && h.IsSubclassOf(typeof(BaseHander)))
                                         select h;

            foreach (Type handlerType in handlers)
            {
                var mapatt = handlerType.GetCustomAttribute<MsgMapping>();
                var msgIdField = mapatt.Msg.GetField("MsgID", BindingFlags.Static | BindingFlags.Public);
                if (msgIdField == null) continue;
                int msgId = (int)msgIdField.GetValue(null);
                Log.Info($"添加tcp message handler:{msgId} {handlerType.FullName}");
                handerMap.Add(msgId, Activator.CreateInstance(handlerType) as BaseHander);
            }
        }

        public static bool IsGateMessage(int msgId)
        {
            return handerMap.ContainsKey(msgId);
        }

        public static BaseHander GetHander(int msgId)
        {
            return handerMap[msgId];
        }
    }
}
