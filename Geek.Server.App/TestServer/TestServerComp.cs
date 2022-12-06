using Geek.Server.Core.Actors;
using Geek.Server.Core.Actors.Impl;
using Geek.Server.Core.Comps;
using Geek.Server.Core.Storage;

namespace Geek.Server.App.TestServer
{
    public class TestServerState : CacheState
    {
    }

    [Comp(ActorType.TestServer)]
    public class TestServerComp : StateComp<TestServerState>
    {
    }
}
