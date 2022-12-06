using Geek.Server.App.Login;
using Geek.Server.App.Server;
using Geek.Server.App.TestServer;
using Geek.Server.Core.Actors;
using Geek.Server.Core.Actors.Impl;
using Geek.Server.Core.Hotfix.Agent;
using Geek.Server.Core.Timer.Handler;
using Geek.Server.Hotfix.Role.Base;

namespace Geek.Server.Hotfix.Server
{
    public class TestServerCompAgent : StateCompAgent<TestServerComp, TestServerState>
    {
        static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public virtual Task TestCall(string testP1, long testP2)
        {
            Log.Info($"TestServerCompAgent.....TestCall:{testP1} {testP2}");
            return Task.CompletedTask;
        }

        public virtual Task TestCall2(PlayerInfo info)
        {
            Log.Info($"TestServerCompAgent.....TestCall: {MessagePack.MessagePackSerializer.SerializeToJson(info)}");
            return Task.CompletedTask;
        }

        public virtual Task TestCall3(List<int> list)
        {
            Log.Info($"TestServerCompAgent.....TestCall: {string.Join(",", list)}");
            return Task.CompletedTask;
        }
    }
}
