using System;
using System.Runtime.InteropServices;

public static class SpanExtension
{
    public const int IntSize = sizeof(int);
    public const int LongSize = sizeof(long);

    #region WriteSpan
    public static unsafe void WriteInt(this Span<byte> buffer, int value, ref int offset)
    {
        if (offset + IntSize > buffer.Length)
        {
            throw new ArgumentException($"xbuffer write out of index {offset + IntSize}, {buffer.Length}");
        }

        fixed (byte* ptr = buffer)
        {
            *(int*)(ptr + offset) = System.Net.IPAddress.HostToNetworkOrder(value);
            offset += IntSize;
        }
    }


    public static unsafe void WriteLong(this Span<byte> buffer, long value, ref int offset)
    {
        if (offset + LongSize > buffer.Length)
        {
            throw new ArgumentException($"xbuffer write out of index {offset + LongSize}, {buffer.Length}");
        }

        fixed (byte* ptr = buffer)
        {
            *(long*)(ptr + offset) = System.Net.IPAddress.HostToNetworkOrder(value);
            offset += LongSize;
        }
    }

    public static unsafe void WriteBytesWithoutLength(this Span<byte> buffer, byte[] value, ref int offset)
    {
        if (value == null)
        {
            buffer.WriteInt(0, ref offset);
            return;
        }

        if (offset + value.Length > buffer.Length)
        {
            throw new ArgumentException($"xbuffer write out of index {offset + value.Length}, {buffer.Length}");
        }

        fixed (byte* ptr = buffer, valPtr = value)
        {
            Buffer.MemoryCopy(valPtr, ptr + offset, value.Length, value.Length);
            offset += value.Length;
        }

    }
    public static ArraySegment<byte> GetArray(this ReadOnlyMemory<byte> memory)
    {
        if (!MemoryMarshal.TryGetArray(memory, out var result))
        {
            throw new InvalidOperationException("Buffer backed by array was expected");
        }
        return result;
    }
    #endregion
}