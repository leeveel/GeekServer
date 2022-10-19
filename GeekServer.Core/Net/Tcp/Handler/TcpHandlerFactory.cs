using System;
using NLog;
using System.Collections.Generic;
using System.Reflection;

namespace Geek.Server
{
    public class TcpHandlerFactory
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        public const string KEY = "MsgID";
        static readonly Dictionary<int, Type> msgMap = new Dictionary<int, Type>();
        static readonly Dictionary<int, Type> handlerMap = new Dictionary<int, Type>();

        static Func<int, Message> msgGetter;
        static Func<int, Type> msgTypeGetter;
        static Func<int, BaseTcpHandler> handlerGetter;
        public static void SetHandlerGetter(Func<int, Message> msgGetter, Func<int, BaseTcpHandler> handlerGetter)
        {
            TcpHandlerFactory.msgGetter = msgGetter;
            TcpHandlerFactory.handlerGetter = handlerGetter;
        }

        public static void SetHandlerGetter(Func<int, Type> msgTypeGetter, Func<int, BaseTcpHandler> handlerGetter)
        {
            TcpHandlerFactory.msgTypeGetter = msgTypeGetter;
            TcpHandlerFactory.handlerGetter = handlerGetter;
        }

        /// <summary>
        /// 仅供非热更模式调用
        /// </summary>
        /// <param name="assemblyType"></param>
        public static void InitHandler(Type assemblyType)
        {
            if (assemblyType != null)
                InitHandler(assemblyType.Assembly);
        }

        /// <summary>消息初始化</summary>
        private static void InitHandler(Assembly assembly)
        {
            msgMap.Clear();
            handlerMap.Clear();
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                var att = (MsgMapping)type.GetCustomAttribute(typeof(MsgMapping), true);
                if (att == null)
                    continue;
                var msgIdField = att.Msg.GetField(KEY, BindingFlags.Static | BindingFlags.Public);
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
            if (handlerGetter != null)
            {
                var extraHandler = handlerGetter(msgId);
                if (extraHandler != null)
                    return extraHandler;
            }

            if (!handlerMap.ContainsKey(msgId))
            {
                return null;
            }

            Type handlerType = handlerMap[msgId];
            var handler = Activator.CreateInstance(handlerType) as BaseTcpHandler;
            if (handler == null)
                LOGGER.Error("创建handler失败:{}", handlerType.ToString());
            return handler;
        }

        /// <summary> 获取消息</summary>
        public static Message GetMsg(int msgId)
        {
            if (msgGetter != null)
            {
                var extraMsg = msgGetter(msgId);
                if (extraMsg != null)
                    return extraMsg;
            }

            if (!msgMap.ContainsKey(msgId))
            {
                LOGGER.Error("未注册的消息ID:{}", msgId);
                return null;
            }

            Type msgType = msgMap[msgId];
            var msg = Activator.CreateInstance(msgType) as Message;
            if (msg == null)
                LOGGER.Error("创建Msg失败:{}", msgType.ToString());
            return msg;
        }


        public static Type GetMsgType(int msgId)
        {
            if (msgTypeGetter != null)
            {
                var extraMsg = msgTypeGetter(msgId);
                if (extraMsg != null)
                    return extraMsg;
            }

            if (msgMap.TryGetValue(msgId, out var msg))
            {
                return msg;
            }
            else
            {
                LOGGER.Error("未注册的消息ID:{}", msgId);
                return null;
            }
        }

    }
}