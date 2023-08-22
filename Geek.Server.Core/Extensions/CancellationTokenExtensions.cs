using System.Runtime.CompilerServices;

namespace Geek.Server.Core.Extensions
{
    public static class CancellationTokenExtensions
    {
        public static TaskAwaiter GetAwaiter(this CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            Task t = tcs.Task;
            if (cancellationToken.IsCancellationRequested) tcs.SetResult(true);
            else cancellationToken.Register(s =>
            {
                ((TaskCompletionSource<bool>)s).SetResult(true);
            }, tcs);
            return t.GetAwaiter();
        }
    }
}
