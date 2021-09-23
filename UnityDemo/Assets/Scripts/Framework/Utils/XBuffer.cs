using System;

namespace Geek.Client
{
    public class XBuffer
    {
        private static readonly int intSize = sizeof(int);
        private static readonly int shortSize = sizeof(short);
        private static readonly int longSize = sizeof(long);
        private static readonly int floatSize = sizeof(float);
        private static readonly int doubleSize = sizeof(double);
        private static readonly int byteSize = sizeof(byte);
        private static readonly int sbyteSize = sizeof(sbyte);
        private static readonly int boolSize = sizeof(bool);

        #region Write
        public static unsafe void WriteInt(int value, byte[] buffer, ref int offset)
        {
            if (offset + intSize > buffer.Length)
            {
                offset += intSize;
                return;
            }

            fixed (byte* ptr = buffer)
            {
                *(int*)(ptr + offset) = System.Net.IPAddress.HostToNetworkOrder(value);
                offset += intSize;
            }
        }

        public static unsafe void WriteShort(short value, byte[] buffer, ref int offset)
        {
            if (offset + shortSize > buffer.Length)
            {
                offset += shortSize;
                return;
            }

            fixed (byte* ptr = buffer)
            {
                *(short*)(ptr + offset) = System.Net.IPAddress.HostToNetworkOrder(value);
                offset += shortSize;
            }
        }

        public static unsafe void WriteLong(long value, byte[] buffer, ref int offset)
        {
            if (offset + longSize > buffer.Length)
            {
                offset += longSize;
                return;
            }

            fixed (byte* ptr = buffer)
            {
                *(long*)(ptr + offset) = System.Net.IPAddress.HostToNetworkOrder(value);
                offset += longSize;
            }
        }

        public static unsafe void WriteFloat(float value, byte[] buffer, ref int offset)
        {
            if (offset + floatSize > buffer.Length)
            {
                offset += floatSize;
                return;
            }

            fixed (byte* ptr = buffer)
            {
                *(float*)(ptr + offset) = value;
                *(int*)(ptr + offset) = System.Net.IPAddress.HostToNetworkOrder(*(int*)(ptr + offset));
                offset += floatSize;
            }
        }

        public static unsafe void WriteDouble(double value, byte[] buffer, ref int offset)
        {
            if (offset + doubleSize > buffer.Length)
            {
                offset += doubleSize;
                return;
            }

            fixed (byte* ptr = buffer)
            {
                *(double*)(ptr + offset) = value;
                *(long*)(ptr + offset) = System.Net.IPAddress.HostToNetworkOrder(*(long*)(ptr + offset));
                offset += doubleSize;
            }
        }

        public static unsafe void WriteByte(byte value, byte[] buffer, ref int offset)
        {
            if (offset + byteSize > buffer.Length)
            {
                offset += byteSize;
                return;
            }

            fixed (byte* ptr = buffer)
            {
                *(ptr + offset) = value;
                offset += byteSize;
            }
        }

        public static unsafe void WriteBytes(byte[] value, byte[] buffer, ref int offset)
        {
            if (value == null)
            {
                WriteInt(0, buffer, ref offset);
                return;
            }

            if (offset + value.Length + intSize > buffer.Length)
            {
                offset += value.Length + intSize;
                return;
            }

            WriteInt(value.Length, buffer, ref offset);
            System.Array.Copy(value, 0, buffer, offset, value.Length);
            offset += value.Length;
        }

        public static unsafe void WriteSByte(sbyte value, byte[] buffer, ref int offset)
        {
            if (offset + sbyteSize > buffer.Length)
            {
                offset += sbyteSize;
                return;
            }

            fixed (byte* ptr = buffer)
            {
                *(sbyte*)(ptr + offset) = value;
                offset += sbyteSize;
            }
        }

        public static unsafe void WriteString(string value, byte[] buffer, ref int offset)
        {
            if (value == null)
                value = "";
            fixed (byte* ptr = buffer)
            {
                int len = 0;
                if (offset >= buffer.Length)
                {
                    //预判已经超出长度了，直接计算长度就行了
                    len = System.Text.Encoding.UTF8.GetBytes(value).Length + shortSize;
                }
                else
                {
                    try
                    {
                        len = System.Text.Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, offset + shortSize);
                    }
                    catch (Exception e)
                    {
                        len = System.Text.Encoding.UTF8.GetBytes(value).Length + shortSize;
                        if (offset + len <= buffer.Length)
                            throw e;
                    }
                }

                if (offset + len + shortSize > buffer.Length)
                {
                    offset += len + shortSize;
                    return;
                }
                WriteShort((short)len, buffer, ref offset);
                offset += len;
            }
        }

