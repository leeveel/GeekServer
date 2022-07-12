using System;

public class XBuffer
{
    public const int IntSize = sizeof(int);
    public const int ShortSize = sizeof(short);
    public const int LongSize = sizeof(long);
    public const int FloatSize = sizeof(float);
    public const int DoubleSize = sizeof(double);
    public const int ByteSize = sizeof(byte);
    public const int SbyteSize = sizeof(sbyte);
    public const int BoolSize = sizeof(bool);

    #region WriteSpan
    public static unsafe void WriteInt(int value, Span<byte> buffer, ref int offset)
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

    public static unsafe void WriteShort(short value, Span<byte> buffer, ref int offset)
    {
        if (offset + ShortSize > buffer.Length)
        {
            throw new ArgumentException($"xbuffer write out of index {offset + ShortSize}, {buffer.Length}");
        }

        fixed (byte* ptr = buffer)
        {
            *(short*)(ptr + offset) = System.Net.IPAddress.HostToNetworkOrder(value);
            offset += ShortSize;
        }
    }

    public static unsafe void WriteLong(long value, Span<byte> buffer, ref int offset)
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

    public static unsafe void WriteFloat(float value, Span<byte> buffer, ref int offset)
    {
        if (offset + FloatSize > buffer.Length)
        {
            throw new ArgumentException($"xbuffer write out of index {offset + FloatSize}, {buffer.Length}");
        }

        fixed (byte* ptr = buffer)
        {
            *(float*)(ptr + offset) = value;
            *(int*)(ptr + offset) = System.Net.IPAddress.HostToNetworkOrder(*(int*)(ptr + offset));
            offset += FloatSize;
        }
    }

    public static unsafe void WriteDouble(double value, Span<byte> buffer, ref int offset)
    {
        if (offset + DoubleSize > buffer.Length)
        {
            throw new ArgumentException($"xbuffer write out of index {offset + DoubleSize}, {buffer.Length}");
        }

        fixed (byte* ptr = buffer)
        {
            *(double*)(ptr + offset) = value;
            *(long*)(ptr + offset) = System.Net.IPAddress.HostToNetworkOrder(*(long*)(ptr + offset));
            offset += DoubleSize;
        }
    }

    public static unsafe void WriteByte(byte value, Span<byte> buffer, ref int offset)
    {
        if (offset + ByteSize > buffer.Length)
        {
            throw new ArgumentException($"xbuffer write out of index {offset + ByteSize}, {buffer.Length}");
        }

        fixed (byte* ptr = buffer)
        {
            *(ptr + offset) = value;
            offset += ByteSize;
        }
    }

    public static unsafe void WriteBytes(byte[] value, Span<byte> buffer, ref int offset)
    {
        if (value == null)
        {
            WriteInt(0, buffer, ref offset);
            return;
        }

        if (offset + value.Length + IntSize > buffer.Length)
        {
            throw new ArgumentException($"xbuffer write out of index {offset + value.Length + IntSize}, {buffer.Length}");
        }

        WriteInt(value.Length, buffer, ref offset);
        //System.Array.Copy(value, 0, buffer, offset, value.Length);
        //offset += value.Length;
        fixed (byte* ptr = buffer, valPtr = value)
        {
            Buffer.MemoryCopy(valPtr, ptr + offset, value.Length, value.Length);
            offset += value.Length;
        }
    }

    public static unsafe void WriteBytesWithoutLength(byte[] value, Span<byte> buffer, ref int offset)
    {
        if (value == null)
        {
            WriteInt(0, buffer, ref offset);
            return;
        }

        if (offset + value.Length + IntSize > buffer.Length)
        {
            throw new ArgumentException($"xbuffer write out of index {offset + value.Length + IntSize}, {buffer.Length}");
        }

        fixed (byte* ptr = buffer, valPtr = value)
        {
            Buffer.MemoryCopy(valPtr, ptr + offset, value.Length, value.Length);
            offset += value.Length;
        }
    }

    public static unsafe void WriteSByte(sbyte value, Span<byte> buffer, ref int offset)
    {
        if (offset + SbyteSize > buffer.Length)
        {
            throw new ArgumentException($"xbuffer write out of index {offset + SbyteSize}, {buffer.Length}");
        }

        fixed (byte* ptr = buffer)
        {
            *(sbyte*)(ptr + offset) = value;
            offset += SbyteSize;
        }
    }

