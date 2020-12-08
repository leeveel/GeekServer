/****************************************************************************
Copyright (c) Geek

https://github.com/leeveel/GeekServer

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
****************************************************************************/
using System;
using System.Collections.Generic;
using DotNetty.Buffers;
using NLog;

namespace Geek.Core.Net.Message
{
    public abstract class BinaryMessageStruct : IMessage
    {
        private readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        // binary data DotNetty.Buffers.IByteBuffer
        protected IByteBuffer buffer;
        
        /// <summary>
        /// 二进制数据  
        /// </summary>
        //public byte[] ByteArr { get; private set; }

        // message length
        protected int msgLength;

        public int UniId { get; set; }

        public void WriteBool(bool boolValue)
        {
            buffer.WriteBoolean(boolValue);
        }

        public void WriteByte(int byteValue)
        {
            buffer.WriteByte(byteValue);
        }

        public void WriteShort(int shortValue)
        {
            buffer.WriteShort(shortValue);
        }

        public void WriteInt(int intValue)
        {
            buffer.WriteInt(intValue);
        }

        public void WriteLong(long longValue)
        {
            buffer.WriteLong(longValue);
        }

        public void WriteFloat(float floatValue)
        {
            buffer.WriteFloat(floatValue);
        }

        public void WriteDouble(double doubleValue)
        {
            buffer.WriteDouble(doubleValue);
        }

        public void WriteString(string strValue)
        {
            if (strValue == null)
            {
                buffer.WriteShort(0);
            }
            else
            {
                try
                {
                    byte[] ex = System.Text.Encoding.UTF8.GetBytes(strValue);
                    int length = ex.Length;
                    buffer.WriteShort(length);
                    buffer.WriteBytes(ex);
                }
                catch (Exception ex)
                {
                    LOGGER.Error(ex, "encode  binary message data error,Exception :{}");
                }
            }
        }

        public void WriteByteArray(byte[] bytes)
        {
            if (bytes == null)
                buffer.WriteShort(0);
            else
            {
                buffer.WriteShort(bytes.Length);
                buffer.WriteBytes(bytes);
            }
        }

        public void WriteBoolList(List<bool> boolValues)
        {
            if (boolValues == null)
            {
                buffer.WriteShort(0);
                return;
            }

            int size = boolValues.Count;
            buffer.WriteShort(size);
            foreach (var value in boolValues)
            {
                buffer.WriteBoolean(value);
            }
        }

        public void WriteByteList(List<byte> byteValues)
        {
            if (byteValues == null)
            {
                buffer.WriteShort(0);
                return;
            }

            int size = byteValues.Count;
            buffer.WriteShort(size);
            foreach (var _byte in byteValues)
            {
                buffer.WriteByte(_byte);
            }
        }

        public void WriteShortList(List<short> shortValues)
        {
            if (shortValues == null)
            {
                buffer.WriteShort(0);
                return;
            }

            int size = shortValues.Count;
            buffer.WriteShort(size);
            foreach (var _short in shortValues)
            {
                buffer.WriteShort(_short);
            }
        }

        public void WriteIntList(List<int> intValues)
        {
            if (intValues == null)
            {
                buffer.WriteShort(0);
                return;
            }

            int size = intValues.Count;
            buffer.WriteShort(size);
            foreach (var _int in intValues)
            {
                buffer.WriteInt(_int);
            }
        }

        public void WriteLongList(List<long> longValues)
        {
            if (longValues == null)
            {
                buffer.WriteShort(0);
                return;
            }

            int size = longValues.Count;
            buffer.WriteShort(size);
            foreach (var _long in longValues)
            {
                buffer.WriteLong(_long);
            }
        }

        public void WriteFloatList(List<float> floatValues)
        {
            if (floatValues == null)
            {
                buffer.WriteShort(0);
                return;
            }

            int size = floatValues.Count;
            buffer.WriteShort(size);
            foreach (var _float in
                floatValues)
            {
                buffer.WriteFloat(_float);
            }
        }

        public void WriteDoubleList(List<double> doubleValues)
        {
            if (doubleValues == null)
            {
                buffer.WriteShort(0);
                return;
            }

            int size = doubleValues.Count;
            buffer.WriteShort(size);
            foreach (var _double in
                doubleValues)
            {
                buffer.WriteDouble(_double);
            }
        }

        public void WriteStringList(List<string> stringValue)
        {
            if (stringValue == null)
            {
                buffer.WriteShort(0);
                return;
            }

            int size = stringValue.Count;
            buffer.WriteShort(size);
            foreach (var _string in
                stringValue)
            {
                WriteString(_string);
            }
        }

        public int getStringByteSize(string value)
        {
            if (value == null)
            {
                return 0;
            }

            try
            {
                byte[] ex = System.Text.Encoding.UTF8.GetBytes(value);
                return ex.Length;
            }
            catch (Exception e)
            {
                LOGGER.Error(e, "resolve  binary message data error,Exception :{}");
                return 0;
            }
        }

        public bool ReadBool()
        {
            return buffer.ReadBoolean();
        }

