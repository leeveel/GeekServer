
using Geek.Server.Core.Actors;
using Geek.Server.Core.Comps;

namespace Geek.Server.Core.Hotfix.Agent
{
    public interface ICompAgent
    {
        long ActorId { get; }

        BaseComp Owner { get; set; }

        void Active();

        Task Deactive();

        ActorType OwnerType { get; }

        public Task<ICompAgent> GetCompAgent(Type agentType);

        public Task<T> GetCompAgent<T>() where T : ICompAgent;

        void Tell(Action work, int timeOut = Actor.TIME_OUT);

        void Tell(Func<Task> work, int timeOut = Actor.TIME_OUT);

        Task SendAsync(Action work, int timeOut = Actor.TIME_OUT);

        Task<T> SendAsync<T>(Func<T> work, int timeOut = Actor.TIME_OUT);

        Task SendAsync(Func<Task> work, int timeOut = Actor.TIME_OUT);

        Task<T> SendAsync<T>(Func<Task<T>> work, int timeOut = Actor.TIME_OUT);

    }
}
