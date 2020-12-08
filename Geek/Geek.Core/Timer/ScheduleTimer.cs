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
using Geek.Core.Hotfix;
using System.Collections.Generic;
using System.Threading.Tasks;
using Geek.Core.Actor;

namespace Geek.Core.Timer
{
    public class ScheduleTimer
    {
        const int TypeWeekly = 1;
        const int TypeMonthly = 2;
        const int TypeDaily = 3;
        const int TypeOnce = 4;

        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        private class ScheduleInfo
        {
            public int type; //1每周几,2每月几,3每天,4单次
            public bool loop;
            public bool firedToday;
            public List<DayOfWeek> dayOfWeeks;
            public DateTime due;

            public string handlerType;
            public Param param;
        }

        private class TimerInfo
        {
            public long waitTime;
            public long period;
            public Func<Param, Task> callback;
            public string handlerType;
            public Param param;
        }

        long currentId = 0;
        Dictionary<long, TimerInfo> timerMap = new Dictionary<long, TimerInfo>();
        List<long> timerRemoveList = new List<long>();

        BaseActor actor;
        public ScheduleTimer(BaseActor actor)
        {
            this.actor = actor;
        }

        /// <summary>延时回调</summary>
        internal long DelayCall(long delay, Func<Param, Task> callback, Param param = null)
        {
            return AddTimer(delay, -1, callback, param);
        }

        /// <summary>定时回调</summary>
        internal long AddTimer(long delay, long period, Func<Param, Task> callback, Param param = null)
        {
            return AddTimer(delay, period, callback, null, param);
        }

        /// <summary>延时回调</summary>
        public long DelayCall(long delay, string handlerType, Param param = null)
        {
            return AddTimer(delay, -1, handlerType, param);
        }

        /// <summary>定时回调</summary>
        public long AddTimer(long delay, long period, string handlerType, Param param = null)
        {
            return AddTimer(delay, period, null, handlerType, param);
        }

        long AddTimer(long delay, long period, Func<Param, Task> callback, string handlerType, Param param)
        {
            var timer = new TimerInfo()
            {
                period = period,
                waitTime = delay,
                handlerType = handlerType,
                callback = callback,
            };

            timer.param = param;
            long id = ++currentId;
            timerMap.Add(id, timer);
            return id;
        }

        /// <summary>delayCall/timer移除</summary>
        public void RemoveTimer(long id)
        {
            timerRemoveList.Add(id);
        }

        public async Task Tick(long deltaTime)
        {
            #region timer
            foreach (var id in timerRemoveList)
            {
                if (timerMap.ContainsKey(id))
                    timerMap.Remove(id);
            }
            timerRemoveList.Clear();

            var timerIdList = new List<long>(timerMap.Keys);
            foreach (var id in timerIdList)
            {
                if (!timerMap.ContainsKey(id))
                    continue;
                var timer = timerMap[id];
                timer.waitTime -= deltaTime;
                if (timer.waitTime <= 0)
                {
                    if (timer.period < 0) //delay call
                        timerRemoveList.Add(id);

                    timer.waitTime += timer.period;
                    try
                    {
                        if (timer.callback != null)
                            await timer.callback(timer.param);
                        var handler = HotfixMgr.GetInstance<ITimerHandler>(timer.handlerType);
                        if(handler != null)
                            await handler.HandleTimer(actor, timer.param);
                    }
                    catch (Exception e)
                    {
                        LOGGER.Error(e.ToString());
                    }
                }
            }
            #endregion



            #region schedule
            foreach (var id in scheduleRemoveList)
            {
                if (scheduleMap.ContainsKey(id))
                    scheduleMap.Remove(id);
            }
            scheduleRemoveList.Clear();

            var now = DateTime.Now;
            var idList = new List<long>(scheduleMap.Keys);
            foreach (var id in idList)
            {
                if (!scheduleMap.ContainsKey(id))
                    continue;
                var info = scheduleMap[id];
                if (info.firedToday)
                    continue;
                if (info.due > now)
                    continue;
                info.firedToday = true;
                switch (info.type)
                {
                    case TypeDaily:
                        if (!info.loop)
                            scheduleRemoveList.Add(id);
                        break;
                    case TypeMonthly:
                        if (!info.loop)
                            scheduleRemoveList.Add(id);
                        break;
                    case TypeWeekly:
                        if (!info.dayOfWeeks.Contains(now.DayOfWeek))
                            continue;
                        break;
                    case TypeOnce:
                        scheduleRemoveList.Add(id);
                        break;
                }
                info.due = info.due.AddDays(1);
                info.firedToday = false;
                try
                {
                    var handler = HotfixMgr.GetInstance<ITimerHandler>(info.handlerType);
                    if (handler != null)
                        await handler.HandleTimer(actor, info.param);
                }
                catch (Exception e)
                {
                    LOGGER.Error(e.ToString());
                }
            }
            #endregion
        }

        Dictionary<long, ScheduleInfo> scheduleMap = new Dictionary<long, ScheduleInfo>();
        List<long> scheduleRemoveList = new List<long>();

        /// <summary>定时任务/每周</summary>
        public long Schedule(int hour, int minute, string handlerType, Param param = null, params DayOfWeek[] days)
        {
            var now = DateTime.Now;
            var due = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);
            if (now > due)
                due = due.AddDays(1);
            return Schedule(TypeWeekly, due, null, handlerType, param);
        }

        /// <summary>定时任务/每月</summary>
        public long Schedule(int date, int hour, int minute, string handlerType, Param param = null)
        {
            var now = DateTime.Now;
            var due = new DateTime(now.Year, now.Month, date, hour, minute, 0);
            if (now > due)
                due = due.AddDays(1);
            return Schedule(TypeMonthly, due, null, handlerType, param);
        }

        /// <summary>定时任务/一次性</summary>
        public long Schedule(DateTime dateTime, string handlerType, Param param = null)
        {
            return Schedule(TypeOnce, dateTime, null, handlerType, param);
        }

        /// <summary>定时任务/每每天</summary>
        public long Schedule(int hour, int minute, string handlerType, Param param = null)
        {
            var now = DateTime.Now;
            var due = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);
            if (now > due)
                due = due.AddDays(1);
            return Schedule(TypeDaily, due, null, handlerType, param);
        }

        long Schedule(int type, DateTime due, List<DayOfWeek> dayOfWeeks, string handlerType, Param param)
        {
            var info = new ScheduleInfo()
            {
                due = due,
                type = type,
                param = param,
                dayOfWeeks = dayOfWeeks,
                handlerType = handlerType,
                loop = type != TypeOnce
            };
            long id = ++currentId;
            scheduleMap.Add(id, info);
            return id;
        }

        /// <summary>取消定时任务</summary>
        public void Unschedule(long id)
        {
            scheduleRemoveList.Add(id);
        }
    }
}