using System.Threading.Tasks;

namespace Geek.Server
{
    public class WorkerActor : BaseActor
    {
        public WorkerActor(int parallelism = 1) : base(parallelism) { }
        public override Task Active() { return Task.CompletedTask; }
        public override Task Deactive() { return Task.CompletedTask; }
    }
}
