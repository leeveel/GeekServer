using Geek.Server.Logic.Role;
using System.Threading.Tasks;

namespace Geek.Server
{

    /// <summary>
    /// Global Event Dispatcher
    /// </summary>
    public static class GED
    {
        public static void AddListener<T>(long roleId, EventID evtId) where T : IEventListener
        {
            GlobalEventDispatcher.AddListener(roleId, (int)evtId, typeof(T).FullName);
        }

        public static void AddListener(long roleId, EventID evtId, IEventListener listener)
        {
            GlobalEventDispatcher.AddListener(roleId, (int)evtId, listener.GetType().FullName);
        }

        public static void RemoveListener<T>(long roleId, EventID evtId) where T : IEventListener
        {
            GlobalEventDispatcher.RemoveListener(roleId, (int)evtId, typeof(T).FullName);
        }

        public static void ClearListener(long roleId)
        {
            GlobalEventDispatcher.ClearListener(roleId);
        }

        public static void DispatchEvent(EventID evtId, Param param = null)
        {
            GlobalEventDispatcher.DispatchEvent((int)evtId, CheckRoleDispatch, param);
        }

        private static async Task<bool> CheckRoleDispatch(int evtType, ComponentActor actor, System.Type compType)
        {
            if (actor == null || compType == null)
                return false;

            if (actor.ActorType == (int)ActorType.Role)
            {
                //在线时始终回调
                var roleComp = await actor.GetCompAgent<RoleCompAgent>();
                if (await roleComp.IsOnline())
                    return true;
                //不在线时只要Comp属于激活状态就要回调[红点]
                return await actor.IsCompActive(compType);
            }
            //其他类型直接回调(也可根据自己的需求在此更改)
            return true;
        }
    }


    public static class EventExt
    {

        readonly static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public static void AddListener<T>(this EventDispatcher dispatcher, EventID evtId) where T : IEventListener
        {
            if (evtId > EventID.Separator)
                GED.AddListener<T>(dispatcher.ownerActor.ActorId, evtId);
            else if (evtId < EventID.Separator)
                dispatcher.AddListener((int)evtId, typeof(T).FullName);
            else
                LOGGER.Error($"请勿将EventID.Separator用于业务逻辑{evtId}");
        }

        public static void AddListener(this EventDispatcher dispatcher, EventID evtId, IEventListener listener)
        {
            if (evtId > EventID.Separator)
                GED.AddListener(dispatcher.ownerActor.ActorId, evtId, listener);
            else if (evtId < EventID.Separator)
                dispatcher.AddListener((int)evtId, listener.GetType().FullName);
            else
                LOGGER.Error($"请勿将EventID.Separator用于业务逻辑{evtId}");
        }

        public static void RemoveListener<T>(this EventDispatcher dispatcher, EventID evtId) where T : IEventListener
        {
            if (evtId > EventID.Separator)
                GED.RemoveListener<T>(dispatcher.ownerActor.ActorId, evtId);
            else if (evtId < EventID.Separator)
                dispatcher.RemoveListener((int)evtId, typeof(T).FullName);
            else
                LOGGER.Error($"请勿将EventID.Separator用于业务逻辑{evtId}");
        }

        public static void DispatchEvent(this EventDispatcher dispatcher, EventID evtId, Param param = null)
        {
            dispatcher.DispatchEvent((int)evtId, param);
        }
        
    }
}