using Geek.Server.Core.Utils;
using NLog;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Geek.Server.Core.Net.Kcp
{
    public class UdpServer
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        public Socket socket;
        public delegate void ReceiveFunc(TempNetPackage package, EndPoint point);
        ReceiveFunc onRecv;
        public UdpServer(int port, ReceiveFunc onRecv)
        {
            this.onRecv = onRecv;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
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

        public Task Start()
        {
            return Task.Factory.StartNew((o) =>
            {
                if (socket == null)
                {
                    return;
                }
                byte[] cache = new byte[4096];
                EndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
                while (socket != null)
                {
                    int len = socket.ReceiveFrom(cache, ref ipEndPoint);
                    //LOGGER.Debug($"收到udp数据...{ipEndPoint} {len}");
                    if (onRecv != null)
                    {
                        var package = new TempNetPackage(cache.AsSpan(0, len));
                        if (package.isOk)
                        {
                            onRecv(package, ipEndPoint);
                        }
                        else
                        {
                            LOGGER.Error($"错误的udp package...{ipEndPoint}");
                        }
                    }
                }
            }, CancellationToken.None, TaskCreationOptions.LongRunning);
        }

        public void SendTo(TempNetPackage package, EndPoint point)
        {
            Span<byte> target = stackalloc byte[package.Length];
            int offset = 0;
            target.Write(package, ref offset);
            try
            {
                socket?.SendTo(target, point);
            }
            catch (Exception e)
            {

            }
        }

        public void SendTo(ReadOnlySpan<byte> span, EndPoint point)
        {
            try
            { 
                socket?.SendTo(span, point);
            }
            catch(Exception e)
            {

            }
        }

        public void Close()
        {
            socket.Close();
            socket = null;
        }
    }
}