    public static unsafe void WriteString(string value, Span<byte> buffer, ref int offset)
    {
        if (value == null)
            value = "";

        int len = System.Text.Encoding.UTF8.GetByteCount(value);
        if (len > short.MaxValue)
            throw new ArgumentException($"string length exceed short.MaxValue {len}, {short.MaxValue}");

        //预判已经超出长度了，直接计算长度就行了
        if (offset + len + ShortSize > buffer.Length)
        {
            throw new ArgumentException($"xbuffer write out of index {offset + len + ShortSize}, {buffer.Length}");
        }

        WriteShort((short)len, buffer, ref offset);
        var val = System.Text.Encoding.UTF8.GetBytes(value);
        fixed (byte* ptr = buffer, valPtr = val)
        {
            //System.Text.Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, offset + ShortSize);
            //WriteShort((short)len, buffer, ref offset);
            //offset += len;
            Buffer.MemoryCopy(valPtr, ptr + offset, len, len);
            offset += len;
        }
    }

    public static unsafe void WriteBool(bool value, Span<byte> buffer, ref int offset)
    {
        if (offset + BoolSize > buffer.Length)
        {
            throw new ArgumentException($"xbuffer write out of index {offset + BoolSize}, {buffer.Length}");
        }

        fixed (byte* ptr = buffer)
        {
            *(bool*)(ptr + offset) = value;
            offset += BoolSize;
        }
    }


    #endregion



    #region ReadSpan
    public static unsafe int ReadInt(Span<byte> buffer, ref int offset)
    {
        if (offset > buffer.Length + IntSize)
            throw new Exception("xbuffer read out of index");

        fixed (byte* ptr = buffer)
        {
            var value = *(int*)(ptr + offset);
            offset += IntSize;
            return System.Net.IPAddress.NetworkToHostOrder(value);
        }
    }

    public static unsafe short ReadShort(Span<byte> buffer, ref int offset)
    {
        if (offset > buffer.Length + ShortSize)
            throw new Exception("xbuffer read out of index");

        fixed (byte* ptr = buffer)
        {
            var value = *(short*)(ptr + offset);
            offset += ShortSize;
            return System.Net.IPAddress.NetworkToHostOrder(value);
        }
    }

    public static unsafe long ReadLong(Span<byte> buffer, ref int offset)
    {
        if (offset > buffer.Length + LongSize)
            throw new Exception("xbuffer read out of index");

        fixed (byte* ptr = buffer)
        {
            var value = *(long*)(ptr + offset);
            offset += LongSize;
            return System.Net.IPAddress.NetworkToHostOrder(value);
        }
    }

    public static unsafe float ReadFloat(Span<byte> buffer, ref int offset)
    {
        if (offset > buffer.Length + FloatSize)
            throw new Exception("xbuffer read out of index");

        fixed (byte* ptr = buffer)
        {
            *(int*)(ptr + offset) = System.Net.IPAddress.NetworkToHostOrder(*(int*)(ptr + offset));
            var value = *(float*)(ptr + offset);
            offset += FloatSize;
            return value;
        }
    }

    public static unsafe double ReadDouble(Span<byte> buffer, ref int offset)
    {
        if (offset > buffer.Length + DoubleSize)
            throw new Exception("xbuffer read out of index");

        fixed (byte* ptr = buffer)
        {
            *(long*)(ptr + offset) = System.Net.IPAddress.NetworkToHostOrder(*(long*)(ptr + offset));
            var value = *(double*)(ptr + offset);
            offset += DoubleSize;
            return value;
        }
    }

    public static unsafe byte ReadByte(Span<byte> buffer, ref int offset)
    {
        if (offset > buffer.Length + ByteSize)
            throw new Exception("xbuffer read out of index");

        fixed (byte* ptr = buffer)
        {
            var value = *(ptr + offset);
            offset += ByteSize;
            return value;
        }
    }

    public static unsafe byte[] ReadBytes(Span<byte> buffer, ref int offset)
    {
        var len = ReadInt(buffer, ref offset);
        //数据不可信
        if (len <= 0 || offset > buffer.Length + len * ByteSize)
            return new byte[0];

        //var data = new byte[len];
        //System.Array.Copy(buffer, offset, data, 0, len);
        var data = buffer.Slice(offset, len).ToArray();
        offset += len;
        return data;
    }

    public static unsafe sbyte ReadSByte(Span<byte> buffer, ref int offset)
    {
        if (offset > buffer.Length + ByteSize)
            throw new Exception("xbuffer read out of index");

        fixed (byte* ptr = buffer)
        {
            var value = *(sbyte*)(ptr + offset);
            offset += ByteSize;
            return value;
        }
    }

