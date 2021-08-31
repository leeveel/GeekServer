using System.Threading.Tasks;

namespace Geek.Server
{
    public class WorkerActor : BaseActor
    {
        public WorkerActor(int parallelism = 1) : base(parallelism) { }
    }
}
