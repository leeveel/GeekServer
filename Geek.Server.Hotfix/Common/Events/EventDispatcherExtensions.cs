using Geek.Server.App.Common.Event;
using Geek.Server.Core.Actors;
using Geek.Server.Core.Events;
using Geek.Server.Core.Hotfix;
using Geek.Server.Core.Hotfix.Agent;
using Geek.Server.Core.Utils;
using Server.Logic.Logic.Server;

namespace Server.Logic.Common.Events
{
    public static class EventDispatcherExtensions
    {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void Dispatch(this ICompAgent agent, int evtId, Param args = null)
        {
            var evt = new Event
            {
                EventId = evtId,
                Data = args
            };

            // 自己处理
            SelfHandle(agent, evtId, evt);

            if ((EventID)evtId > EventID.RoleSeparator && agent.OwnerType > ActorType.Separator)
            {
                // 全局非玩家事件，抛给所有玩家
                agent.Tell(()
                    => ServerCompAgent.OnlineRoleForeach(role
                    => role.Dispatch(evtId, args)));
            }

            static void SelfHandle(ICompAgent agent, int evtId, Event evt)
            {
                agent.Tell(async () =>
                {
                    // 事件需要在本actor内执行，不可多线程执行，所以不能使用Task.WhenAll来处理
                    var listeners = HotfixMgr.FindListeners(agent.OwnerType, evtId);
                    if (listeners.IsNullOrEmpty())
                    {
                        // Log.Warn($"事件：{(EventID)evtId} 没有找到任何监听者");
                        return;
                    }
                    foreach (var listener in listeners)
                    {
                        var comp = await agent.GetCompAgent(listener.AgentType);
                        await listener.HandleEvent(comp, evt);
                    }
                });
            }
        }

        public static void Dispatch(this ICompAgent agent, EventID evtId, Param args = null)
        {
            Dispatch(agent, (int)evtId, args);
        }
    }
}
