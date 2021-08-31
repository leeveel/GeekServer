using System.Threading.Tasks;

namespace Geek.Server
{

    /// <summary>
    /// 请使用ActorMgr而不是ActorManager
    /// </summary>
    public static class ActorMgr
    {

        public static Task<ComponentActor> GetOrNew(long id)
        {
            return ActorManager.GetOrNew(id);
        }

        public static Task<ComponentActor> GetOrNew(ActorType actorType)
        {
            long id = ActorID.GetID(actorType);
            return ActorManager.GetOrNew(id);
        }

        public static async Task<TAgent> GetCompAgent<TAgent>(ActorType actorType) where TAgent : IComponentAgent, new()
        {
            long id = ActorID.GetID(actorType);
            return await ActorManager.GetCompAgent<TAgent>(id);
        }

        public static async Task<TAgent> GetCompAgent<TAgent>(long actorId) where TAgent : IComponentAgent, new()
        {
            return await ActorManager.GetCompAgent<TAgent>(actorId);
        }

    }
}
