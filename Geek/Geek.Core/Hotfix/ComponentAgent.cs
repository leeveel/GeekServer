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
using Base;
using System;
using System.Threading.Tasks;
using Geek.Core.Component;
using Geek.Core.Timer;

namespace Geek.Core.Hotfix
{
    public abstract class ComponentAgent<T> : IComponentAgent where T : BaseComponent
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public T Comp => (T)Owner;
        public ComponentActor Actor => Comp.Actor;
        public TA GetActor<TA>() where TA : ComponentActor { return (TA)Actor; }
        public IComponentActorAgent ActorAgent => Comp.Actor.Agent;
        public object Owner { get; set; }
        /// <summary>
        /// 获取actor身上的Component,线程安全
        /// </summary>
        public Task<TComp> GetComponent<TComp>() where TComp : BaseComponent, new()
        {
            return Actor.GetComponent<TComp>();
        }

        public virtual Task Active()
        {
            return Task.CompletedTask;
        }

        public virtual Task Deactive()
        {
            return Task.CompletedTask;
        }
        
        /// <summary>延时回调</summary>
        public long DelayCall<TH>(long delay, Param param = null) where TH : ITimerHandler
        {
            if (HotfixMgr.IsFromHotfix(param))
            {
                LOGGER.Fatal("不能添加hotfix工程的类型作为timer参数 DelayCall");
                return -1;
            }
            return Actor.Timer.DelayCall(delay, typeof(TH).FullName, param);
        }

        /// <summary>定时回调</summary>
        public long AddTimer<TH>(long delay, long period, Param param = null) where TH : ITimerHandler
        {
            if (HotfixMgr.IsFromHotfix(param))
            {
                LOGGER.Fatal("不能添加hotfix工程的类型作为timer参数 AddTimer");
                return -1;
            }
            return Actor.Timer.AddTimer(delay, period, typeof(TH).FullName, param);
        }

        /// <summary>取消延时</summary>
        public void RemoveTimer(long id)
        {
            Actor.Timer.RemoveTimer(id);
        }

        /// <summary>定时任务/每周</summary>
        public long Schedule<TH>(int hour, int minute, Param param = null, params DayOfWeek[] days) where TH : ITimerHandler
        {
            if (HotfixMgr.IsFromHotfix(param))
            {
                LOGGER.Fatal("不能添加hotfix工程的类型作为Schedule参数 1");
                return -1;
            }
            return Actor.Timer.Schedule(hour, minute, typeof(TH).FullName, param, days);
        }

        /// <summary>定时任务/每月</summary>
        public long Schedule<TH>(int date, int hour, int minute, Param param = null) where TH : ITimerHandler
        {
            if (HotfixMgr.IsFromHotfix(param))
            {
                LOGGER.Fatal("不能添加hotfix工程的类型作为Schedule参数 2");
                return -1;
            }
            return Actor.Timer.Schedule(date, hour, minute, typeof(TH).FullName, param);
        }

        /// <summary>定时任务/一次性</summary>
        public long Schedule<TH>(DateTime dateTime, Param param = null) where TH : ITimerHandler
        {
            if (HotfixMgr.IsFromHotfix(param))
            {
                LOGGER.Fatal("不能添加hotfix工程的类型作为Schedule参数 3");
                return -1;
            }
            return Actor.Timer.Schedule(dateTime, typeof(TH).FullName, param);
        }

        /// <summary>定时任务/每每天</summary>
        public long Schedule<TH>(int hour, int minute, Param param = null) where TH : ITimerHandler
        {
            if (HotfixMgr.IsFromHotfix(param))
            {
                LOGGER.Fatal("不能添加hotfix工程的类型作为Schedule参数 4");
                return -1;
            }
            return Actor.Timer.Schedule(hour, minute, typeof(TH).FullName, param);
        }

        /// <summary>取消定时</summary>
        public void Unschedule(long id)
        {
            Actor.Timer.Unschedule(id);
        }
    }
}