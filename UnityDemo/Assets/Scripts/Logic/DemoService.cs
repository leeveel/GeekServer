using Geek.Client;
using Geek.Client.Message.Bag;
using Geek.Client.Message.Login;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Logic
{

    /// <summary>
    /// 服务器错误码
    /// </summary>
    public enum ErrCode
    {
        Success = 0,
        ConfigErr = 400, //配置表错误
        ParamErr, //客户端传递参数错误
        CostNotEnough, //消耗不足

        Notice = 100000, //正常通知
        FuncNotOpen, //功能未开启，主消息屏蔽
        Other //其他
    }


    public class DemoService : NetEventComp
    {

        protected static DemoService mSingleton = null;

        public static DemoService Singleton
        {
            get
            {
                if (mSingleton == null)
                    mSingleton = new DemoService();
                return mSingleton;
            }
        }
        public static int UniId { private set; get; } = 200;


        public Task<bool> SendMsg(BaseMessage msg)
        {
            msg.UniId = UniId++;
            MessageHandle.GetInstance().Send(msg);
            return MsgWaiter.StartWait(msg.UniId);
        }

        protected T GetCurMsg<T>(int msgId) where T : BaseMessage, new()
        {
            var rMsg = MessageHandle.GetInstance().GetCurMsg();
            if (rMsg == null)
                return null;
            if (rMsg.MsgId != msgId)
            {
                UnityEngine.Debug.LogErrorFormat("获取网络消息失败, mine:{0}   cur:{1}", msgId, rMsg.MsgId);
                return null;
            }

#if UNITY_EDITOR
            UnityEngine.Debug.Log("deal msg:" + msgId + ">" + typeof(T));
#endif
            //已经提前解析好了
            if (rMsg.Msg != null)
                return rMsg.Msg as T;

            T msg = new T();
            int offset = 0;
            offset = msg.Read(rMsg.ByteContent, offset);
            return msg;
        }

        public void RegisterEventListener()
        {
            AddListener(MessageHandle.ConnectSucceedEvt, OnConnectServer);
            AddListener(MessageHandle.DisconnectEvt, OnDisconnectServer);
            AddListener(ResLogin.MsgId, OnResLogin); 
            AddListener(ResBagInfo.MsgId, OnResBagInfo);
            AddListener(ResErrorCode.MsgId, OnResErrorCode);
        }

        private void OnResErrorCode(Event e)
        {
            ResErrorCode res = GetCurMsg<ResErrorCode>(e.EventId);
            switch (res.errCode)
            {
                case (int)ErrCode.Success:
                    //do some thing
                    break;
                case (int)ErrCode.ConfigErr:
                    //do some thing
                    break;
                //case ....
                default:
                    break;
            }
            MsgWaiter.EndWait(res.UniId, res.errCode == (int)ErrCode.Success);
            if (!string.IsNullOrEmpty(res.desc))
                UnityEngine.Debug.Log("服务器提示:" + res.desc);
        }


        private void OnConnectServer(Event e)
        {
            UnityEngine.Debug.Log("-------OnConnectServer-->>>" + (NetCode)e.Data);
            int code = (int)e.Data;
            if ((NetCode)code == NetCode.Success)
            {
                UnityEngine.Debug.Log("连接服务器成功!");
                MsgWaiter.EndWait(MessageHandle.ConnectSucceedEvt);
            }
            else
            {
                UnityEngine.Debug.Log("连接服务器失败!");
                MsgWaiter.EndWait(MessageHandle.ConnectSucceedEvt, false);
            }
        }

        private void OnDisconnectServer(Event e)
        {
            UnityEngine.Debug.Log("与服务器断开!");
        }

        private void OnResLogin(Event e)
        {
            var res = GetCurMsg<ResLogin>(e.EventId);
            UnityEngine.Debug.Log($"{res.userInfo.roleName}:登录成功!");
        }

        private void OnResBagInfo(Event e)
        {
            var msg = GetCurMsg<ResBagInfo>(e.EventId);
            var data = msg.itemDic;
            StringBuilder str = new StringBuilder();
            str.Append("收到背包数据:");
            foreach (KeyValuePair<int, long> keyVal in data)
            {
                str.Append($"{keyVal.Key}:{keyVal.Value},");
            }
            UnityEngine.Debug.Log(str);
        }

    }
}
