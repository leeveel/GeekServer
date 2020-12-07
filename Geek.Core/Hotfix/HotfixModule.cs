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
using System.IO;
using System.Reflection;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Geek.Core.Net.Handler;
using Geek.Core.Net.Http;
using Geek.Core.Net.Message;

namespace Geek.Core.Hotfix
{
    public class HotfixModule
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public bool IsSucceed { get; private set; }

        DllLoader dllLoader;
        Assembly hotfixAssembly;
        public IHotfixBridge HotfixBridge { get; private set; }

        
        //actor-actorAgent, comp-compAgent
        Dictionary<Type, Type> agentMap = new Dictionary<Type, Type>();
        ConcurrentDictionary<object, IAgent> agentInstanceMap = new ConcurrentDictionary<object, IAgent>();
        ConcurrentDictionary<string, object> cacheMap = new ConcurrentDictionary<string, object>();

        Dictionary<int, Type> tcpMsgMap = new Dictionary<int, Type>();
        Dictionary<int, Type> tcpHandlerMap = new Dictionary<int, Type>();
        Dictionary<string, Type> httpHandlerMap = new Dictionary<string, Type>();

        public async Task Load(string dllVersion, bool isReload)
        {
            try
            {
                bool writeDllVersion = false;
                string currentAssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string dllPath = Path.Combine(currentAssemblyDirectory, "Geek.Hotfix.dll");
                if (!string.IsNullOrEmpty(dllVersion))
                {
                    var path = Path.Combine(currentAssemblyDirectory, dllVersion + "/Geek.Hotfix.dll");
                    if (File.Exists(path))
                    {
                        dllPath = path;
                        writeDllVersion = true;
                    }
                    else
                    {
                        dllVersion = "org";
                    }
                }
                else
                {
                    dllVersion = "org";
                    var txtPath = Path.Combine(currentAssemblyDirectory, "dllVersion.txt");
                    if (File.Exists(txtPath))
                    {
                        var versionStr = File.ReadAllText(txtPath);
                        var path = Path.Combine(currentAssemblyDirectory, versionStr + "/Geek.Hotfix.dll");
                        if (File.Exists(path))
                        {
                            dllPath = path;
                            dllVersion = versionStr;
                        }
                    }
                }

                dllLoader = new DllLoader(dllPath);
                dllLoader.Load();
                hotfixAssembly = dllLoader.HotfixDll;
                loadDll();

                if(!isReload)
                    await HotfixBridge.Start();
                else
                    await HotfixBridge.Reload();

                if (writeDllVersion)
                    File.WriteAllText(Path.Combine(currentAssemblyDirectory, "dllVersion.txt"), dllVersion);
                LOGGER.Info("hotfix dll loaded:" + dllVersion);
                IsSucceed = true;
            }
            catch (Exception e)
            {
                LOGGER.Info("hotfix dll init failed..." + e.ToString());
                IsSucceed = false;
            }
        }

        public void Unload()
        {
            if (dllLoader != null)
            {
                var weak = dllLoader.Unload();
                if(Settings.Ins.isDebug)
                {
                    //检查hotfix dll是否已经释放
                    Task.Run(async () => {
                        while (weak.IsAlive)
                        {
                            await Task.Delay(100);
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }
                        LOGGER.Warn("hotfix dll unloaded");
                    });
                }
            }
        }

        void loadDll()
        {
            var types = hotfixAssembly.GetTypes();
            foreach(var type in types)
            {
                addAgent(type);
                addTcpHandler(type);
                addHttpHandler(type);
                if (HotfixBridge == null && type.GetInterface(typeof(IHotfixBridge).FullName) != null)
                    HotfixBridge = (IHotfixBridge)Activator.CreateInstance(type);
            }
        }

        void addAgent(Type type)
        {
            if (type.GetInterface(typeof(IAgent).FullName) == null)
                return;

            Type impType = type;
            while(impType != null && !impType.IsGenericType)
                impType = impType.BaseType;
            if (impType == null || !impType.IsGenericType)
                return;
            var argTypes = impType.GetGenericArguments();
            if (argTypes == null || argTypes.Length != 1)
                return;
            agentMap[argTypes[0]] = type;
        }

