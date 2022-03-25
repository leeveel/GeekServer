
using Google.Protobuf;
using Grpc.Core;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geek.Server
{
    public class GrpcClient
    {

        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private static readonly ConcurrentDictionary<int, Channel> clientDic = new ConcurrentDictionary<int, Channel>();

        public static Task Showdown()
        {
            var taskList = new List<Task>();
            foreach (var kv in clientDic)
                taskList.Add(kv.Value.ShutdownAsync());
            return Task.WhenAll(taskList);
        }

        public static async Task<int> Invoke(int serverId, IMessage msg, long actorId)
        {
            LOGGER.Debug($"发起grpc请求 目标serverId：{serverId} msgId：{msg.GetMsgId()} msgType:{msg.GetType()}");
            var res = await Invoke<IMessage>(serverId, actorId, msg.GetMsgId(), msg.Serialize());
            if(!GrpcRes.IsOK(res.Code))
                LOGGER.Error($"目标serverId：{serverId} msgId：{msg.GetMsgId()} msgType:{msg.GetType()} 调用失败{res.Code}");
            return res.Code;
        }

        public static async Task<GrpcRes<T>> Invoke<T>(int serverId, IMessage msg, long actorId) where T : IMessage
        {
            LOGGER.Debug($"发起grpc请求 目标serverId：{serverId} msgId：{msg.GetMsgId()} msgType:{msg.GetType()}");
            return await Invoke<T>(serverId, actorId, msg.GetMsgId(), msg.Serialize());
        }

        private static async Task<GrpcRes<T>> Invoke<T>(int serverId, long actorId, int msgId, byte[] data) where T : IMessage
        {
            // 创建通道成本高昂。 重用 gRPC 调用的通道可提高性能。
            // 通道和从该通道创建的客户端可由多个线程安全使用。
            // 从通道创建的客户端可同时进行多个调用。
            if (!clientDic.TryGetValue(serverId, out var channel) || channel.State == ChannelState.Shutdown)
            {
                //var serverConfig = await ConsulUtils.GetServerConfig(serverId);
                ServerConfig serverConfig = await ServerInfoUtils.GetServerConfig(serverId);
                if (serverConfig == null)
                    return PackResult<T>(GrpcCode.TARGET_SERVER_CONFIG_NOT_FOUND);

                channel = new Channel($"{serverConfig.Ip}:{serverConfig.GrpcPort}", ChannelCredentials.Insecure);

                clientDic.TryRemove(serverId, out _);
                clientDic.TryAdd(serverId, channel);
            }

            // gRPC 客户端是使用通道创建的。 gRPC 客户端是轻型对象，无需缓存或重用。
            Inner.InnerClient client = new Inner.InnerClient(channel);

            if (!await ServerInfoUtils.IsAlive(serverId))
                return PackResult<T>(GrpcCode.APP_NOT_RUNNING);

            var rpcMsg = new RpcMessage
            {
                MsgId = msgId,
                ActorId = actorId,
                Data = ByteString.CopyFrom(data),
                ServerId = Settings.Ins.ServerId
            };

            RpcReply reply;
            try
            {
                reply = await client.InvokeAsync(rpcMsg, deadline: DateTime.UtcNow.AddSeconds(4));
            }
            catch (RpcException e)
            {
                if (e.StatusCode == StatusCode.Unavailable)
                {
                    try
                    {
                        await Task.Delay(50);
                        reply = await client.InvokeAsync(rpcMsg, deadline: DateTime.UtcNow.AddSeconds(4));
                    }
                    catch (RpcException e1)
                    {
                        LOGGER.Error($"grpc调用异常 重发无效 serverId:{serverId} actorId:{actorId} msgId:{msgId} e:{e1.Message}");
                        return PackResult<T>(GrpcCode.GRPC_CALL_EXCEPTION);
                    }
                }
                else
                {
                    LOGGER.Error($"grpc调用异常 serverId:{serverId} actorId:{actorId} msgId:{msgId} e:{e.Message}");
                    return PackResult<T>(GrpcCode.GRPC_CALL_EXCEPTION);
                }
            }

            GrpcCode code =(GrpcCode) reply.Code;
            if (reply.MsgId <= 0)
                return PackResult<T>(code);

            T replyMsg = (T) MsgFactory.GetMsg(reply.MsgId);
            if (replyMsg == null)
            {
                LOGGER.Error($"grpc消息反序列化失败，消息未注册. msgId:{reply.MsgId}");
                return PackResult<T>(code);
            }

            replyMsg.Deserialize(reply.Data.ToByteArray());
            return PackResult(code, replyMsg);
        }

        private static GrpcRes<T> PackResult<T>(GrpcCode code, T msg) where T : IMessage
        {
            return new GrpcRes<T>((int)code, msg);
        }

        private static GrpcRes<T> PackResult<T>(GrpcCode code) where T : IMessage
        {
            return new GrpcRes<T>((int)code, null);
        }
    }
}
