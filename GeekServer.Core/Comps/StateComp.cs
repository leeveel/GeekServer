using NLog;
using System.Collections.Concurrent;

namespace Geek.Server
{

    public sealed class StateComp
    {
    }

    public abstract class StateComp<TState> : BaseComp, IState where TState : CacheState, new()
    {

        static readonly Logger Log = LogManager.GetCurrentClassLogger();

        static readonly ConcurrentDictionary<long, TState> stateDic = new();

        public TState State { get; private set; }

        public override bool IsActive => State != null;

        public override Task Active()
        {
            if (State != null)
                return Task.CompletedTask;
            return ReadStateAsync();
        }

        public override Task Deactive()
        {
            stateDic.TryRemove(ActorId, out _);
            return base.Deactive();
        }

        internal override bool SaveState()
        {
            try
            {
                RocksDBConnection.Singleton.SaveState(State);
                return true;
            }
            catch (Exception e)
            {
                Log.Fatal($"StateComp.SaveState.Failed.StateId:{State.Id},{e}");
                return false;
            }
            
        }

        public Task ReadStateAsync()
        {
            State = RocksDBConnection.Singleton.LoadState<TState>(ActorId);
            stateDic.TryRemove(State.Id, out _);
            stateDic.TryAdd(State.Id, State);
            return Task.CompletedTask;
        }

        public Task WriteStateAsync()
        {
            RocksDBConnection.Singleton.SaveState(State);
            return Task.CompletedTask;
        }

    }
}