        public byte ReadByte()
        {
            return buffer.ReadByte();
        }

        public short ReadUnsignedByte()
        {
            return (short)(buffer.ReadByte() & 0xFF);
        }

        public short ReadShort()
        {
            return buffer.ReadShort();
        }

        public int ReadInt()
        {
            return buffer.ReadInt();
        }

        public float ReadFloat()
        {
            return buffer.ReadFloat();
        }

        public double ReadDouble()
        {
            return buffer.ReadDouble();
        }

        public long ReadLong()
        {
            return buffer.ReadLong();
        }

        public string ReadString()
        {
            int length = buffer.ReadShort();
            byte[] str = new byte[length];
            buffer.ReadBytes(str);

            try
            {
                return System.Text.Encoding.UTF8.GetString(str);
            }
            catch (Exception e)
            {
                LOGGER.Error(e, "resolve  binary message data error,Exception :{}");
                return System.Text.Encoding.UTF8.GetString(str);
            }
        }

        public byte[] ReadByteArray()
        {
            int length = buffer.ReadShort();
            byte[] bytes = new byte[length];
            buffer.ReadBytes(bytes);
            return bytes;
        }

        public List<bool> ReadBoolList(List<bool> _out)
        {
            if (_out == null)
                _out = new List<bool>();
            int length = buffer.ReadShort();
            for (int i = 0; i < length; i++)
            {
                _out[i] = buffer.ReadBoolean();
            }

            return _out;
        }

        public List<byte> ReadByteList(List<byte> _out)
        {
            if (_out == null)
                _out = new List<byte>();
            int length = buffer.ReadShort();
            for (int i = 0; i < length; i++)
            {
                _out[i] = buffer.ReadByte();
            }

            return _out;
        }

        public List<short> ReadShortList(List<short> _out)
        {
            if (_out == null)
                _out = new List<short>();
            int length = buffer.ReadShort();
            for (int i = 0; i < length; i++)
            {
                _out[i] = (buffer.ReadShort());
            }

            return _out;
        }

        public List<int> ReadIntList(List<int> _out)
        {
            if (_out == null)
                _out = new List<int>();
            int length = buffer.ReadShort();
            for (int i = 0; i < length; i++)
            {
                _out[i] = (buffer.ReadInt());
            }

            return _out;
        }

        public List<long> ReadLongList(List<long> _out)
        {
            if (_out == null)
                _out = new List<long>();
            int length = buffer.ReadShort();
            for (int i = 0; i < length; i++)
            {
                _out[i] = (buffer.ReadLong());
            }

            return _out;
        }

        public List<float> ReadFloatList(List<float> _out)
        {
            if (_out == null)
                _out = new List<float>();
            int length = buffer.ReadShort();
            for (int i = 0; i < length; i++)
            {
                _out[i] = (buffer.ReadFloat());
            }

            return _out;
        }

        public List<double> ReadDoubleList(List<double> _out)
        {
            if (_out == null)
                _out = new List<double>();
            int length = buffer.ReadShort();
            for (int i = 0; i < length; i++)
            {
                _out[i] = (buffer.ReadDouble());
            }

            return _out;
        }

        public List<string> ReadStringList(List<string> _out)
        {
            if (_out == null)
                _out = new List<string>();
            int length = buffer.ReadShort();
            for (int i = 0; i < length; i++)
            {
                _out[i] = (ReadString());
            }

            return _out;
        }

        public void Write(IByteBuffer byteBuf)
        {
            buffer = byteBuf;
            Write();
            buffer = null;
        }

        public abstract void Write();
        public abstract void Read(IByteBuffer buffer=null);

        public virtual int GetMsgId()
        {
            return -1;
        }

        /// <summary>
        /// 通过byte数组反序列化
        /// </summary>
        /// <param name="byteArr"></param>
        public void Deserialize(byte[] byteArr)
        {
            try
            {
                buffer = Unpooled.Buffer();
                buffer.WriteBytes(byteArr);
                Read();
                buffer.Release();
                buffer = null;
            }
            catch (Exception e)
            {
                LOGGER.Error(e);
            }
        }

        /// <summary>
        /// 通过IByteBuffer反序列化
        /// </summary>
        /// <param name="byteBuf"></param>
        public void Deserialize(IByteBuffer byteBuf)
        {
            try
            {
                buffer = byteBuf;
                Read();
                buffer.Release();
                buffer = null;
            }
            catch (Exception e)
            {
                LOGGER.Error(e);
            }
        }

        /// <summary>
        /// 将消息实体序列化为 IByteBuffer并返回ByteArray
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            try
            {
                //buffer = PooledByteBufferAllocator.Default.Buffer();
                buffer = Unpooled.Buffer();
                Write();
                var arr = buffer.Array;
                var data = new byte[buffer.WriterIndex];
                Array.Copy(arr, 0, data, 0, data.Length);
                buffer.Release();
                buffer = null;
                return data;
            }
            catch (Exception e)
            {
                LOGGER.Error(e);
                return null;
            }
        }

    }
}
