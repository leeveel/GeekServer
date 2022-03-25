
using NLog;
using System;
using System.Threading.Tasks;

namespace Geek.Server
{
    public abstract class GrpcCompHandler<T> : GrpcBaseHandler where T : IComponentAgent
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        protected T CompAgent;
        public async override Task<GrpcRes> InnerActionAsync()
        {
            CompAgent = await EntityMgr.GetCompAgent<T>(EntityId);
            if (CompAgent == null)
            {
                LOGGER.Error($"目标服务器找不到Comp：{EntityId}");
                throw new Exception($"目标服务器找不到Comp：{EntityId}");
            }
            return await DoAction();
        }


        protected Task<GrpcRes> DoAction()
        {
            return CompAgent.Owner.Actor.SendAsync(ActionAsync);
        }
    }
}