        public static unsafe void WriteBool(bool value, byte[] buffer, ref int offset)
        {
            if (offset + boolSize > buffer.Length)
            {
                offset += boolSize;
                return;
            }

            fixed (byte* ptr = buffer)
            {
                *(bool*)(ptr + offset) = value;
                offset += boolSize;
            }
        }
        #endregion

        #region Read

        public static unsafe int ReadInt(byte[] buffer, ref int offset)
        {
            if (offset > buffer.Length + intSize)
                throw new Exception("xbuffer read out of index");

            fixed (byte* ptr = buffer)
            {
                var value = *(int*)(ptr + offset);
                offset += intSize;
                return System.Net.IPAddress.NetworkToHostOrder(value);
            }
        }

        public static unsafe short ReadShort(byte[] buffer, ref int offset)
        {
            if (offset > buffer.Length + shortSize)
                throw new Exception("xbuffer read out of index");

            fixed (byte* ptr = buffer)
            {
                var value = *(short*)(ptr + offset);
                offset += shortSize;
                return System.Net.IPAddress.NetworkToHostOrder(value);
            }
        }

        public static unsafe long ReadLong(byte[] buffer, ref int offset)
        {
            if (offset > buffer.Length + longSize)
                throw new Exception("xbuffer read out of index");

            fixed (byte* ptr = buffer)
            {
                var value = *(long*)(ptr + offset);
                offset += longSize;
                return System.Net.IPAddress.NetworkToHostOrder(value);
            }
        }

        public static unsafe float ReadFloat(byte[] buffer, ref int offset)
        {
            if (offset > buffer.Length + floatSize)
                throw new Exception("xbuffer read out of index");

            fixed (byte* ptr = buffer)
            {
                *(int*)(ptr + offset) = System.Net.IPAddress.NetworkToHostOrder(*(int*)(ptr + offset));
                var value = *(float*)(ptr + offset);
                offset += floatSize;
                return value;
            }
        }

        public static unsafe double ReadDouble(byte[] buffer, ref int offset)
        {
            if (offset > buffer.Length + doubleSize)
                throw new Exception("xbuffer read out of index");

            fixed (byte* ptr = buffer)
            {
                *(long*)(ptr + offset) = System.Net.IPAddress.NetworkToHostOrder(*(long*)(ptr + offset));
                var value = *(double*)(ptr + offset);
                offset += doubleSize;
                return value;
            }
        }

        public static unsafe byte ReadByte(byte[] buffer, ref int offset)
        {
            if (offset > buffer.Length + byteSize)
                throw new Exception("xbuffer read out of index");

            fixed (byte* ptr = buffer)
            {
                var value = *(ptr + offset);
                offset += byteSize;
                return value;
            }
        }

        public static unsafe byte[] ReadBytes(byte[] buffer, ref int offset)
        {
            var len = ReadInt(buffer, ref offset);
            //数据不可信
            if (len == 0 || offset > buffer.Length + len * byteSize)
                return new byte[0];

            var data = new byte[len];
            System.Array.Copy(buffer, offset, data, 0, len);
            offset += len;
            return data;
        }

        public static unsafe sbyte ReadSByte(byte[] buffer, ref int offset)
        {
            if (offset > buffer.Length + byteSize)
                throw new Exception("xbuffer read out of index");

            fixed (byte* ptr = buffer)
            {
                var value = *(sbyte*)(ptr + offset);
                offset += byteSize;
                return value;
            }
        }

        public static unsafe string ReadString(byte[] buffer, ref int offset)
        {
            fixed (byte* ptr = buffer)
            {
                var len = ReadShort(buffer, ref offset);
                //数据不可信
                if (len == 0 || offset > buffer.Length + len * byteSize)
                    return "";

                var value = System.Text.Encoding.UTF8.GetString(buffer, offset, len);
                offset += len;
                return value;
            }
        }

        public static unsafe bool ReadBool(byte[] buffer, ref int offset)
        {
            if (offset > buffer.Length + boolSize)
                throw new Exception("xbuffer read out of index");

            fixed (byte* ptr = buffer)
            {
                var value = *(bool*)(ptr + offset);
                offset += boolSize;
                return value;
            }
        }
        #endregion

    }
}