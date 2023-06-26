using System.Collections.Concurrent;
using System.Reflection;
using Geek.Server.Core.Actors;
using Geek.Server.Core.Comps;
using Geek.Server.Core.Events;
using Geek.Server.Core.Hotfix.Agent;
using Geek.Server.Core.Net.BaseHandler;
using Geek.Server.Core.Net.Http;
using Geek.Server.Core.Net.Tcp.Handler;
using Geek.Server.Core.Utils;
using NLog;

namespace Geek.Server.Core.Hotfix
{
    internal class HotfixModule
    {
        static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private DllLoader DllLoader = null;
        readonly string DllPath;

        internal IHotfixBridge HotfixBridge { get; private set; }

        internal Assembly HotfixAssembly = null;

        /// <summary>
        /// comp -> compAgent
        /// </summary>
        readonly Dictionary<Type, Type> agentCompMap = new();

        readonly Dictionary<Type, Type> compAgentMap = new();

        readonly Dictionary<Type, Type> agentAgentWrapperMap = new();

        /// <summary>
        /// cmd -> handler
        /// </summary>
        readonly Dictionary<string, BaseHttpHandler> httpHandlerMap = new();

        /// <summary>
        /// msgId -> handler
        /// </summary>
        readonly Dictionary<int, Type> tcpHandlerMap = new();

        /// <summary>
        /// actorType -> evtId -> listeners
        /// </summary>
        readonly Dictionary<ActorType, Dictionary<int, List<IEventListener>>> actorEvtListeners = new();

        readonly bool useAgentWrapper = true;

        internal HotfixModule(string dllPath)
        {
            DllPath = dllPath;
        }

        internal HotfixModule()
        {
            HotfixAssembly = Assembly.GetEntryAssembly();

            ParseDll();
        }

        internal bool Init(bool reload)
        {
            bool success = false;
            try
            {
                DllLoader = new DllLoader(DllPath);
                HotfixAssembly = DllLoader.HotfixDll;
                if (!reload)
                {
                    // 启动服务器时加载关联的dll
                    LoadRefAssemblies();
                }

                ParseDll();

                File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "dllPath.txt"), DllPath);

