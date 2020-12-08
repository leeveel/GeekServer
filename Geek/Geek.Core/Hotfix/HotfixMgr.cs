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
using Geek.Core.Net.Handler;
using Geek.Core.Net.Http;
using Geek.Core.Net.Message;
using System.Threading.Tasks;

namespace Geek.Core.Hotfix
{
    public class HotfixMgr
    {
        static HotfixModule module = null;
        public static DateTime ReloadTime { get; private set; }
        public static async Task<bool> ReloadModule(string dllVersion)
        {
            var newModule = new HotfixModule();
            await newModule.Load(dllVersion, module != null);
            if (!newModule.IsSucceed)
                return false;

            bool isReload = module != null;
            if (module != null)
                module.Unload();
            module = newModule;
            ReloadTime = DateTime.Now;
            await module.HotfixBridge.OnLoadSucceed(isReload);
            return true;
        }

        public static Task Stop()
        {
            return module.HotfixBridge.Stop();
        }

        public static IMessage GetTcpMsg(int msgId)
        {
            return module.GetTcpMsg(msgId);
        }

        public static BaseTcpHandler GetTcpHandler(int msgId)
        {
            return module.GetTcpHandler(msgId);
        }

        public static BaseHttpHandler GetHttpHandler(string cmd)
        {
            return module.GetHttpHandler(cmd);
        }

        /// <summary>
        /// 对应agent是不是对应接口
        /// </summary>
        public static bool IsAgentInterface(Type refType, Type interfaceType)
        {
            return module.IsAgentInterface(refType, interfaceType);
        }

        /// <summary>
        /// 获取/更新热更代理实例
        /// </summary>
        public static T GetAgent<T>(object refOwner) where T : IAgent
        {
            return module.GetAgent<T>(refOwner);
        }

        /// <summary>
        /// 获取实例
        /// 主要用于获取Event,Timer, Schedule,的Handler实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static T GetInstance<T>(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return default;
            return module.GetInstance<T>(typeName);
        }

        public static T CreateInstance<T>(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return default;
            return module.CreateInstance<T>(typeName);
        }

        /// <summary>
        /// 判断类型是不是来自Hotfix
        /// </summary>
        public static bool IsTypeFromHotfix(Type type)
        {
            return module.IsTypeFromHotfix(type);
        }

        /// <summary>
        /// 判断对象是不是来自Hotfix
        /// </summary>
        public static bool IsFromHotfix(object obj)
        {
            if (obj == null)
                return false;
            return IsTypeFromHotfix(obj.GetType());
        }
    }
}