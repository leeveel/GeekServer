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
using DotNetty.Buffers;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Geek.Core.Net.Message
{
    //�����������ݿ���
    [BsonIgnoreExtraElements(true, Inherited = true)]//���Դ���ɾ�����ֶ�[���ݿ������ֶ�]
    public abstract class BaseMessage : IMessage
    {
        /// <summary>
        /// ��ϢΨһid
        /// </summary>
        [BsonIgnore]
        public int UniId { get; set; }

        public virtual int Read(byte[] buffer, int offset)
        {
            return offset;
        }

        public virtual int Write(byte[] buffer, int offset)
        {
            return offset;
        }

        public virtual int WriteWithType(byte[] buffer, int offset)
        {
            return offset;
        }

        public virtual void Reset()
        {

        }

        public virtual int GetMsgId()
        {
            return 0;
        }

        public byte[] Serialize()
        {
            return writeAsBuffer(512);
        }

        byte[] writeAsBuffer(int size)
        {
            var data = new byte[size];
            int offset = this.Write(data, 0);
            if(offset <= data.Length)
            {
                if(offset < data.Length)
                {
                    var ret = new byte[offset];
                    Array.Copy(data, 0, ret, 0, offset);
                    data = ret;
                }
                return data;
            }
            else
            {
                return writeAsBuffer(offset);
            }
        }

        public void Deserialize(byte[] data)
        {
            this.Read(data, 0);
        }

        public void Deserialize(IByteBuffer buffer)
        {
            throw new NotImplementedException("BaseMessage.Deserialize(IByteBuffer) not implemented");
        }
    }
}