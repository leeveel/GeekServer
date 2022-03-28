

using System;
using System.Collections.Generic;

namespace Geek.Server
{
    /// <summary>
    /// Component统一注册，不用提前new出来,第一次访问的时候再new
    /// </summary>
    public class CompSetting
    {
        public interface IComponentInfo
        {
            Type CompType { get; }
            bool AutoActive { get; }
            bool IsInterface(Type interfaceType);
            BaseComponent Instance(WorkerActor actor, int entityType, long entityId);
        }

        public class CompTypeInfo<T> : IComponentInfo where T : BaseComponent, new()
        {
            private bool autoActive = false;
            public CompTypeInfo(bool autoActive = false)
            {
                this.autoActive = autoActive;
            }

            public bool IsInterface(Type interfaceType)
            {
                var type = typeof(T);
                return type.GetInterface(interfaceType.FullName) != null;
            }

            public Type CompType => typeof(T);
            public bool AutoActive => autoActive;
            public BaseComponent Instance(WorkerActor actor, int entityType, long entityId)
            {
                var t = new T();
                t.Init(actor, entityType, entityId);
                return t;
            }
        }

        public static readonly CompSetting Singleton = new CompSetting();
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        readonly Dictionary<int, bool> entityActorTypeMap = new Dictionary<int, bool>();//是否comp共享一个actor
        readonly Dictionary<int, List<IComponentInfo>> compMap = new Dictionary<int, List<IComponentInfo>>();
        readonly Dictionary<Type, List<int>> actorMap = new Dictionary<Type, List<int>>();

        /// <summary>起服时自动激活的实体类型</summary>
        public List<int> AutoActiveEntityList { get; } = new List<int>();

        public void SetIfEntityCompShareActor(int entityType, bool compShareActor)
        {
            entityActorTypeMap[entityType] = compShareActor;
        }

        public bool IsEntityShareActor(int entityType)
        {
            if (!entityActorTypeMap.TryGetValue(entityType, out var share))
                return true;
            return share;
        }

        public bool IsAutoActive(int entityType)
        {
            return AutoActiveEntityList.Contains(entityType);
        }

        /// <summary>
        /// 注册组件到actor，所有组件应在起服流程中注册好，起服后注册有多线程问题
        /// </summary>
        public void RegistComp<TComp>(int entityType, bool activeWhenStartServer = false) where TComp : BaseComponent, new()
        {
            if (!compMap.ContainsKey(entityType))
                compMap[entityType] = new List<IComponentInfo>();
            compMap[entityType].Add(new CompTypeInfo<TComp>(activeWhenStartServer));
            if (activeWhenStartServer && !AutoActiveEntityList.Contains(entityType))
                AutoActiveEntityList.Add(entityType);

            if (!actorMap.ContainsKey(typeof(TComp)))
                actorMap[typeof(TComp)] = new List<int>();
            actorMap[typeof(TComp)].Add(entityType);
        }


        public List<int> GetEntityTypeList(Type compType)
        {
            if (actorMap.ContainsKey(compType))
                return actorMap[compType];
            return null;
        }

        public T NewComponent<T>(WorkerActor actor, int entityType, long entityId) where T : BaseComponent, new()
        {
            return (T)NewComponent(actor, entityType, entityId, typeof(T));
        }

        public BaseComponent NewComponent(WorkerActor actor, int entityType, long entityId, Type compType)
        {
            if (compMap.ContainsKey(entityType))
            {
                var list = compMap[entityType];
                foreach (var info in list)
                {
                    if (info.CompType == compType)
                        return info.Instance(actor, entityType, entityId);
                }
            }
            LOGGER.Error($"entityType={entityType} 获取未注册的component:{compType}");
            return default;
        }

        public bool IsCompRegisted(int entityType, Type compType)
        {
            if (compMap.ContainsKey(entityType))
            {
                var list = compMap[entityType];
                foreach (var info in list)
                {
                    if (info.CompType == compType)
                        return true;
                }
            }
            return false;
        }

        public List<Type> GetAllComps(int entityType)
        {
            if (!compMap.ContainsKey(entityType))
                return new List<Type>();
            var retList = new List<Type>();
            var list = compMap[entityType];
            foreach (var info in list)
                retList.Add(info.CompType);
            return retList;
        }

        readonly List<IComponentInfo> EmptyInfoList = new List<IComponentInfo>();
        public List<IComponentInfo> GetAllCompsInfo(int entityType)
        {
            if (compMap.ContainsKey(entityType))
                return compMap[entityType];
            return EmptyInfoList;
        }
    }
}
