using System;
using System.Collections.Generic;

namespace Geek.Server
{
    /// <summary>
    /// Component统一注册，不用提前new出来,第一次访问的时候再new
    /// </summary>
    public class ComponentMgr
    {
        public interface IComponentInfo
        {
            Type CompType { get; }
            bool IsEventListener { get; }
            BaseComponent Instance(ComponentActor actor);
        }

        public class CompTypeInfo<T> : IComponentInfo where T : BaseComponent, new()
        {
            public CompTypeInfo()
            {
                var type = typeof(T);
                IsEventListener = type.GetInterface(typeof(IEventListener).FullName) != null;
            }

            public Type CompType => typeof(T);
            public bool IsCrossDay { get; private set; }
            public bool IsEventListener { get; private set; }

            public BaseComponent Instance(ComponentActor actor)
            {
                var t = new T();
                t.Init(actor);
                return t;
            }
        }

        public static readonly ComponentMgr Singleton = new ComponentMgr();
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        readonly Dictionary<Type, List<IComponentInfo>> compMap = new Dictionary<Type, List<IComponentInfo>>();

        /// <summary>
        /// 注册组件到actor，所有组件应在起服流程中注册好，起服后注册有多线程问题
        /// </summary>
        public void RegistComp<TActor, TComp>() where TComp : BaseComponent, new() where TActor : ComponentActor
        {
            RegistComp<TComp>(typeof(TActor));
        }

        public void RegistComp<TComp>(Type actorType) where TComp : BaseComponent, new()
        {
            if (!compMap.ContainsKey(actorType))
                compMap[actorType] = new List<IComponentInfo>();
            compMap[actorType].Add(new CompTypeInfo<TComp>());
        }

        public T NewComponent<T>(ComponentActor actor) where T : BaseComponent, new()
        {
            return (T)NewComponent(actor, typeof(T));
        }

        public BaseComponent NewComponent(ComponentActor actor, Type compType)
        {
            var actorType = actor.GetType();
            if (compMap.ContainsKey(actorType))
            {
                var list = compMap[actorType];
                foreach (var info in list)
                {
                    if (info.CompType == compType)
                        return info.Instance(actor);
                }
            }
            LOGGER.Error($"{actor.GetType()}获取未注册的component:{compType}");
            return default;
        }

        public bool IsCompRegisted(ComponentActor actor, Type compType)
        {
            var actorType = actor.GetType();
            if (compMap.ContainsKey(actorType))
            {
                var list = compMap[actorType];
                foreach (var info in list)
                {
                    if (info.CompType == compType)
                        return true;
                }
            }
            return false;
        }

        public List<Type> GetAllComps(ComponentActor actor)
        {
            var actorType = actor.GetType();
            if (!compMap.ContainsKey(actorType))
                return new List<Type>();
            var retList = new List<Type>();
            var list = compMap[actorType];
            foreach (var info in list)
                retList.Add(info.CompType);
            return retList;
        }

        readonly List<IComponentInfo> EmptyInfoList = new List<IComponentInfo>();
        public List<IComponentInfo> GetAllCompsInfo(ComponentActor actor)
        {
            var actorType = actor.GetType();
            if (compMap.ContainsKey(actorType))
                return compMap[actorType];
            return EmptyInfoList;
        }
    }
}
