using Consul;
using Geek.Server.Core.Center;
using Geek.Server.Core.Serialize.PolymorphicMessagePack;
using MessagePack;
using Microsoft.AspNetCore.DataProtection;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geek.Server.Core.Actors.Impl
{
    [PolymorphicIgnore]
    [MessagePackObject(true)]
    public class ActorRemoteCallParams
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        public long targetActorId;
        public string agentName;
        public string funcName;
        public List<string> realParamTypes = new List<string>();
        public List<byte[]> paramDatas = new List<byte[]>();

        public void AddParam<T>(T data)
        {
            Type type = null;
            if (data == null)
            {
                type = typeof(T);
            }
            else
            {
                type = data.GetType();
            }
            realParamTypes.Add(type.FullName);
            paramDatas.Add(MessagePack.MessagePackSerializer.Serialize(type, data));
        }

        public T GetParam<T>(int index, T defValue = default(T))
        {
            if (index >= realParamTypes.Count)
            {
                LOGGER.Fatal($"actor远程调用,参数index不匹配{targetActorId}{agentName}{funcName}");
                return defValue;
            }
            var type = ActorRemoteCall.typeGetter(realParamTypes[index]);
            return (T)MessagePack.MessagePackSerializer.Deserialize(type, paramDatas[index]);
        }
    }

    [PolymorphicIgnore]
    [MessagePackObject(true)]
    public class ActorRemoteCallResult
    {
        public bool success;
        public byte[] resultData;
    }


    public static class ActorRemoteCall
    {
        static BaseCenterRpcClient rpcClient;
        internal static Func<string, Type> typeGetter;

        public static void SetRpc(BaseCenterRpcClient rpc, Func<string, Type> typeGetter)
        {
            rpcClient = rpc;
            ActorRemoteCall.typeGetter = typeGetter;
        }

        public static Task Call(ActorType targetType, ActorRemoteCallParams param)
        {
            var node = rpcClient?.GetNode(NodeType.Game, targetType);
            if (node != null)
                return rpcClient.ServerAgent.ActorAgentCall(node.NodeId, MessagePack.MessagePackSerializer.Serialize(param));
            return Task.CompletedTask;
        }

        public static async Task<TResult> Call<TResult>(ActorType targetType, ActorRemoteCallParams param)
        {
            var node = rpcClient?.GetNode(NodeType.Game, targetType);
            if (node != null)
            {
                var ret = await rpcClient.ServerAgent.ActorAgentCall(node.NodeId, MessagePack.MessagePackSerializer.Serialize(param));
                if (ret != null && ret.success)
                {
                    return MessagePack.MessagePackSerializer.Deserialize<TResult>(ret.resultData);
                }
            }
            return default(TResult);
        }
    }
}
