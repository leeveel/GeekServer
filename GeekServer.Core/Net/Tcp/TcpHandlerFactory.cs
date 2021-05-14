using System;
using NLog;
using System.Collections.Generic;
using System.Reflection;

namespace Geek.Server
{
    public class TcpHandlerFactory
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        static readonly Dictionary<int, Type> msgMap = new Dictionary<int, Type>();
        static readonly Dictionary<int, Type> handlerMap = new Dictionary<int, Type>();

        static Func<int, IMessage> extraMsgGetter;
        static Func<int, BaseTcpHandler> extraHandlerGetter;
        public static void SetExtraHandlerGetter(Func<int, IMessage> msgGetter, Func<int, BaseTcpHandler> handlerGetter)
        {
            extraMsgGetter = msgGetter;
            extraHandlerGetter = handlerGetter;
        }

        /// <summary>消息初始化</summary>
        public static void InitHandler(Type assemblyType)
        {
            if(assemblyType != null)
                InitHandler(assemblyType.Assembly);
        }

        /// <summary>消息初始化</summary>
        public static void InitHandler(Assembly assembly)
        {
            msgMap.Clear();
            handlerMap.Clear();
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                var att = (MsgMapping)type.GetCustomAttribute(typeof(MsgMapping), true);
                if (att == null)
                    continue;
                var msgIdField = att.Msg.GetField("MsgId", BindingFlags.Static | BindingFlags.Public);
                if (msgIdField == null)
                    continue;

                int msgId = (int)msgIdField.GetValue(null);
                if (!msgMap.ContainsKey(msgId))
                {
                    handlerMap.Add(msgId, type);
                    msgMap.Add(msgId, att.Msg);
                }
                else
                {
                    LOGGER.Error("重复注册消息Handler:[{}] msg:[{}]", msgId, type);
                }
            }
        }

        /// <summary>获取消息Handler</summary>
        public static BaseTcpHandler GetHandler(int msgId)
        {
            if(extraHandlerGetter != null)
            {
                var extraHandler = extraHandlerGetter(msgId);
                if (extraHandler != null)
                    return extraHandler;
            }

            if (!handlerMap.ContainsKey(msgId))
            {
                LOGGER.Error("未注册的消息ID:{}", msgId);
                return null;
            }

            Type handlerType = handlerMap[msgId];
            var handler = Activator.CreateInstance(handlerType) as BaseTcpHandler;
            if (handler == null)
                LOGGER.Error("创建handler失败:{}", handlerType.ToString());
            return handler;
        }

        /// <summary> 获取消息</summary>
        public static IMessage GetMsg(int msgId)
        {
            if (extraMsgGetter != null)
            {
                var extraMsg = extraMsgGetter(msgId);
                if (extraMsg != null)
                    return extraMsg;
            }

            if (!msgMap.ContainsKey(msgId))
            {
                LOGGER.Error("未注册的消息ID:{}", msgId);
                return null;
            }

            Type msgType = msgMap[msgId];
            var msg = Activator.CreateInstance(msgType) as IMessage;
            if (msg == null)
                LOGGER.Error("创建Msg失败:{}", msgType.ToString());
            return msg;
        }
    }
}