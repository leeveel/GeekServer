using Geek.Server.Core.Net.Tcp;
using Newtonsoft.Json;
using System.Net.Sockets;

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
        readonly long id;
        NetChannel netChannel;
        readonly MsgWaiter msgWaiter = new();
        int msgUniId = 200;

        public Client(long id)
        {
            this.id = id;
        }

        public async Task Start(string ip, int port)
        {
            var context = await new SocketConnection(AddressFamily.InterNetwork, ip, port).StartAsync(10000);
            if (context != null)
            {
                Log.Info($"Connected to {context.LocalEndPoint}");
                netChannel = new NetChannel(context, new ClientProtocol(), OnRevice, OnDisConnected);
                _ = netChannel.StartReadMsgAsync();
            }
            else
            {
                Log.Error($"连接服务器失败...");
                return;
            }
            await ReqRouter();
            await ReqLogin();
            await Task.Delay(100);
            await ReqBagInfo();
            await Task.Delay(100);
            await ReqComposePet();
        }

        private Task<bool> ReqRouter()
        {
            var req = new ReqConnectGate
            {
                ServerId = TestSettings.Ins.serverId
            };
            return SendMsgAndWaitBack(req);
        }

        private Task<bool> ReqLogin()
        {
            //登陆
            var req = new ReqLogin
            {
                SdkType = 0,
                SdkToken = "555",
                UserName = "name" + id,
                Device = new Random().NextInt64().ToString(),
                Platform = "android"
            };
            return SendMsgAndWaitBack(req);
        }

        private Task ReqBagInfo()
        {
            return SendMsgAndWaitBack(new ReqBagInfo());
        }

        private Task ReqComposePet()
        {
            return SendMsgAndWaitBack(new ReqComposePet() { FragmentId = 1000 });
        }

        void SendMsg(Message msg)
        {
            msg.UniId = msgUniId++;
            Log.Info($"{id} 发送消息:{msg.GetType().Name},{JsonConvert.SerializeObject(msg)}");
            netChannel.Write(msg);
        }

        async Task<bool> SendMsgAndWaitBack(Message msg)
        {
            SendMsg(msg);
            return await msgWaiter.StartWait(msg.UniId);
        }

        public void OnDisConnected()
        {
            Log.Info($"客户端[{id}]断开");
        }

        public void OnRevice(NetMessage nmsg)
        {
            var msg = nmsg.Msg;
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
            Log.Info($"{id} 收到消息:{msg.GetType().Name},{JsonConvert.SerializeObject(msg)}");
        }
    }
}
