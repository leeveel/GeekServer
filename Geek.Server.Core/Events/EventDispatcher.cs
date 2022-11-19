
using Geek.Server.Core.Actors;
using Geek.Server.Core.Hotfix;
using Geek.Server.Core.Utils;

namespace Geek.Server.Core.Events
{
    public static class EventDispatcher
    {
        public static void Dispatch(long id, int evtId, Param args = null)
        {
            var actor = ActorMgr.GetActor(id);
            if (actor != null)
            {
                var evt = new Event
                {
                    EventId = evtId,
                    Data = args
                };

                actor.Tell(async () =>
                {
                    // 事件需要在本actor内执行，不可多线程执行，所以不能使用Task.WhenAll来处理
                    var listeners = HotfixMgr.FindListeners(actor.Type, evtId);
                    if (listeners.IsNullOrEmpty())
                    {
                        // Log.Warn($"事件：{(EventID)evtId} 没有找到任何监听者");
                        return;
                    }
                    foreach (var listener in listeners)
                    {
                        var comp = await actor.GetCompAgent(listener.AgentType);
                        await listener.HandleEvent(comp, evt);
                    }
                });
            }
        }
    }
}
