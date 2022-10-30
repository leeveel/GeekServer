using Quartz.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GeekServer.Gateaway.MessageHandler
{
    public class MsgHanderFactory
    {
        static Dictionary<int, BaseHander> handerMap;
        public static void Init()
        {
            IEnumerable<Type> handlers = from h in Assembly.GetEntryAssembly().GetTypes()
                                         where h.GetCustomAttributes<MsgMapping>().Any() && h.IsImplWithInterface(typeof(BaseHander))
                                         select h;

            foreach (Type handlerType in handlers)
            {
                var mapatt = handlerType.GetCustomAttribute<MsgMapping>();
                var msgIdField = mapatt.Msg.GetField("MsgID", BindingFlags.Static | BindingFlags.Public);
                if (msgIdField == null) continue;
                int msgId = (int)msgIdField.GetValue(null);
                handerMap.Add(msgId, Activator.CreateInstance(handlerType) as BaseHander);
            }
        }
    }
}
