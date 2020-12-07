/****************************************************************************
Copyright (c) Geek

https://github.com/leeveel/GeekServer

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
****************************************************************************/
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Geek.Core.Net.Http
{
    public class HttpHandlerFactory
    {
        private static Dictionary<string, Type> cmdHandlerMap = new Dictionary<string, Type>();
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
