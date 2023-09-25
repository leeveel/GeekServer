using Base.Net;
using ClientProto;
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


        public Task<bool> SendMsg(Message msg)
        {
            msg.UniId = UniId++;
            GameClient.Singleton.Send(msg);
            UnityEngine.Debug.Log("开始等待消息:" + msg.UniId);
            return MsgWaiter.StartWait(msg.UniId);
        }

        protected T GetCurMsg<T>(object msg) where T : Message, new()
        {
            return msg as T;
        }

        public void RegisterEventListener()
        {
            AddListener(NetDisConnectMessage.MsgID, OnDisconnectServer);
            AddListener(ResLogin.MsgID, OnResLogin);
            AddListener(ResBagInfo.MsgID, OnResBagInfo);
            AddListener(ResComposePet.MsgID, OnResComposePet);
            AddListener(ResErrorCode.MsgID, OnResErrorCode);
        }

        private void OnResErrorCode(Event e)
        {
            ResErrorCode res = GetCurMsg<ResErrorCode>(e.Data);
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


        private void OnDisconnectServer(Event e)
        {
            UnityEngine.Debug.Log("与服务器断开!");
        }

        private void OnResLogin(Event e)
        {
            var res = GetCurMsg<ResLogin>(e.Data);
            UnityEngine.Debug.Log($"{res.UserInfo.RoleName}:登录成功!");
            GameMain.Singleton.AppendLog($"{res.UserInfo.RoleName}:登录成功!");
        }

        private void OnResBagInfo(Event e)
        {
            var msg = GetCurMsg<ResBagInfo>(e.Data);
            var data = msg.ItemDic;
            StringBuilder str = new StringBuilder();
            str.Append("收到背包数据:");
            foreach (KeyValuePair<int, long> keyVal in data)
            {
                str.Append($"{keyVal.Key}:{keyVal.Value},");
            }
            UnityEngine.Debug.Log(str);
            GameMain.Singleton.AppendLog(str.ToString());
        }

        private void OnResComposePet(Event e)
        {
            var msg = GetCurMsg<ResComposePet>(e.Data);
            var str = $"合成宠物成功{msg.PetId}";
            UnityEngine.Debug.Log(str);
        }

    }
}
