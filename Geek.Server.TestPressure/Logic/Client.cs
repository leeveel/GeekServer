using Geek.Server.Core.Net;
using Geek.Server.Core.Net.Kcp;
using Geek.Server.Core.Net.Tcp;
using Geek.Server.Core.Utils;
using Geek.Server.TestPressure.Net;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

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
        KcpChannel netChannel;
        readonly MsgWaiter msgWaiter = new();
        int msgUniId = 200;

        public Client(long id)
        {
            this.id = id;
        }

        public async Task Start(string ip, int port)
        {
            var serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            var serverId = TestSettings.Ins.serverId;
            IKcpSocket clientSocket = null;

            long netId = 0;
            KcpChannel kcpChannel = null;
            async Task Connect(int delay = 0)
            {
                if (delay > 0)
                {
                    await Task.Delay(delay);
                }
                LOGGER.Info("开始连接服务器...TODO...选择网关...");
                //clientSocket = new KcpUdpClientSocket(serverId);
                clientSocket = new KcpUdpClientSocket(serverId);
                if (!await clientSocket.Connect(ip, port, netId))
                {
                    LOGGER.Error("连接服务器失败...");
                    await Connect(100);
                    return;
                }

                if (kcpChannel == null)
                {
                    netId = clientSocket.NetId;
                    kcpChannel = new KcpChannel(false, netId, serverId, serverEndPoint, (chann, data) =>
                    {
                        var package = new TempNetPackage(NetPackageFlag.MSG, chann.NetId, serverId, data);
                        clientSocket?.Send(package);
                    }, async (chann, msg) =>
                    {
                        await OnRevice(msg);
                    });
                    netChannel = kcpChannel;
                }

                _ = clientSocket.StartRecv(kcpChannel.HandleRecv, () =>
                 {
                     _ = Connect(100);
                 }, () =>
                 {
                     LOGGER.Error("服务器断开连接....");
                 });
            }
            await Connect();


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
            netChannel?.Update(DateTime.UtcNow);
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
                SdkToken = "555sdasfda"+id,
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
            netChannel.Write(msg);
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
