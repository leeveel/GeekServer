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

    }
}
