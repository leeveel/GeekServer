using System;
using System.Collections.Generic;
using System.Reflection;

namespace Geek.Server
{
    public class HttpHandlerFactory
    {
        static readonly Dictionary<string, Type> cmdHandlerMap = new Dictionary<string, Type>();
        static Func<string, BaseHttpHandler> handlerGetter;
        static Func<Dictionary<string, string>, BaseHttpHandler> noCmdHandlerGetter;
        public static void SetExtraHandlerGetter(Func<string, BaseHttpHandler> func, Func<Dictionary<string, string>, BaseHttpHandler> noCmdFunc = null)
        {
            handlerGetter = func;
            noCmdHandlerGetter = noCmdFunc;
        }

        public static void InitHandler(Type autoHandlerAssemblyType)
        {
            if (autoHandlerAssemblyType != null)
                InitHandler(autoHandlerAssemblyType.Assembly);
        }

        public static void InitHandler(Assembly assembly)
        {
            cmdHandlerMap.Clear();
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                var attribute = (HttpMsgMapping)type.GetCustomAttribute(typeof(HttpMsgMapping), true);
                if (attribute == null)
                    continue;
                var cmd = attribute.cmd;
                cmdHandlerMap[cmd] = type;
            }
        }

        public static BaseHttpHandler GetHandler(string cmd, Dictionary<string, string> paramMap)
        {
            BaseHttpHandler handler = null;
            if (string.IsNullOrEmpty(cmd))
            {
                if (noCmdHandlerGetter != null)
                {
                    handler = noCmdHandlerGetter(paramMap);
                    if (handler != null)
                        return handler;
                }
                return null;
            }

            if (handlerGetter != null)
                handler = handlerGetter(cmd);
            if (handler != null)
                return handler;

            if (cmdHandlerMap.ContainsKey(cmd))
                handler = Activator.CreateInstance(cmdHandlerMap[cmd]) as BaseHttpHandler;
            return handler;
        }
    }
}
