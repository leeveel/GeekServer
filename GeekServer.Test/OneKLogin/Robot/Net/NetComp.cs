using DotNetty.Transport.Channels;
using System;
using System.Threading.Tasks;

namespace Geek.Server.Test
{

    public class NetComp : NoHotfixComponent
    {
        public DateTime handTime;
        public IChannel channel;
        public MsgWaiter Waiter = new MsgWaiter();
        public int UniId { set; get; } = 200;
    }

    public class NetCompAgent : FuncComponentAgent<NetComp>
    {
        public async Task Start()
        {
            Comp.handTime = DateTime.Now;
            Comp.channel = await RobotClient.Connect();
            //添加session
            Session session = new Session();
            session.Id = EntityId;
            Comp.channel.GetAttribute(SessionManager.SESSION).Set(session);
        }

        public Task<bool> SendMsg(IMessage msg)
        {
            msg.UniId = Comp.UniId++;
            NMessage nmsg = NMessage.Create(msg.GetMsgId(), msg.Serialize());
            Comp.channel.WriteAndFlushAsync(nmsg);
            return Comp.Waiter.StartWait(msg.UniId);
        }

        public double GetIdleTimeInSeconds()
        {
            return (DateTime.Now - Comp.handTime).TotalSeconds;
        }

        public bool IsConnected()
        {
            if (Comp.channel != null)
                return Comp.channel.Active && Comp.channel.Open;
            return false;
        }

        public Task Close()
        {
            return Task.CompletedTask;
        }

    }
    
}
