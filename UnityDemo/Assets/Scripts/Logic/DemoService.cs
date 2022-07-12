using Geek.Client;
using Geek.Server;
using Geek.Server.Proto;
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
            GameClient.Singleton.Send(msg);
            return MsgWaiter.StartWait(msg.UniId);
        }

        protected T GetCurMsg<T>(int msgId) where T : BaseMessage, new()
        {
            var rMsg = GameClient.Singleton.GetCurMsg();
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
            return rMsg as T;
        }

        public void RegisterEventListener()
        {
            AddListener(GameClient.ConnectEvt, OnConnectServer);
            AddListener(GameClient.DisconnectEvt, OnDisconnectServer);
            AddListener(ResLogin.MsgID, OnResLogin); 
            AddListener(ResBagInfo.MsgID, OnResBagInfo);
            AddListener(ResErrorCode.MsgID, OnResErrorCode);
        }

        private void OnResErrorCode(Event e)
        {
            ResErrorCode res = GetCurMsg<ResErrorCode>(e.EventId);
            switch (res.ErrCode)
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
            MsgWaiter.EndWait(res.UniId, res.ErrCode == (int)ErrCode.Success);
            if (!string.IsNullOrEmpty(res.Desc))
                UnityEngine.Debug.Log("服务器提示:" + res.Desc);
        }


        private void OnConnectServer(Event e)
        {
            UnityEngine.Debug.Log("-------OnConnectServer-->>>" + (NetCode)e.Data);
            int code = (int)e.Data;
            if ((NetCode)code == NetCode.Success)
            {
                UnityEngine.Debug.Log("连接服务器成功!");
                MsgWaiter.EndWait(GameClient.ConnectEvt);
            }
            else
            {
                UnityEngine.Debug.Log("连接服务器失败!");
                MsgWaiter.EndWait(GameClient.ConnectEvt, false);
            }
        }

        private void OnDisconnectServer(Event e)
        {
            UnityEngine.Debug.Log("与服务器断开!");
        }

        private void OnResLogin(Event e)
        {
            var res = GetCurMsg<ResLogin>(e.EventId);
            UnityEngine.Debug.Log($"{res.UserInfo.RoleName}:登录成功!");
        }

        private void OnResBagInfo(Event e)
        {
            var msg = GetCurMsg<ResBagInfo>(e.EventId);
            var data = msg.ItemDic;
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
