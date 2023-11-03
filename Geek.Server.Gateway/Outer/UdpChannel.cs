using Geek.Server.Core.Net;
using Geek.Server.Core.Net.Kcp;
using System.Net;

namespace Geek.Server.Gateway.Outer
{
    internal class UdpChannel : BaseNetChannel
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        public override string RemoteAddress => "udp->"+ remoteEndPoint.ToString();
        public IPEndPoint remoteEndPoint;
        UdpServer udpServer;
        bool isClose;

        public UdpChannel(long netId, int targetNetId, UdpServer userver, EndPoint address)
        {
            NetId = netId;
            TargetServerId = targetNetId;
            udpServer = userver;
            UpdateRemoteAddress(address);
            UpdateRecvMessageTime(TimeSpan.FromSeconds(5).Ticks);
        }

        public void UpdateRemoteAddress(EndPoint address)
        {
            isClose = false;
            var ipep = address as IPEndPoint;
            remoteEndPoint = new IPEndPoint(ipep.Address, ipep.Port);
            UpdateRecvMessageTime();
        }

        public override void Close()
        {
            isClose = true;
        }

        public override bool IsClose()
        {
            return isClose;
        }

        public override void Write(TempNetPackage msg)
        {
            if (!isClose)
            {
                udpServer.SendTo(msg, remoteEndPoint);
            }
        }
    }
}
