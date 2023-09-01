using System.Runtime.CompilerServices;
 
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
