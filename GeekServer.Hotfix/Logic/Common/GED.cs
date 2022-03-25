using System.Threading.Tasks;
using System;
using Geek.Server.Logic.Role;

namespace Geek.Server
{
    public static class GED
    {
        readonly static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public static void AddListener<T>(Enum entityType, EventID evtId) where T : IEventListener
        {
            int intType = (int)(object)entityType;
            var entityId = EntityMgr.Type2ID(intType);
            AddListener<T>(entityId, evtId);
        }

        public static void AddListener<T>(long entityId, EventID evtId) where T : IEventListener
        {
            if (evtId > EventID.Separator)
                GlobalEventDispatcher.AddListener(entityId, (int)evtId, typeof(T).FullName);
            else if (evtId < EventID.Separator)
                EventDispatcher.AddListener(entityId, (int)evtId, typeof(T).FullName);
            else
                LOGGER.Warn($"请勿将EventID.Separator用于业务逻辑{evtId}");
        }

        public static void AddListener(long entityId, EventID evtId, IEventListener listener)
        {
            if (evtId > EventID.Separator)
                GlobalEventDispatcher.AddListener(entityId, (int)evtId, listener.GetType().FullName);
            else if (evtId < EventID.Separator)
                EventDispatcher.AddListener(entityId, (int)evtId, listener.GetType().FullName);
            else
                LOGGER.Warn($"请勿将EventID.Separator用于业务逻辑{evtId}");
        }

        public static void RemoveListener<T>(long entityId, EventID evtId) where T : IEventListener
        {
            if (evtId > EventID.Separator)
                GlobalEventDispatcher.RemoveListener(entityId, (int)evtId, typeof(T).FullName);
            else if (evtId < EventID.Separator)
                EventDispatcher.RemoveListener(entityId, (int)evtId, typeof(T).FullName);
            else
                LOGGER.Warn($"请勿将EventID.Separator用于业务逻辑{evtId}");
        }

        public static void DispatchEvent(long entityId, EventID evtId, Param param = null)
        {
            if (evtId > EventID.Separator)
                GlobalEventDispatcher.DispatchEvent((int)evtId, CheckRoleDispatch, param);
            else if (evtId < EventID.Separator)
                EventDispatcher.DispatchEvent(entityId, (int)evtId, param);
            else
                LOGGER.Warn($"请勿将EventID.Separator用于业务逻辑{evtId}");
        }

        public static void DispatchEvent(this IComponentAgent agent, EventID evtId, Param param = null)
        {
            DispatchEvent(agent.EntityId, evtId, param);
        }

        private static async Task<bool> CheckRoleDispatch(int evtType, long entityId, System.Type compType)
        {
            if (compType == null)
                return false;

            var entityType = EntityID.GetEntityType(entityId);
            if (entityType == EntityType.Role)
            {
                //在线时始终回调
                //var roleComp = await EntityMgr.GetCompAgent<RoleCompAgent>(entityId);
                //if (await roleComp.IsOnline())
                if(await RoleCompAgent.IsRoleOnline(entityId))
                    return true;
                //不在线时只要Comp属于激活状态就要回调[红点]
                return await EntityMgr.IsCompActive(entityId, compType);
            }
            //其他类型直接回调(也可根据自己的需求在此更改)
            return true;
        }
    }
}