using Geek.Server.Core.Net.Kcp;
using Geek.Server.TestPressure.Logic;
using System.Buffers.Binary;

public delegate void OnReceiveNetPackFunc(ReadOnlySpan<byte> data);

namespace Geek.Server.TestPressure.Net
{
    public class ConnectResult
    {
        public bool isSuccess;
        public bool allowReconnect;
        public bool resetNetId;

        public ConnectResult()
        {

        }
        public ConnectResult(bool success, bool allowRecon, bool resetNid)
        {
            isSuccess = success;
            allowReconnect = allowRecon;
            resetNetId = resetNid;
        }
    }

    public abstract class AKcpSocket
    {
        public static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        public virtual Task<ConnectResult> Connect(string ip, int port, long netId = 0) => throw new NotImplementedException();
        public long NetId { get; set; }
        public int ServerId { get; set; }
        protected bool isConnecting = true;
        protected int immediatelyHeartId = int.MinValue;
        protected CancellationTokenSource cancelSrc = new();

        protected void EndWaitHeartId(int id)
        {
        }

        public virtual void Send(TempNetPackage data) { }
        public virtual void Close()
        {
            EndWaitHeartId(immediatelyHeartId);
            cancelSrc?.Cancel();
        }
        public virtual bool IsClose() { return false; }
        public virtual Task StartRecv(OnReceiveNetPackFunc onRecv, Action onGateClose, Action onServerClose) => Task.CompletedTask;
        public virtual async ValueTask<bool> HeartCheckImmediate(MsgWaiter msgWaiter = null)
        {
            return await ValueTask.FromResult(true);
        }

        protected async Task StartGateHeartAsync()
        {
            while (!cancelSrc.IsCancellationRequested)
            {
                try
                {
                    Send(new TempNetPackage(NetPackageFlag.HEART, NetId, ServerId));
                    await Task.Delay(TimeSpan.FromSeconds(3), cancelSrc.Token);
                }
                catch
                {
                    break;
                }
            }
        }
    }
}
