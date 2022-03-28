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

        public static void InitMsg(Type assemblyType)
        {
            foreach (var type in assemblyType.Assembly.GetTypes())
            {
                if (!type.IsSubclassOf(typeof(BaseMessage))) continue;

                var msgIdField = type.GetField("SID", BindingFlags.Static | BindingFlags.Public);
                if (msgIdField == null) continue;

                int msgId = (int)msgIdField.GetValue(null);
                if (MsgDic.ContainsKey(msgId))
                    LOGGER.Error($"重复的消息 msgId：{msgId}");
                else
                    MsgDic.Add(msgId, type);
            }
        }

        public static IMessage GetMsg(int msgId)
        {
            if (!MsgDic.ContainsKey(msgId))
            {
                LOGGER.Error($"未注册的消息ID:{msgId}");
                return null;
            }
            Type msgType = MsgDic[msgId];
            var msg = Activator.CreateInstance(msgType) as IMessage;
            if (msg == null)
                LOGGER.Error($"创建msg失败 msgId:{msgId} msgTy[e:{msgType}");
            return msg;
        }
    }
}
