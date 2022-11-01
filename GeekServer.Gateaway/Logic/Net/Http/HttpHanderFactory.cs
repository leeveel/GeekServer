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

namespace Geek.Server.Gateaway.MessageHandler
{
    public class HttpHanderFactory
    {
        static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        static Dictionary<string, BaseHttpHandler> handerMap = new();
        public static void Init()
        {
            IEnumerable<Type> handlers = from h in Assembly.GetEntryAssembly().GetTypes()
                                         where (h.GetCustomAttributes<HttpMsgMapping>().Any() && h.IsSubclassOf(typeof(BaseHttpHandler)))
                                         select h;

            foreach (Type handlerType in handlers)
            {
                var mapatt = handlerType.GetCustomAttribute<HttpMsgMapping>();
                handerMap.Add(mapatt.Cmd, Activator.CreateInstance(handlerType) as BaseHttpHandler);
            }

            HttpHandler.SetHandlerGetter(GetHander);
        }

        public static BaseHttpHandler GetHander(string msgId)
        {
            handerMap.TryGetValue(msgId, out var handler);
            return handler;
        }
    }
}
