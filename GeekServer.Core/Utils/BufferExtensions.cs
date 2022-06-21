using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geek.Server
{
    public static class BufferExtensions
    {

        public static T[] ToPooledArray<T>(in this ReadOnlySequence<T> sequence)
        {
            var array = ArrayPool<T>.Shared.Rent((int)sequence.Length);
            sequence.CopyTo(array);
            return array;
        }

        //public static void ExtractMessageHeaderOld(ReadOnlySequence<byte> ros, out MessageHeader messageHeader)
        //{
        //    Span<byte> stackSpan = stackalloc byte[(int)ros.Length];
        //    ros.CopyTo(stackSpan);

        //    ReadOnlySpan<MessageHeader> mhSpan = MemoryMarshal.Cast<byte, MessageHeader>(stackSpan);

        //    messageHeader = mhSpan[0];
        //}

    }
}
