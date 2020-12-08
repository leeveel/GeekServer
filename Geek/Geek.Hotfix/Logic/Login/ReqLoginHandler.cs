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
using Geek.Core.Actor;
using Geek.Core.Net.Handler;
using Geek.App.Login;
using Message.Login;
using System.Threading;
using System.Threading.Tasks;
using Geek.App.Logic.Login;

namespace Geek.Hotfix.Logic.Login
{
    [TcpMsgMapping(typeof(ReqLogin))]
    public class ReqLoginHandler : BaseTcpHandler
    {
        public override async Task ActionAsync()
        {
            var loginActor = await ActorManager.GetOrNew<LoginActor>(ServerActorID.Login);
            if (loginActor.QueueNum > Settings.Ins.loginQueueNum)
            {
                //等待登陆的人太多了，等会儿吧
                ChannelUtils.SendToClient(Ctx, new ResLogin() {
                    result = -1,
                    reason = 1,
                    UniId = Msg.UniId,
                });
                await Task.Delay(100);
                await Ctx.CloseAsync();
                return;
            }

            //放行登陆
            Interlocked.Increment(ref loginActor.QueueNum);
            await loginActor.SendAsync(() => {
                Interlocked.Decrement(ref loginActor.QueueNum);
                var agent = (LoginActorAgent)loginActor.Agent;
                return agent.ReqLogin((ReqLogin)Msg, Ctx);
            });
        }
    }
}
