/****************************************************************************
Copyright (c) Geek

https://github.com/leeveel/GeekServer

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
****************************************************************************/
using Net.Tcp;
using System;
using Geek.Core.Utils;
using DotNetty.Transport.Channels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Geek.Core.Net.Message;
using Geek.Core.Net.Codecs;

namespace Geek.App.Net
{
    public class LogicClient : SafeSingletonTemplate<LogicClient>
    {
        ConcurrentDictionary<int, TaskCompletionSource<IMessage>> waitMap = new ConcurrentDictionary<int, TaskCompletionSource<IMessage>>();
        TcpClient tcpClient;
        DateTime handTime;
        IChannel channel;

        public Task Connect(string configuration)
        {
            var arr = configuration.Split(':');
            int port = 0;
            if(arr.Length >= 2)
                int.TryParse(arr[1], out port);
            return Start(arr[0], port);
        }

        public async Task Start(string host, int port)
        {
            handTime = DateTime.Now;
            tcpClient = new TcpClient(host, port);
            var handlerList = new List<Type>
            {
                typeof(ClientEncoder),
                typeof(ClientDecoder),
                typeof(TcpClientHandler)
            };
            channel = await tcpClient.Connect(handlerList);
        }

        public async Task ReConnect()
        {
            if (IsConnected())
                return;
            if (tcpClient != null)
                channel = await tcpClient.ReConnect();
        }

        public Task SendMsg(IMessage msg)
        {
            if (IsConnected())
            {
                SMessage smsg = new SMessage(msg.GetMsgId(), msg.Serialize());
                return channel.WriteAndFlushAsync(smsg);
            }
            return Task.CompletedTask;           
        }

        public double GetIdleTimeInSeconds()
        {
            return (DateTime.Now - handTime).TotalSeconds;
        }

        public void OnReciveMsg(IMessage msg)
        {
            lock(tcpClient)
                handTime = DateTime.Now;
            waitMap.TryRemove(msg.GetMsgId(), out var task);
            if (task != null)
                task.SetResult(msg);
        }

        public Task<IMessage> WaitMsg(int msgId, int timeOut = 10000)
        {
            waitMap.TryAdd(msgId, new TaskCompletionSource<IMessage>());
            waitMap.TryGetValue(msgId, out var task);
            if (task != null)
            {
                task.Task.Wait(timeOut);//默认10秒超时
                return task.Task;
            }
            return Task.FromResult<IMessage>(null);
        }

        public bool IsConnected()
        {
            if (channel != null)
                return channel.Active && channel.Open;
            return false;
        }

        public async Task Close()
        {
            if (tcpClient != null)
                await tcpClient.Close();
            if (channel != null && channel.Active && channel.Open)
                await channel.CloseAsync();
            tcpClient = null;
            channel = null;
        }
    }
}
