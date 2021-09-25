using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;

namespace Geek.Client
{
    public class MessageCoder
    {
        const int MAX_SEND_SIZE = 1024 * 8;
        int remainLength = 0;
        byte[] remainBuffer = new byte[1024 * 30];
        const int intSize = sizeof(int);

        public const int EncodeHeadLength = 4 + 8 + 4 + 4;
        public Func<int, BaseMessage> MsgFactory;

        public void Clear()
        {
            remainLength = 0;
            count = 0;
        }

        public void Encode(int msgId, ref byte[] data, int length)
        {
            EncodePackage(msgId, ref data, length);
        }

        private const int Magic = 0x1234;
        int count;
        private void EncodePackage(int msgId, ref byte[] data, int length)
        {
            var times = TimeUtils.CurrentTimeMillis();
            var magic = Magic + count++;
            magic ^= Magic << 8;
            magic ^= length;

            int offset = 0;
            //EncodeHeadLength = 4 + 8 + 4 + 4
            XBuffer.WriteInt(length, data, ref offset);
            XBuffer.WriteLong(times, data, ref offset);
            XBuffer.WriteInt(magic, data, ref offset);
            XBuffer.WriteInt(msgId, data, ref offset);
        }

        public void Decode(byte[] data, int length, ref Queue<RMessage> msgQueue)
        {
            if (data == null || length <= 0)
            {
                logErr("接受数据异常: length <= 0 || bytes == null");
                return;
            }

            if(remainLength + length > remainBuffer.Length)
            {
                var buffer = NetBufferPool.Alloc(remainLength + length);
                Array.Copy(remainBuffer, buffer, remainLength);
                NetBufferPool.Free(remainBuffer);
                remainBuffer = buffer;
            }
            Array.Copy(data, 0, remainBuffer, remainLength, length);
            remainLength += length;

            int offset = 0;
            while(true)
            {
                //int[totalSize]+int[msgId]+data[message]
                if (remainLength < intSize * 2)
                    break;

                var msgLength = XBuffer.ReadInt(remainBuffer, ref offset);
                bool isZip = msgLength < 0;
                if (msgLength < 0)
                    msgLength = -msgLength;

                remainLength -= msgLength;
                if (msgLength > MAX_SEND_SIZE)
                {
                    logErr("接受数据异常, 数据长度超出限制: " + msgLength);
                    offset += msgLength - intSize; //offset跳到消息结尾
                    continue;
                }

                int msgId = XBuffer.ReadInt(remainBuffer, ref offset);
                int msgSize = msgLength - intSize * 2;
                if(msgSize < 0)
                {
                    logErr("消息长度不对，已经小于0了!! " + msgSize);
                    break;
                }
                if(msgSize == 0)
                    continue;

                //解析协议
                if(isZip)
                {
                    var content = unZip(msgId, remainBuffer, offset, msgSize);

                    BaseMessage msg = null;
                    if (MsgFactory != null)
                        msg = MsgFactory(msgId);
                    if (msg != null)
                        msg.Read(content, 0);
#if UNITY_EDITOR
                    UnityEngine.Debug.LogWarning($"{(msg != null ? msg.GetType().FullName : "")} msgId={msgId} 数据长度解压前={(msgSize / 1024f).ToString("f2")}kb 解压后={(content.Length / 1024f).ToString("f2")}kb");
#endif
                    msgQueue.Enqueue(new RMessage() { MsgId = msgId, ByteContent = content, Msg = msg });
                }
                else
                {
                    var content = NetBufferPool.Alloc(msgSize);
                    Array.Copy(remainBuffer, offset, content, 0, msgSize);

                    BaseMessage msg = null;
                    if (MsgFactory != null)
                        msg = MsgFactory(msgId);
                    if (msg != null)
                        msg.Read(content, 0);
                    msgQueue.Enqueue(new RMessage() { MsgId = msgId, ByteContent = content, Msg = msg });
                }
                //offset跳到消息结尾
                offset += msgLength - intSize * 2;
            }

            if(offset > 0)
            {
                //剩余的数据移到remainBuffer开头
                var arr = NetBufferPool.Alloc(remainLength);
                Array.Copy(remainBuffer, offset, arr, 0, remainLength);
                Array.Copy(arr, remainBuffer, remainLength);
                NetBufferPool.Free(arr);
            }
        }

        byte[] unZip(int msgId, byte[] before, int offset, int msgSize)
        {
            try
            {
                if (before == null)
                    return null;
                using (MemoryStream ms = new MemoryStream(before, offset, msgSize))
                {
                    using (ZipInputStream zipStream = new ZipInputStream(ms))
                    {
                        zipStream.IsStreamOwner = true;
                        var file = zipStream.GetNextEntry();
                        var after = NetBufferPool.Alloc((int)file.Size);
                        zipStream.Read(after, 0, (int)file.Size);
                        return after;
                    }
                }
            }
            catch (Exception e)
            {
                logErr($"消息解压失败>{msgId}\n{e.ToString()}");
            }
            return null;
        }

        private void logErr(string str)
        {
            UnityEngine.Debug.LogError(str);
        }
    }
}
