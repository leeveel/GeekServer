using Newtonsoft.Json;

namespace Geek.Server.TestPressure.Logic
{
    public enum ServerErrorCode
    {
        Success = 0,
        ConfigErr = 400, //配置表错误
        ParamErr, //客户端传递参数错误
        CostNotEnough, //消耗不足

        Notice = 100000, //正常通知
        FuncNotOpen, //功能未开启，主消息屏蔽
        Other //其他
    }

    public class Client
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        long id;
        ClientNetChannel netChannel;
        MsgWaiter msgWaiter = new();
        int msgUniId = 200;

        public Client(long id)
        {
            this.id = id;
        }

        public async void Start()
        {
            netChannel = new ClientNetChannel(this);
            var connCode = await netChannel.Connect(TestSettings.Ins.serverIp, TestSettings.Ins.serverPort);
            Log.Info($"客户端[{id}]链接:{connCode}");
            if (connCode == NetCode.Success)
            {
                await ReqLogin();
            }
            else
            {
                return;
            }
            for (int i = 0; i < 1; i++)
            {
                await ReqBagInfo();
                await Task.Delay(1000);
            }
            await ReqComposePet();
        }

        private Task<bool> ReqLogin()
        {
            //登陆
            var req = new ReqLogin();
            req.SdkType = 0;
            req.SdkToken = "555";
            req.UserName = "name" + id;
            req.Device = new Random().NextInt64().ToString();
            req.Platform = "android";
            return SendMsgAndWaitBack(req);
        }

        private Task ReqBagInfo()
        {
            return SendMsgAndWaitBack(new ReqBagInfo());
        }

        private Task ReqComposePet()
        {
            return SendMsgAndWaitBack(new ReqComposePet() { FragmentId=1000 });
        }

        void SendMsg(Message msg)
        {
            msg.UniId = msgUniId++;
            Log.Info($"{id} 发送消息:{JsonConvert.SerializeObject(msg)}");
            netChannel.Write(new NMessage(msg));
        }

        Task<bool> SendMsgAndWaitBack(Message msg)
        {
            SendMsg(msg);
            return msgWaiter.StartWait(msg.UniId);
        }

        public void OnDisConnected()
        {

        }

        public void OnConnected(NetCode code)
        {
            Log.Info($"客户端[{id}]链接:{code}");
        }

        public void OnRevice(Message msg)
        {
            //Log.Error($"收到消息:{msg.MsgId} {MsgFactory.GetType(msg.MsgId)}");

            if (msg.MsgId == ResErrorCode.MsgID)
            {
                ResErrorCode errMsg = (ResErrorCode)msg;
                switch (errMsg.ErrCode)
                {
                    case (int)ServerErrorCode.Success:
                        //do some thing
                        break;
                    case (int)ServerErrorCode.ConfigErr:
                        //do some thing
                        break;
                    //case ....
                    default:
                        break;
                }
                msgWaiter.EndWait(errMsg.UniId, errMsg.ErrCode == (int)ServerErrorCode.Success);
                if (!string.IsNullOrEmpty(errMsg.Desc))
                    Log.Info("服务器提示:" + errMsg.Desc);
            }
            else
            {

                Log.Info($"{id} 收到消息:{JsonConvert.SerializeObject(msg)}");
            }
        }
    }
}
