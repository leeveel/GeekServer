using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Geek.Server
{
    public class RuntimeContext
    {
        public static long Current => callCtx.Value;

        public static AsyncLocal<long> callCtx = new AsyncLocal<long>();


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetContext(long callChainId)
        {
            callCtx.Value = callChainId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ResetContext()
        {
            callCtx.Value = 0;
        }

    }
}

