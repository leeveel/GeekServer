using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Geek.Client
{
    public enum NetCode
    {
        Unknown = 0,
        CreateTCPErr,
        ConnectErr,
        ConnectFalse,
        EncodeErr,
        DecodeErr,
        ReceiveErr,
        IsConnecting,
        IsConnected,

        TimeOut = 100,     //超时
        Success,     //连接成功
        Closed,     //断开连接
    }

    public class AsyncTCPSocket
    {
        private enum LogType
        {
            Err,
            Wrn,
            Log
        }

#if UNITY_IPHONE && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string getIPv6(string mHost, string mPort);
#endif

        //"192.168.1.1&&ipv4"
        private static string GetIPv6(string mHost, string mPort)
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            //需要自己实现获取IPV6的Native代码
		    string mIPv6 = getIPv6(mHost, mPort);
		    return mIPv6;
#else
            return mHost + "&&ipv4";
#endif
        }

        private string ip;
        private int port;
        private TcpClient client;
        private byte[] readBuffer = new byte[1024 * 8];
        private Action<NetCode> connectRetCb;
        private Action<NetCode> disconnectedCb;
        private Action<byte[], int> receiveCb;
        private OneParam<bool> readTag;
        readonly Actor actor = new Actor();

        bool createClient()
        {
            AddressFamily ipType = AddressFamily.InterNetwork;
            try
            {
                string ipv6 = GetIPv6(ip, port.ToString());
                if (!string.IsNullOrEmpty(ipv6))
                {
                    string[] tmp = System.Text.RegularExpressions.Regex.Split(ipv6, "&&");
                    if (tmp != null && tmp.Length >= 2)
                    {
                        string type = tmp[1];
                        if (type == "ipv6")
                        {
                            ip = tmp[0];
                            ipType = AddressFamily.InterNetworkV6;
                        }
                    }
                }
                client = new TcpClient(ipType);
            }
            catch (Exception e)
            {
                LogMsg(e.Message + "\n" + e.StackTrace);
                return false;
            }
            return true;
        }

        public void Connect(string ip, int port, Action<NetCode> onConnect, Action<NetCode> onDisconnect, Action<byte[], int> onReceive, int timeOut = 5000)
        {
            receiveCb = onReceive;
            connectRetCb = onConnect;
            disconnectedCb = onDisconnect;

            actor.SendAsync(async () => {
                this.ip = ip;
                this.port = port;
                LogMsg("请求连接服务器 " + ip + ":" + port, LogType.Log);
                
                try
                {
                    if (client != null)
                        client.Close();
                }
                catch (Exception e)
                {
                    LogMsg("close error", LogType.Log);
                    LogMsg(e.ToString());
                }

                if (!createClient())
                {
                    notifyConnectRet(NetCode.CreateTCPErr);
                    return;
                }
                
                try
                {
                    var task = client.ConnectAsync(ip, port);
                    var tokenSource = new CancellationTokenSource();
                    var completeTask = await Task.WhenAny(task, Task.Delay(timeOut, tokenSource.Token));
                    if (completeTask != task)
                    {
                        client.Close();
                        client.Dispose();
                        client = null;
                        notifyConnectRet(NetCode.TimeOut);
                        return;
                    }
                    else
                    {
                        tokenSource.Cancel();
                        await task;
                    }
                }
                catch(Exception e)
                {
                    if(client.Connected)
                        client.Close();
                    client = null;
                    LogMsg(e.ToString());
                    notifyConnectRet(NetCode.ConnectErr);
                    return;
                }
                if (readTag != null)
                    readTag.value = false;

                readTag = new OneParam<bool>(true);
                _ = readData(readTag);
                notifyConnectRet(NetCode.Success);
            });
        }

        async Task readData(OneParam<bool> flag)
        {
            while (flag.value)
            {
                await actor.SendAsync(async ()=> {
                    if (!flag.value)
                        return;

                    if (client == null || !client.Connected || client.Client == null)
                    {
                        LogMsg("接收消息时连接不存在");
                        flag.value = false;
                        notifyDisconnectRet(NetCode.ConnectFalse);
                        return;
                    }

                    //检测断线
                    //https://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c
                    bool part1 = client.Client.Poll(1000, SelectMode.SelectRead);
                    bool part2 = client.Client.Available == 0;
                    if (part1 && part2)
                    {
                        LogMsg("接收消息时连接不存在");
                        flag.value = false;
                        notifyDisconnectRet(NetCode.ConnectFalse);
                        return;
                    }

                    if (client.GetStream().DataAvailable)
                    {
                        try
                        {
                            var length = await client.GetStream().ReadAsync(readBuffer, 0, readBuffer.Length);
                            if (length > 0)
                            {
                                var data = NetBufferPool.Alloc(length);
                                Array.Copy(readBuffer, data, length);
                                receiveCb?.Invoke(data, length);
                            }
                            else if(length < 0)
                            {
                                LogMsg("接受消息长度异常：" + length);
                                notifyDisconnectRet(NetCode.ReceiveErr);
                            }
                        }
                        catch (Exception e)
                        {
                            LogMsg("接受消息发生异常" + e.ToString());
                            notifyDisconnectRet(NetCode.ReceiveErr);
                        }
                    }
                    else
                    {
                        await Task.Delay(10);
                    }
                });
            }
        }

        void notifyDisconnectRet(NetCode code)
        {
            disconnectedCb?.Invoke(code);
            if (client != null)
            {
                client.Close();
                client.Dispose();
                client = null;
            }
        }

        void notifyConnectRet(NetCode code)
        {
            connectRetCb?.Invoke(code);
        }
        
        public bool IsConnected => (client != null && client.Connected);

        public void Close()
        {
            actor.SendAsync(() => {
                if (client == null)
                    return;

                if (readTag != null)
                    readTag.value = false;
                client.Close();
                client.Dispose();
                client = null;
            });
        }

        public Task SendMsg(byte[] data, int size)
        {
            return actor.SendAsync(async () => {
                if (client == null)
                {
                    LogMsg("发送消息时socket为空");
                    return;
                }
                if (!client.Connected)
                {
                    LogMsg("发送消息时socket未连接");
                    return;
                }

                try
                {
                    await client.GetStream().WriteAsync(data, 0, size);
                }
                catch (Exception e)
                {
                    LogMsg("发送消息异常");
                    LogMsg(e.ToString());
                }
            });
        }
        
        /// 日志
        void LogMsg(string str, LogType type = LogType.Err)
        {
            switch (type)
            {
                case LogType.Log:
                    UnityEngine.Debug.Log(str);
                    break;
                case LogType.Wrn:
                    UnityEngine.Debug.LogWarning(str);
                    break;
                case LogType.Err:
                    UnityEngine.Debug.LogError(str);
                    break;
            }
        }
    }
}