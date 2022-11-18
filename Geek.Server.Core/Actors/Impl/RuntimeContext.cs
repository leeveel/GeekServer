using System.Runtime.CompilerServices;

namespace Geek.Server.Core.Actors.Impl
{
    internal class RuntimeContext
    {
        internal static long CurChainId => chainCtx.Value;
        internal static long CurActor => actorCtx.Value;

        internal static AsyncLocal<long> chainCtx = new();
        internal static AsyncLocal<long> actorCtx = new();


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetContext(long callChainId, long actorId)
        {
            chainCtx.Value = callChainId;
            actorCtx.Value = actorId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ResetContext()
        {
            chainCtx.Value = 0;
            actorCtx.Value = 0;
        }
    }
}