    public static unsafe string ReadString(Span<byte> buffer, ref int offset)
    {
        var len = ReadShort(buffer, ref offset);
        //数据不可信
        if (len <= 0 || offset > buffer.Length + len * ByteSize)
            return "";
        fixed (byte* ptr = buffer)
        {
            var value = System.Text.Encoding.UTF8.GetString(ptr + offset, len);
            offset += len;
            return value;
        }
    }

    public static unsafe bool ReadBool(Span<byte> buffer, ref int offset)
    {
        if (offset > buffer.Length + BoolSize)
            throw new Exception("xbuffer read out of index");

        fixed (byte* ptr = buffer)
        {
            var value = *(bool*)(ptr + offset);
            offset += BoolSize;
            return value;
        }
    }
    #endregion


    #region Write

    public static unsafe void WriteInt(int value, byte[] buffer, ref int offset)
    {
        if (offset + IntSize > buffer.Length)
        {
            offset += IntSize;
            return;
        }

        fixed (byte* ptr = buffer)
        {
            *(int*)(ptr + offset) = System.Net.IPAddress.HostToNetworkOrder(value);
            offset += IntSize;
        }
    }

    public static unsafe void WriteShort(short value, byte[] buffer, ref int offset)
    {
        if (offset + ShortSize > buffer.Length)
        {
            offset += ShortSize;
            return;
        }

        fixed (byte* ptr = buffer)
        {
            *(short*)(ptr + offset) = System.Net.IPAddress.HostToNetworkOrder(value);
            offset += ShortSize;
        }
    }

    public static unsafe void WriteLong(long value, byte[] buffer, ref int offset)
    {
        if (offset + LongSize > buffer.Length)
        {
            offset += LongSize;
            return;
        }

        fixed (byte* ptr = buffer)
        {
            *(long*)(ptr + offset) = System.Net.IPAddress.HostToNetworkOrder(value);
            offset += LongSize;
        }
    }

    public static unsafe void WriteFloat(float value, byte[] buffer, ref int offset)
    {
        if (offset + FloatSize > buffer.Length)
        {
            offset += FloatSize;
            return;
        }

        fixed (byte* ptr = buffer)
        {
            *(float*)(ptr + offset) = value;
            *(int*)(ptr + offset) = System.Net.IPAddress.HostToNetworkOrder(*(int*)(ptr + offset));
            offset += FloatSize;
        }
    }

    public static unsafe void WriteDouble(double value, byte[] buffer, ref int offset)
    {
        if (offset + DoubleSize > buffer.Length)
        {
            offset += DoubleSize;
            return;
        }

        fixed (byte* ptr = buffer)
        {
            *(double*)(ptr + offset) = value;
            *(long*)(ptr + offset) = System.Net.IPAddress.HostToNetworkOrder(*(long*)(ptr + offset));
            offset += DoubleSize;
        }
    }

    public static unsafe void WriteByte(byte value, byte[] buffer, ref int offset)
    {
        if (offset + ByteSize > buffer.Length)
        {
            offset += ByteSize;
            return;
        }

        fixed (byte* ptr = buffer)
        {
            *(ptr + offset) = value;
            offset += ByteSize;
        }
    }

    public static unsafe void WriteBytes(byte[] value, byte[] buffer, ref int offset)
    {
        if (value == null)
        {
            WriteInt(0, buffer, ref offset);
            return;
        }

        if (offset + value.Length + IntSize > buffer.Length)
        {
            offset += value.Length + IntSize;
            return;
        }

        WriteInt(value.Length, buffer, ref offset);
        System.Array.Copy(value, 0, buffer, offset, value.Length);
        offset += value.Length;
    }

    public static unsafe void WriteSByte(sbyte value, byte[] buffer, ref int offset)
    {
        if (offset + SbyteSize > buffer.Length)
        {
            offset += SbyteSize;
            return;
        }

        fixed (byte* ptr = buffer)
        {
            *(sbyte*)(ptr + offset) = value;
            offset += SbyteSize;
        }
    }

