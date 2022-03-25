using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;





namespace Geek.Server
{
    public class HotfixMgr
    {
        static HotfixModule module = null;
        //更新dll后可能有老代码正在跑，老代码拿agent需要返回老dll中的agent
        static readonly ConcurrentDictionary<int, HotfixModule> oldModuleMap = new ConcurrentDictionary<int, HotfixModule>();
        public volatile static bool DoingHotfix;//是否正在热更
        public static DateTime ReloadTime { get; private set; }
        public static async Task<bool> ReloadModule(string dllVersion)
        {
            var newModule = new HotfixModule();
            var success = await newModule.Load(dllVersion, module != null);
            if (!success)
                return false;

            var oldModule = module;
            bool isReload = oldModule != null;
            ReloadTime = DateTime.Now;
            if (oldModule != null)
            {
                DoingHotfix = true;
                oldModuleMap.TryAdd(oldModule.GetHashCode(), oldModule);
                _ = Task.Run(async () => {
                    await Task.Delay(1000 * 60 * 3); //延时回收,等待老dll中的代码执行完
                    if (oldModule != null)
                    {
                        oldModuleMap.TryRemove(oldModule.GetHashCode(), out _);
                        oldModule.Unload();
                    }
                    DoingHotfix = oldModuleMap.Count > 0;
                });
            }
            module = newModule;
            return await module.HotfixBridge.OnLoadSucceed(isReload);
        }

        public static Task Stop()
        {
            return module.HotfixBridge.Stop();
        }

        public static T GetHandler<T>(int msgId)
        {
            return module.GetHandler<T>(msgId);
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
        public static T GetAgent<T>(BaseComponent refOwner, Type refAssemblyType) where T : IComponentAgent
        {
            if (oldModuleMap.Count > 0)
            {
                var asb = typeof(T).Assembly;
                var asb2 = refAssemblyType?.Assembly;
                foreach (var kv in oldModuleMap)
                {
                    var old = kv.Value;
                    if(asb == old.HotfixAssembly || asb2 == old.HotfixAssembly)
                        return old.GetAgent<T>(refOwner);
                }
            }

            return module.GetAgent<T>(refOwner);
        }

        public static Type GetAgentType(Type ownerType)
        {
            return module.GetAgentType(ownerType);
        }

        public static void RemoveAgentCache(object refOwner)
        {
            foreach (var kv in oldModuleMap)
                kv.Value.RemoveAgentCache(refOwner);
            module.RemoveAgentCache(refOwner);
        }

        public static List<IEventListener> GetEventListeners(int entityType)
        {
            if (module == null)
                return null;
            return module.GetEventListeners(entityType);
        }

        /// <summary>
        /// 获取实例
        /// 主要用于获取Event,Timer, Schedule,的Handler实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static T GetInstance<T>(string typeName, Type refAssemblyType = null)
        {
            if (string.IsNullOrEmpty(typeName))
                return default;
            if (oldModuleMap.Count > 0)
            {
                var asb = refAssemblyType?.Assembly;
                foreach (var kv in oldModuleMap)
                {
                    var old = kv.Value;
                    if (asb == old.HotfixAssembly)
                        return old.GetInstance<T>(typeName);
                }
            }
            return module.GetInstance<T>(typeName);
        }

        public static Type GetType(string typeName, Type refAssemblyType = null)
        {
            if (string.IsNullOrEmpty(typeName))
                return default;
            if (oldModuleMap.Count > 0)
            {
                var asb = refAssemblyType?.Assembly;
                foreach (var kv in oldModuleMap)
                {
                    var old = kv.Value;
                    if (asb == old.HotfixAssembly)
                        return old.GetType(typeName);
                }
            }
            return module.GetType(typeName);
        }

        /// <summary>
        /// 判断对象是不是来自Hotfix,暂时只针对Param
        /// </summary>
        public static bool IsFromHotfix(Param obj)
        {
            if (obj == null)
                return false;
            return IsTypeFromHotfix(obj.GetType());
        }

        /// <summary>
        /// 判断类型是不是来自Hotfix，暂时只针对Param
        /// </summary>
        static bool IsTypeFromHotfix(Type type)
        {
            if (oldModuleMap.Count > 0)
            {
                foreach (var kv in oldModuleMap)
                {
                    var old = kv.Value;
                    if (old.IsTypeFromHotfix(type))
                        return true;
                }
            }
            return module.IsTypeFromHotfix(type);
        }
    }
}