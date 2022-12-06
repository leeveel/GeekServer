using Consul;
using Geek.Server.Core.Center;
using Geek.Server.Core.Serialize.PolymorphicMessagePack;
using Grpc.Net.Client;
using MagicOnion.Client;
using MessagePack;
using Microsoft.AspNetCore.DataProtection;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
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
            var type = ActorRemoteCallHelper.typeGetter(realParamTypes[index]);
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

    public static class ActorRemoteCallHelper
    {
        static ConcurrentDictionary<int, KeyValuePair<string, IActorAgentRemoteCallService>> nodeAARCSMap = new ConcurrentDictionary<int, KeyValuePair<string, IActorAgentRemoteCallService>>();
        internal static Func<string, Type> typeGetter;
        internal static Func<NodeType, ActorType, NetNode> netNodeGetter;

        public static void SetGetter(Func<NodeType, ActorType, NetNode> netNodeGetter, Func<string, Type> typeGetter)
        {
            nodeAARCSMap.Clear();
            ActorRemoteCallHelper.typeGetter = typeGetter;
            ActorRemoteCallHelper.netNodeGetter = netNodeGetter;
        }

        static IActorAgentRemoteCallService GetCallAgent(NodeType targetNodeType, ActorType targetActorType)
        {
            var node = netNodeGetter(targetNodeType, targetActorType);
            if (node != null)
            {
                lock (nodeAARCSMap)
                {
                    if (nodeAARCSMap.TryGetValue(node.NodeId, out var seragent))
                    {
                        var rpcUrl = seragent.Key;
                        //如果服务节点改变了配置
                        if (node.RpcUrl == rpcUrl)
                        {
                            return seragent.Value;
                        }
                        else
                        {
                            nodeAARCSMap.TryRemove(node.NodeId, out _);
                        }
                    }
                    var channel = GrpcChannel.ForAddress(node.RpcUrl);
                    var client = MagicOnionClient.Create<IActorAgentRemoteCallService>(channel);
                    nodeAARCSMap.TryAdd(node.NodeId, new KeyValuePair<string, IActorAgentRemoteCallService>(node.RpcUrl, client));
                    return client;
                }
            }
            else
            {
                return null;
            }
        }

        public static async Task Call(ActorType targetType, ActorRemoteCallParams param)
        {
            var serviceAgent = GetCallAgent(NodeType.Game, targetType);
            if (serviceAgent != null)
                await serviceAgent.Call(param);
        }

        public static async Task<TResult> Call<TResult>(ActorType targetType, ActorRemoteCallParams param)
        {
            var serviceAgent = GetCallAgent(NodeType.Game, targetType);
            if (serviceAgent != null)
            {
                var ret = await serviceAgent.Call(param);
                if (ret != null && ret.success)
                {
                    return MessagePack.MessagePackSerializer.Deserialize<TResult>(ret.resultData);
                }
            }
            return default(TResult);
        }
    }
}
