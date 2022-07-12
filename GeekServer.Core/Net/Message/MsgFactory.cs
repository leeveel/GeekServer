using NLog;
using System;
using System.Collections.Generic;
using System.Reflection;
namespace Geek.Server
{
    public class MsgFactory
    {

        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private static Dictionary<int, Type> MsgDic = new Dictionary<int, Type>();

        public const string KEY = "MsgID";

        public static void InitMsg(Type assemblyType)
        {
            foreach (var type in assemblyType.Assembly.GetTypes())
            {
                if (!type.IsSubclassOf(typeof(BaseMessage))) continue;

                var msgIdField = type.GetField(KEY, BindingFlags.Static | BindingFlags.Public);
                if (msgIdField == null) continue;

                int msgId = (int)msgIdField.GetValue(null);
                if (MsgDic.ContainsKey(msgId))
                    LOGGER.Error($"重复的消息 msgId：{msgId}");
                else
                    MsgDic.Add(msgId, type);
            }
        }

        public static BaseMessage GetMsg(int msgId)
        {
            if (!MsgDic.ContainsKey(msgId))
            {
                LOGGER.Error($"未注册的消息ID:{msgId}");
                return null;
            }
            Type msgType = MsgDic[msgId];
            var msg = Activator.CreateInstance(msgType) as BaseMessage;
            if (msg == null)
                LOGGER.Error($"创建msg失败 msgId:{msgId} msgTy[e:{msgType}");
            return msg;
        }

        public static Type GetMsgType(int msgId)
        {
            if (MsgDic.TryGetValue(msgId, out var msg))
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
