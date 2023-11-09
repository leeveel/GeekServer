using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Geek.Server.Core.Utils;
using NLog;

namespace Geek.Server.Core.Net.Kcp
{
    //docker 内部发送 需要用localIP 否则172.17.0.1可能丢包
    public class UdpServer
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        public Socket socket;
        public delegate void ReceiveFunc(TempNetPackage package, EndPoint point);
        ReceiveFunc onRecv;
        int innerId = 0;
        bool isInnerServer => innerId != 0;

        Func<int, EndPoint> getEndPointById;
        public UdpServer(int port, ReceiveFunc onRecv, int innerId = 0, Func<int, EndPoint> getEndPointById = null)
        {
            this.getEndPointById = getEndPointById;
            this.innerId = innerId;
            this.onRecv = onRecv;
            socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp)
            {
                DualMode = true,
                EnableBroadcast = false
            };
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                socket.SendBufferSize = 1024 * 1024 * 40;
                socket.ReceiveBufferSize = 1024 * 1024 * 40;
            }
            try
            {
                socket.Bind(new IPEndPoint(IPAddress.Any, port));
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    uint IOC_IN = 0x80000000;
                    uint IOC_VENDOR = 0x18000000;
                    int SIO_UDP_CONNRESET = unchecked((int)(IOC_IN | IOC_VENDOR | 12));
                    socket.IOControl(SIO_UDP_CONNRESET, new[] { Convert.ToByte(false) }, null);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"udp bind error: {port}", e);
            }
        }

        public Task Start(int parallel = 1)
        {
            parallel = Math.Max(parallel, 1);

            List<Task> tasks = new List<Task>();
            for (int i = 0; i < parallel; i++)
            {
                tasks.Add(Task.Factory.StartNew((o) =>
                {
                    byte[] cache = new byte[2048];
                    EndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    while (socket != null)
                    {
                        try
                        {
                            int len = socket.ReceiveFrom(cache, ref ipEndPoint);
                            if (isInnerServer)
                            {
                                ipEndPoint = getEndPointById?.Invoke(cache.ReadInt(0)) ?? ipEndPoint;
                            }
                            //LOGGER.Info($"收到udp数据...{ipEndPoint} {len}");
                            if (onRecv != null)
                            {
                                var offset = isInnerServer ? 4 : 0;
                                var package = new TempNetPackage(cache.AsSpan(offset, len - offset));
                                if (package.isOk)
                                {
                                    //if (package.flag != NetPackageFlag.HEART)
                                    //{
                                    //    LOGGER.Info($"收到包...{package.ToString()}");
                                    //}
                                    onRecv(package, ipEndPoint);
                                }
                                else
                                {
                                    LOGGER.Error($"错误的udp package...{ipEndPoint}");
                                }
                            }
                        }
                        catch (Exception e)
                        {
#if DEBUG
                            LOGGER.Warn(e);
#endif
                        }
                    }
                    LOGGER.Warn($"退出udp接收线程...{i}");
                }, TaskCreationOptions.LongRunning));
            }
            return Task.WhenAll(tasks);
        }

        public void SendTo(TempNetPackage package, EndPoint point)
        {
            var len = package.Length + (innerId == 0 ? 0 : 4);
            Span<byte> target = stackalloc byte[len];
            int offset = 0;
            if (isInnerServer)
            {
                target.Write(innerId, ref offset);
            }
            target.Write(package, ref offset);
            try
            {
                socket?.SendTo(target, point);

                //if (package.flag != NetPackageFlag.HEART)
                //{
                //    LOGGER.Info($"发送包...{package.ToString()} {point}");
                //}
            }
            catch
            {

            }
        }

        public void SendTo(ReadOnlySpan<byte> span, EndPoint point)
        {
            try
            {
                if (isInnerServer)
                {
                    int offset = 0;
                    Span<byte> target = stackalloc byte[span.Length + 4];
                    target.Write(innerId, ref offset);
                    span.CopyTo(target[offset..]);
                    socket?.SendTo(target, point);
                }
                else
                    socket?.SendTo(span, point);
            }
            catch
            {

            }
        }

        public void Close()
        {
            socket?.Close();
            socket = null;
        }
    }
}
