using System.Threading.Tasks;

namespace Geek.Server
{

    /// <summary>
    /// 固定ID实体类型的Handler
    /// FixedIDEntityHandler
    /// </summary>
    public abstract class FixedIdEntityHandler<TAgent> : TcpCompHandler<TAgent> where TAgent : IComponentAgent
    {
        public abstract EntityType EntityType { get; }
        protected long RoleId => SessionId;

        public override Task<long> GetEntityId()
        {
            long entityId = EntityID.GetID(EntityType);
            return Task.FromResult(entityId);
        }

        public Task<RoleAgent> GetRoleCompAgent<RoleAgent>() where RoleAgent : IComponentAgent
        {
            return EntityMgr.GetCompAgent<RoleAgent>(RoleId);
        }

        protected virtual void WriteAndFlush(MSG msg)
        {
            if (msg.MsgId > 0)
            {
                msg.msg.UniId = Msg.UniId;  //写入req中的UniId
                WriteAndFlush(msg.MsgId, msg.ByteArr);
            }
            NotifyErrorCode(msg.Info);
        }


    }
}