    public static unsafe void WriteString(string value, byte[] buffer, ref int offset)
    {
        if (value == null)
            value = "";

        int len = System.Text.Encoding.UTF8.GetByteCount(value);

        if (len > short.MaxValue)
            throw new ArgumentException($"string length exceed short.MaxValue {len}, {short.MaxValue}");

        //预判已经超出长度了，直接计算长度就行了
        if (offset + len + ShortSize > buffer.Length)
        {
            offset += len + ShortSize;
            return;
        }

        fixed (byte* ptr = buffer)
        {
            System.Text.Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, offset + ShortSize);
            WriteShort((short)len, buffer, ref offset);
            offset += len;
        }
    }

    public static unsafe void WriteBool(bool value, byte[] buffer, ref int offset)
    {
        if (offset + BoolSize > buffer.Length)
        {
            offset += BoolSize;
            return;
        }

        fixed (byte* ptr = buffer)
        {
            *(bool*)(ptr + offset) = value;
            offset += BoolSize;
        }
    }
    #endregion

    #region Read

    public static unsafe int ReadInt(byte[] buffer, ref int offset)
    {
        if (offset > buffer.Length + IntSize)
            throw new Exception("xbuffer read out of index");

        fixed (byte* ptr = buffer)
        {
            var value = *(int*)(ptr + offset);
            offset += IntSize;
            return System.Net.IPAddress.NetworkToHostOrder(value);
        }
    }

    public static unsafe short ReadShort(byte[] buffer, ref int offset)
    {
        if (offset > buffer.Length + ShortSize)
            throw new Exception("xbuffer read out of index");

        fixed (byte* ptr = buffer)
        {
            var value = *(short*)(ptr + offset);
            offset += ShortSize;
            return System.Net.IPAddress.NetworkToHostOrder(value);
        }
    }

    public static unsafe long ReadLong(byte[] buffer, ref int offset)
    {
        if (offset > buffer.Length + LongSize)
            throw new Exception("xbuffer read out of index");

        fixed (byte* ptr = buffer)
        {
            var value = *(long*)(ptr + offset);
            offset += LongSize;
            return System.Net.IPAddress.NetworkToHostOrder(value);
        }
    }

    public static unsafe float ReadFloat(byte[] buffer, ref int offset)
    {
        if (offset > buffer.Length + FloatSize)
            throw new Exception("xbuffer read out of index");

        fixed (byte* ptr = buffer)
        {
            *(int*)(ptr + offset) = System.Net.IPAddress.NetworkToHostOrder(*(int*)(ptr + offset));
            var value = *(float*)(ptr + offset);
            offset += FloatSize;
            return value;
        }
    }

    public static unsafe double ReadDouble(byte[] buffer, ref int offset)
    {
        if (offset > buffer.Length + DoubleSize)
            throw new Exception("xbuffer read out of index");

        fixed (byte* ptr = buffer)
        {
            *(long*)(ptr + offset) = System.Net.IPAddress.NetworkToHostOrder(*(long*)(ptr + offset));
            var value = *(double*)(ptr + offset);
            offset += DoubleSize;
            return value;
        }
    }

    public static unsafe byte ReadByte(byte[] buffer, ref int offset)
    {
        if (offset > buffer.Length + ByteSize)
            throw new Exception("xbuffer read out of index");

        fixed (byte* ptr = buffer)
        {
            var value = *(ptr + offset);
            offset += ByteSize;
            return value;
        }
    }

    public static unsafe byte[] ReadBytes(byte[] buffer, ref int offset)
    {
        var len = ReadInt(buffer, ref offset);
        //数据不可信
        if (len <= 0 || offset > buffer.Length + len * ByteSize)
            return new byte[0];

        var data = new byte[len];
        System.Array.Copy(buffer, offset, data, 0, len);
        offset += len;
        return data;
    }

    public static unsafe sbyte ReadSByte(byte[] buffer, ref int offset)
    {
        if (offset > buffer.Length + ByteSize)
            throw new Exception("xbuffer read out of index");

        fixed (byte* ptr = buffer)
        {
            var value = *(sbyte*)(ptr + offset);
            offset += ByteSize;
            return value;
        }
    }

    public static unsafe string ReadString(byte[] buffer, ref int offset)
    {
        fixed (byte* ptr = buffer)
        {
            var len = ReadShort(buffer, ref offset);
            //数据不可信
            if (len <= 0 || offset > buffer.Length + len * ByteSize)
                return "";

            var value = System.Text.Encoding.UTF8.GetString(buffer, offset, len);
            offset += len;
            return value;
        }
    }

    public static unsafe bool ReadBool(byte[] buffer, ref int offset)
    {
        if (offset > buffer.Length + BoolSize)
            throw new Exception("xbuffer read out of index");

        fixed (byte* ptr = buffer)
        {
            var value = *(bool*)(ptr + offset);
            offset += BoolSize;
            return value;
        }
    }
    #endregion

}