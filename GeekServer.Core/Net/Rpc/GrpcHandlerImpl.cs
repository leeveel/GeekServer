
using Google.Protobuf;
using Grpc.Core;
using NLog;
using System;
using System.Threading.Tasks;

namespace Geek.Server
{
    /// <summary>
    /// grpc最初处理消息的类
    /// </summary>
    public class GrpcHandlerImpl : Inner.InnerBase
    {

        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        public override async Task<RpcReply> Invoke(RpcMessage request, ServerCallContext context)
        {
            if (!Settings.Ins.AppRunning)
                return new RpcReply { Code = (int)GrpcCode.APP_NOT_RUNNING };

            int reqMsgId = request.MsgId;

            IMessage msg = MsgFactory.GetMsg(reqMsgId);
            if (msg == null)
                return ErrorMsg(GrpcCode.REQUEST_MSG_ERROR);

            GrpcBaseHandler handler = HotfixMgr.GetHandler<GrpcBaseHandler>(reqMsgId);
            if (handler == null)
                return ErrorMsg(GrpcCode.HANDLER_NOT_FOUND);

            msg.Deserialize(request.Data.ToByteArray());
            handler.Msg = msg;
            handler.EntityId = request.ActorId;
            handler.ServerId = request.ServerId;

            LOGGER.Debug($"收到Grpc请求 msgId：{reqMsgId} reqType:{request.GetType().FullName} reqServerId:{request.ServerId} actorId:{request.ActorId} handlerType:{handler.GetType()} msgType:{handler.Msg.GetType()}");
            GrpcRes res = null;
            try
            {
                res = await handler.InnerActionAsync();
            }
            catch (Exception e)
            {
                LOGGER.Error($"grpc handler调用异常 msgId：{reqMsgId} reqType:{request.GetType().FullName} reqServerId:{request.ServerId} actorId:{request.ActorId} e:{e.Message}");
                return ErrorMsg(GrpcCode.HANDLER_EXCEPTION);
            }

            LOGGER.Debug($"处理完成Grpc请求 msgId：{reqMsgId} reqType:{request.GetType().FullName} reqServerId:{request.ServerId} actorId:{request.ActorId} 返回code：{res.Code} {(res.imsg == null ? "返回空消息" : $"返回类型msgId：{res.imsg.GetMsgId()} 返回消息类型：{res.imsg.GetType()}")}");

            var reply = new RpcReply { Code = res.Code };
            if (res.imsg != null)
            {
                reply.MsgId = res.imsg.GetMsgId();
                reply.Data = ByteString.CopyFrom(res.imsg.Serialize());
            }

            return reply;
        }

        private static RpcReply ErrorMsg(GrpcCode code)
        {
            return new RpcReply { Code = (int)code };
        }
    }
}
