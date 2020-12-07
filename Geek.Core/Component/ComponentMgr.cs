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
using Geek.Core.Events;
using System.Collections.Generic;
using Geek.Core.CrossDay;

namespace Geek.Core.Component
{
    /// <summary>
    /// Component统一注册，不用提前new出来,第一次访问的时候再new
    /// </summary>
    public class ComponentMgr
    {
        public interface IComponentInfo
        {
            Type CompType { get; }
            bool IsTick { get; }
            bool IsCrossDay { get; }
            bool IsEventListener { get; }
            BaseComponent Instance(ComponentActor actor);
        }

        public class CompTypeInfo<T> : IComponentInfo where T : BaseComponent, new()
        {
            public CompTypeInfo()
            {
                var type = typeof(T);
                IsTick = type.GetInterface(typeof(ITick).FullName) != null;
                IsCrossDay = type.GetInterface(typeof(ICrossDay).FullName) != null;
                IsEventListener = type.GetInterface(typeof(IEventListener).FullName) != null;
            }

            public Type CompType => typeof(T);
            public bool IsTick { get; private set; }
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
        Dictionary<Type, List<IComponentInfo>> compMap = new Dictionary<Type, List<IComponentInfo>>();
        public void RegistComp<TActor, TComp>() where TComp : BaseComponent, new() where TActor : ComponentActor
        {
            if (!compMap.ContainsKey(typeof(TActor)))
                compMap[typeof(TActor)] = new List<IComponentInfo>();
            compMap[typeof(TActor)].Add(new CompTypeInfo<TComp>());
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
                    if (info.CompType== compType)
                        return info.Instance(actor);
                }
            }
            LOGGER.Error($"{actor.GetType()}获取未注册的component>{compType}");
            return default;
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
