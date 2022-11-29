using Geek.Server.Core.Net.Http;
using System.Reflection;

namespace Geek.Server.Rebalance.Net.Http
{
    public class HttpHanderFactory
    {
        static readonly Logger Log = LogManager.GetCurrentClassLogger();
        static Dictionary<string, BaseHttpHandler> handerMap = new();
        public static void Init()
        {
            IEnumerable<Type> handlers = from h in Assembly.GetEntryAssembly().GetTypes()
                                         where h.GetCustomAttributes<HttpMsgMapping>().Any() && h.IsSubclassOf(typeof(BaseHttpHandler))
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
