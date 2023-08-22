using Geek.Server.Core.Net.Kcp;

namespace Geek.Server.TestPressure.Net
{
    public delegate void OnReceiveNetPackFunc(ReadOnlySpan<byte> data);
    public interface IKcpSocket
    {
        public Task<bool> Connect(string ip, int port, long netId = 0);
        public long NetId { get; set; }
        public int ServerId { get; set; }
        public void Send(TempNetPackage data);
        public void Close();
        public Task StartRecv(OnReceiveNetPackFunc onRecv, Action onGateClose, Action onServerClose);
    }
}
