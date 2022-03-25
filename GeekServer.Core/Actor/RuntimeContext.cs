using System.Runtime.CompilerServices;
using System.Threading;

namespace Geek.Server
{
    internal class RuntimeContext
    {
        internal static long Current => callCtx.Value;
        internal static BaseActor CurentEntityId => callEntityCtx.Value;

        internal static AsyncLocal<long> callCtx = new AsyncLocal<long>();
        internal static AsyncLocal<BaseActor> callEntityCtx = new AsyncLocal<BaseActor>();


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetContext(long callChainId, BaseActor actor)
        {
            callCtx.Value = callChainId;
            callEntityCtx.Value = actor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ResetContext()
        {
            callCtx.Value = 0;
            callEntityCtx.Value = null;
        }
    }
}

