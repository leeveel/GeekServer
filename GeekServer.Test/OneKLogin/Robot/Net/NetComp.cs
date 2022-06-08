using System;
using System.Threading.Tasks;

namespace Geek.Server.Test
{

    public class NetComp : NoHotfixComponent
    {
        public DateTime handTime;
        public ClientNetChannel Channel { get; set; }
        public MsgWaiter Waiter = new MsgWaiter();
        public int UniId { set; get; } = 200;
    }

    public class NetCompAgent : FuncComponentAgent<NetComp>
    {
        public async Task Start()
        {
            Comp.handTime = DateTime.Now;
            Comp.Channel = await RobotClient.Connect();
            Comp.Channel.Start();
            //添加session
            Session session = new Session();
            session.Id = EntityId;
            //Comp.channel.GetAttribute(SessionManager.SESSION).Set(session);
            Comp.Channel.SetSessionId(session.Id);
        }

        public Task<bool> SendMsg(IMessage msg)
        {
            msg.UniId = Comp.UniId++;
            Message nmsg = Message.Create(msg.MsgId, msg.Serialize());
            //Comp.channel.WriteAndFlushAsync(nmsg);
            Comp.Channel.WriteAsync(nmsg);
            return Comp.Waiter.StartWait(msg.UniId);
        }

        public double GetIdleTimeInSeconds()
        {
            return (DateTime.Now - Comp.handTime).TotalSeconds;
        }

        public bool IsConnected()
        {
            return Comp.Channel != null && Comp.Channel.Context != null;
        }

        public Task Close()
        {
            return Task.CompletedTask;
        }

    }
    
}
