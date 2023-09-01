using Geek.Server.Core.Net.Kcp;
using Geek.Server.TestPressure.Net;
using System.Diagnostics;
using System.Net;
using static System.Net.Mime.MediaTypeNames;
using System.Threading.Channels;

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
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        readonly long id;
        private KcpChannel channel { get; set; }
        AKcpSocket clientSocket = null;
        readonly MsgWaiter msgWaiter = new();
        int msgUniId = 200;

        public Client(long id)
        {
            this.id = id;
        }

        async Task<bool> Connect(string ip, int port, int serverId)
        {
            long netId = 0;
            KcpChannel kcpChannel = null;
            async Task<bool> Connect(int delay = 0)
            {
                if (delay > 0)
                {
                    await Task.Delay(delay);
                }
                //Debug.Log("选择网关...");
                //var index = UnityEngine.Random.Range(0, gateList.serverIps.Count);
                //var ip = gateList.serverIps[index];
                //var port = gateList.ports[index];

                clientSocket = null;

                var serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

                //clientSocket = new KcpTcpClientSocket(serverId);
                clientSocket = new KcpUdpClientSocket(serverId);
#if UNITY_EDITOR
                Debug.Log($"当前准备连接网关类:{clientSocket.GetType().Name}");
#endif

                var result = await clientSocket.Connect(ip, port, netId);
                if (result.resetNetId)
                {
                    netId = 0;
                }

                int reconnectDelay = 900;
                if (!result.isSuccess)
                {
                    if (result.allowReconnect)
                    {
                        LOGGER.Error("连接服务器失败...");
                        //TODO:限制连接次数
                        await Connect(reconnectDelay);
                    }
                    else
                    {
                        OnDisConnected();
                    }
                    return false;
                }
                if (kcpChannel == null)
                {
                    netId = clientSocket.NetId;
                    kcpChannel = new KcpChannel(false, netId, serverId, serverEndPoint, (chann, data) =>
                    {
                        var package = new TempNetPackage(NetPackageFlag.MSG, chann.NetId, serverId, data);
                        clientSocket?.Send(package);
                    }, (chann, msg) =>
                    {
                        return OnRevice(msg);
                    });
                    channel = kcpChannel;
                }

                _ = clientSocket.StartRecv(kcpChannel.HandleRecv, () =>
                {
                    if ((bool)!kcpChannel?.IsClose())
                        _ = Connect(reconnectDelay);
                }, () =>
                {
                    LOGGER.Error("服务器断开连接....");
                    OnDisConnected();
                });
                return true;
            }
            return await Connect();
        }

        public async Task Start(string ip, int port)
        {
            var serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            var serverId = TestSettings.Ins.serverId;

            await Connect(ip, port, serverId);

            if (!await ReqLogin())
            {
                return;
            }

            //   foreach (var i in Enumerable.Range(0, 10))
            {
                if (!await ReqBagInfo())
                {
                    return;
                }
            }
            foreach (var i in Enumerable.Range(0, 30222))
            {
                await ReqComposePet();
                await Task.Delay(500);
            }
        }

        public void Update()
        {
            channel?.Update(DateTime.UtcNow);
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
                SdkToken = "555sdasfda" + id,
                UserName = "name" + id,
                Device = "test device",
                Platform = "android",
                Sign = "test device"
            };

            return SendMsgAndWaitBack(req);
        }

        private Task<bool> ReqBagInfo()
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
            //LOGGER.Info($"{id} 发送消息:{msg.GetType().Name},{JsonConvert.SerializeObject(msg)}");
            channel.Write(msg);
        }

        async Task<bool> SendMsgAndWaitBack(Message msg)
        {
            SendMsg(msg);
            return await msgWaiter.StartWait(msg.UniId);
        }

        public void OnDisConnected()
        {
            LOGGER.Info($"客户端[{id}]断开");
        }

        public Task OnRevice(Message msg)
        {
            //LOGGER.Info($"{id} 收到消息:{msg.GetType().Name},{JsonConvert.SerializeObject(msg)}");
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
                    LOGGER.Info("服务器提示:" + errMsg.Desc);
            }

            if (msg.MsgId == ResConnectGate.MsgID)
            {
                msgWaiter.EndWait(msg.UniId, (msg as ResConnectGate).Result);
            }
            return Task.CompletedTask;
        }
    }
}
