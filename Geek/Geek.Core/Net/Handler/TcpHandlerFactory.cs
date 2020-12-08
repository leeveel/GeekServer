/****************************************************************************
Copyright (c) Geek

https://github.com/leeveel/GeekServer

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
****************************************************************************/
using System;
using NLog;
using System.Collections.Generic;
using System.Reflection;
using Geek.Core.Net.Message;

namespace Geek.Core.Net.Handler
{
    public class TcpHandlerFactory
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

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
                var att = (TcpMsgMapping)type.GetCustomAttribute(typeof(TcpMsgMapping), true);
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