        void addTcpHandler(Type type)
        {
            var attribute = (TcpMsgMapping)type.GetCustomAttribute(typeof(TcpMsgMapping), true);
            if (attribute == null) return;
            var msgIdField = attribute.Msg.GetField("MsgId", BindingFlags.Static | BindingFlags.Public);
            if (msgIdField == null) return;
            int msgId = (int)msgIdField.GetValue(null);
            if (!tcpHandlerMap.ContainsKey(msgId))
            {
                tcpHandlerMap.Add(msgId, type);
                tcpMsgMap.Add(msgId, attribute.Msg);
            }
            else
            {
                LOGGER.Error("重复注册消息tcp handler:[{}] msg:[{}]", msgId, type);
            }
        }

        void addHttpHandler(Type type)
        {
            var attribute = (HttpMsgMapping)type.GetCustomAttribute(typeof(HttpMsgMapping), true);
            if (attribute == null) return;
            var cmd = attribute.cmd;
            if (httpHandlerMap.ContainsKey(cmd))
                LOGGER.Warn($"http cmd handler 已存在：{cmd}，新的handler将覆盖老的handler");
            httpHandlerMap[cmd] = type;
        }

        public BaseTcpHandler GetTcpHandler(int msgId)
        {
            if (!tcpHandlerMap.ContainsKey(msgId))
            {
                LOGGER.Error("未注册的tcp消息ID:{}", msgId);
                return null;
            }

            Type handlerType = tcpHandlerMap[msgId];
            var handler = Activator.CreateInstance(handlerType) as BaseTcpHandler;
            if (handler == null)
                LOGGER.Error("创建tcp handler失败:{} {}", msgId, handlerType.ToString());
            return handler;
        }

        public IMessage GetTcpMsg(int msgId)
        {
            if (!tcpHandlerMap.ContainsKey(msgId))
            {
                LOGGER.Error("未注册的tcp消息ID:{}", msgId);
                return null;
            }

            Type msgType = tcpHandlerMap[msgId];
            var msg = Activator.CreateInstance(msgType) as IMessage;
            if (msg == null)
                LOGGER.Error("创建tcp handler失败:{} {}", msgId, msgType.ToString());
            return msg;
        }

        public BaseHttpHandler GetHttpHandler(string cmd)
        {
            if (!httpHandlerMap.ContainsKey(cmd))
            {
                LOGGER.Error("未注册的http消息:{}", cmd);
                return null;
            }
            Type msgType = httpHandlerMap[cmd];
            var msg = Activator.CreateInstance(msgType) as BaseHttpHandler;
            if (msg == null)
                LOGGER.Error("http handler创建失败:{} {}", cmd, msgType.ToString());
            return msg;
        }

        public bool IsTypeFromHotfix(Type type)
        {
            if (type == null)
                return false;
            if(type.IsGenericType)
            {
                var argTypes = type.GetGenericArguments();
                foreach(var inType in argTypes)
                {
                    if (IsTypeFromHotfix(inType))
                        return true;
                }
            }
            if (type.BaseType != null)
                return IsTypeFromHotfix(type.BaseType);
            return type.Assembly == hotfixAssembly;
        }

        /// <summary>
        /// 对应agent是不是对应接口
        /// </summary>
        public bool IsAgentInterface(Type refType, Type interfaceType)
        {
            if (!agentMap.ContainsKey(refType))
                return false;
            var type = agentMap[refType];
            return type.GetInterface(interfaceType.FullName) != null;
        }

        /// <summary>
        /// 获取热更代理实例
        /// </summary>
        public T GetAgent<T>(object refOwner) where T : IAgent
        {
            if (agentInstanceMap.ContainsKey(refOwner))
                return (T)agentInstanceMap[refOwner];

            if (!agentMap.ContainsKey(refOwner.GetType()))
                return default;

            lock(agentInstanceMap)
            {
                if (agentInstanceMap.ContainsKey(refOwner))
                    return (T)agentInstanceMap[refOwner];

                var agent = (T)Activator.CreateInstance(agentMap[refOwner.GetType()]);
                agentInstanceMap.TryAdd(refOwner, agent);
                agent.Owner = refOwner;
                return agent;
            }
        }

        /// <summary>获取实例(主要用于获取Event,Timer, Schedule,的Handler实例)</summary>
        public T GetInstance<T>(string typeName)
        {
            cacheMap.TryGetValue(typeName, out var cacheObj);
            if (cacheObj != null)
                return (T)cacheObj;

            var obj = CreateInstance<T>(typeName);
            cacheMap.TryAdd(typeName, obj);
            return obj;
        }

        public T CreateInstance<T>(string typeName)
        {
            return (T)hotfixAssembly.CreateInstance(typeName);
        }
    }
}