                Log.Info($"hotfix dll init success: {DllPath}");
                success = true;
            }
            catch (Exception e)
            {
                Log.Error($"hotfix dll init failed...\n{e}");
                if (!reload)
                    throw;
            }
            return success;
        }

        public void Unload()
        {
            if (DllLoader != null)
            {
                var weak = DllLoader.Unload();
                if (Settings.IsDebug)
                {
                    //检查hotfix dll是否已经释放
                    Task.Run(async () =>
                    {
                        int tryCount = 0;
                        while (weak.IsAlive && tryCount++ < 10)
                        {
                            await Task.Delay(100);
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }
                        Log.Warn($"hotfix dll unloaded {(weak.IsAlive ? "failed" : "success")}");
                    });
                }
            }
        }

        private void LoadRefAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var nameSet = new HashSet<string>(assemblies.Select(t => t.GetName().Name));
            var hotfixRefAssemblies = HotfixAssembly.GetReferencedAssemblies();
            foreach (var refAssembly in hotfixRefAssemblies)
            {
                if (nameSet.Contains(refAssembly.Name))
                    continue;

                var refPath = $"{Environment.CurrentDirectory}/{refAssembly.Name}.dll";
                if (File.Exists(refPath))
                    Assembly.LoadFrom(refPath);
            }
        }

        private void ParseDll()
        {
            foreach (var type in HotfixAssembly.GetTypes())
            {
                if (!AddAgent(type)
                    && !AddEvent(type)
                    && !AddTcpHandler(type)
                    && !AddHttpHandler(type))
                {
                    if ((HotfixBridge == null && type.GetInterface(typeof(IHotfixBridge).FullName) != null))
                    {
                        var bridge = (IHotfixBridge)Activator.CreateInstance(type);
                        if (bridge.BridgeType == Settings.ServerType)
                            HotfixBridge = bridge;
                    }
                }
            }
        }

        private bool AddHttpHandler(Type type)
        {
            if (!type.IsSubclassOf(typeof(BaseHttpHandler)))
                return false;

            var attr = (HttpMsgMapping)type.GetCustomAttribute(typeof(HttpMsgMapping));
            if (attr == null)
            {
                // 不是最终实现类
                return true;
            }

            var handler = (BaseHttpHandler)Activator.CreateInstance(type);
            if (!httpHandlerMap.TryAdd(attr.Cmd, handler))
            {
                throw new Exception($"http handler cmd重复注册，cmd:{attr.Cmd}");
            }
            return true;
        }

        public const string KEY = "MsgID";
        private bool AddTcpHandler(Type type)
        {
            var attribute = (MsgMapping)type.GetCustomAttribute(typeof(MsgMapping), true);
            if (attribute == null) return false;
            var msgIdField = attribute.Msg.GetField(KEY, BindingFlags.Static | BindingFlags.Public);
            if (msgIdField == null) return false;
            int msgId = (int)msgIdField.GetValue(null);
            if (!tcpHandlerMap.ContainsKey(msgId))
            {
                tcpHandlerMap.Add(msgId, type);
            }
            else
            {
                Log.Error("重复注册消息tcp handler:[{}] msg:[{}]", msgId, type);
            }
            return true;
        }

        private bool AddEvent(Type type)
        {
            if (!type.IsImplWithInterface(typeof(IEventListener)))
                return false;

            var compAgentType = type.BaseType.GetGenericArguments()[0];
            var compType = compAgentType.BaseType.GetGenericArguments()[0];
            var actorType = CompRegister.CompActorDic[compType];
            var evtListenersDic = actorEvtListeners.GetOrAdd(actorType);

            bool find = false;
            foreach (var attr in type.GetCustomAttributes())
            {
                if (attr is EventInfoAttribute evt)
                {
                    find = true;

                    var evtId = evt.EventId;
                    var listeners = evtListenersDic.GetOrAdd(evtId);
                    listeners.Add((IEventListener)Activator.CreateInstance(type));
                }
            }
            if (!find)
            {
                throw new Exception($"IEventListener:{type.FullName}没有指定监听的事件");
            }
            return true;
        }

        private bool AddAgent(Type type)
        {
            if (!type.IsImplWithInterface(typeof(ICompAgent)))
                return false;

            if (type.FullName.StartsWith("Wrapper.Agent.") && type.FullName.EndsWith("Wrapper"))
            {
                agentAgentWrapperMap[type.BaseType] = type;
                return true;
            }
            var compType = type.BaseType.GetGenericArguments()[0];
            if (compAgentMap.ContainsKey(compType))
            {
                throw new Exception($"comp:{compType.FullName}有多个agent");
            }

            compAgentMap[compType] = type;
            agentCompMap[type] = compType;
            return true;
        }

        internal BaseMessageHandler GetTcpHandler(int msgId)
        {
            if (tcpHandlerMap.TryGetValue(msgId, out var handlerType))
            {
                var ins = Activator.CreateInstance(handlerType);
                if (ins is BaseMessageHandler handler)
                {
                    return handler;
                }
                else
                {
                    throw new Exception($"错误的tcp handler类型，{ins.GetType().FullName}");
                }
            }
            return null;
            //throw new HandlerNotFoundException($"消息id：{msgId}");
        }

        internal BaseHttpHandler GetHttpHandler(string cmd)
        {
            if (httpHandlerMap.TryGetValue(cmd, out var handler))
            {
                return handler;
            }
            return null;
            // throw new HttpHandlerNotFoundException($"未注册的http命令:{cmd}");
        }

        internal T GetAgent<T>(BaseComp comp) where T : ICompAgent
        {
            var agentType = compAgentMap[comp.GetType()];
            var agent = (T)Activator.CreateInstance(useAgentWrapper ? agentAgentWrapperMap[agentType] : agentType);
            agent.Owner = comp;
            return agent;
        }

        internal List<IEventListener> FindListeners(ActorType actorType, int evtId)
        {
            if (actorEvtListeners.TryGetValue(actorType, out var evtListeners)
                && evtListeners.TryGetValue(evtId, out var listeners))
            {
                return listeners;
            }
            return null;
        }

        readonly ConcurrentDictionary<string, object> typeCacheMap = new();

        /// <summary>获取实例(主要用于获取Event,Timer, Schedule,的Handler实例)</summary>
        internal T GetInstance<T>(string typeName)
        {
            return (T)typeCacheMap.GetOrAdd(typeName, k => HotfixAssembly.CreateInstance(k));
        }

        internal Type GetAgentType(Type compType)
        {
            compAgentMap.TryGetValue(compType, out var agentType);
            return agentType;
        }

        internal Type GetCompType(Type agentType)
        {
            agentCompMap.TryGetValue(agentType, out var compType);
            return compType;
        }
    }
}
