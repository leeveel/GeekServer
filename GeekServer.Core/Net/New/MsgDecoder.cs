using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Connections;
using System;
using System.Buffers;
using System.IO;

namespace Geek.Server
{
    public static class MsgDecoder
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public const string LAST_RECV_ORDER = "LAST_RECV_ORDER";

        public const string LAST_RECV_TIME = "LAST_RECV_TIME";

        /// 从客户端接收的包大小最大值（单位：字节 1M）
        const int MAX_RECV_SIZE = 1024 * 1024;


        public static byte[] UnGZip(int msgId, byte[] before, int offset, int msgSize)
        {
            try
            {
                if (before == null)
                    return before;
                using (MemoryStream ms = new MemoryStream(before, offset, msgSize))
                {
                    using (ZipInputStream zipStream = new ZipInputStream(ms))
                    {
                        zipStream.IsStreamOwner = true;
                        var file = zipStream.GetNextEntry();
                        var after = ArrayPool<byte>.Shared.Rent((int)file.Size);
                        zipStream.Read(after, 0, (int)file.Size);
                        //Console.WriteLine($"unzip:{file.Size}");
                        return after;
                    }
                }
            }
            catch (Exception e)
            {
                LOGGER.Error($"消息解压失败>{msgId}\n{e.ToString()}");
                throw;
            }
        }


        public static byte[] CompressGZip(byte[] buffer, int realLength=0)
        {
            try
            {
                if (realLength <= 0)
                    realLength = buffer.Length;
                using MemoryStream ms = new MemoryStream();
                using ZipOutputStream compressedzipStream = new ZipOutputStream(ms);
                var entry = new ZipEntry("m");
                entry.Size = realLength;
                compressedzipStream.PutNextEntry(entry);
                compressedzipStream.Write(buffer, 0, realLength);
                compressedzipStream.CloseEntry();
                compressedzipStream.Close();
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                LOGGER.Error("数据压缩失败{}", ex.Message);
                return buffer;
            }
        }


        public static BaseMessage ClientDecode(NMessage message)
        {
            var reader = new SequenceReader<byte>(message.Payload);

            //数据包长度+消息ID=两个int=8位
            if (message.Payload.Length < 4)
                return null;

            //消息id
            reader.TryReadBigEndian(out int msgId);

            var msg = TcpHandlerFactory.GetMsg(msgId);
            if (msg == null)
            {
                LOGGER.Error("消息ID:{} 找不到对应的Msg.", msgId);
                return null;
            }

            if (msg.MsgId == msgId)
            {
                var unread = reader.UnreadSequence;
                var unreadLen = (int)unread.Length;
                var msgData = unread.ToPooledArray(); //获取池化的array (TODO:让协议直接支持读取ReadOnlySequence，减少拷贝)
                if (message.Ziped)
                {
                    var temp = msgData;
                    msgData = UnGZip(msgId, msgData, 0, unreadLen);
                    ArrayPool<byte>.Shared.Return(temp); //归还压缩之前的数据
                }
                msg.Deserialize(msgData.AsSpan());
                ArrayPool<byte>.Shared.Return(msgData); //归还
                return msg;
            }
            else
            {
                LOGGER.Error("后台解析消息失败，注册消息id和消息无法对应.real:{0}, register:{1}", msg.MsgId, msgId);
                return null;
            }
        }

        public static BaseMessage Decode(ConnectionContext context, NMessage msg)
        {
            var reader = new SequenceReader<byte>(msg.Payload);
            
            int msgLen = (int)msg.Payload.Length;  //4
            if (!CheckMsgLen(msgLen))
            {
                context.Abort();
                return null;
            }

            reader.TryReadBigEndian(out long time); //8
            if (!CheckTime(context, time))
            {
                context.Abort();
                return null;
            }

            reader.TryReadBigEndian(out int order);  //4
            if (!CheckMagicNumber(context, order, msgLen))
            {
                context.Abort();
                return null;
            }

            reader.TryReadBigEndian(out int msgId);  //4

            //需要优化，让协议支持直接从SequenceReader读取, 避免额外分配/拷贝
            var unread = reader.UnreadSequence;
            var unreadLen = (int)unread.Length;
            var msgData = unread.ToPooledArray(); //获取池化的array
            //处理压缩
            if (msg.Ziped)
            {
                var temp = msgData;
                msgData = UnGZip(msgId, msgData, 0, unreadLen);
                ArrayPool<byte>.Shared.Return(temp); //归还压缩之前的数据
            }

            var protoMsg = TcpHandlerFactory.GetMsg(msgId);
            if (protoMsg == null)
            {
                LOGGER.Error($"消息ID:{msgId} 找不到对应的Msg");
                return null;
            }
            else
            {
                if (protoMsg.MsgId == msgId)
                {
                    protoMsg.Deserialize(msgData.AsSpan());
                    ArrayPool<byte>.Shared.Return(msgData); //归还
                }
                else
                {
                    LOGGER.Error($"后台解析消息失败，注册消息id和消息无法对应.real:{protoMsg.MsgId}, register:{msgId}");
                    return null;
                }
            }

            return protoMsg;
        }


        public static bool CheckMagicNumber(ConnectionContext context, int order, int msgLen)
        {
            order ^= (0x1234 << 8);
            order ^= msgLen+4;
            context.Items.TryGetValue(LAST_RECV_ORDER, out object objOrder);
            if (objOrder != null)
            {
                int lastOrder = (int)objOrder;
                if (order != lastOrder + 1)
                {
                    LOGGER.Error("包序列出错, order=" + order + ", lastOrder=" + lastOrder);
                    return false;
                }
            }
            context.Items[LAST_RECV_ORDER] = order;
            return true;
        }

        /// <summary>
        /// 检查消息长度是否合法
        /// </summary>
        /// <param name="msgLen"></param>
        /// <returns></returns>
        public static bool CheckMsgLen(int msgLen)
        {
            //消息长度+时间戳+magic+消息id+数据
            //4 + 8 + 4 + 4 + data
            if (msgLen <= 16)//(消息长度已经被读取)
            {
                LOGGER.Error("从客户端接收的包大小异常:" + msgLen + ":至少16个字节");
                return false;
            }
            else if (msgLen > MAX_RECV_SIZE)
            {
                LOGGER.Error("从客户端接收的包大小超过限制：" + msgLen + "字节，最大值：" + MAX_RECV_SIZE / 1024 + "字节");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 时间戳检查(可以防止客户端游戏过程中修改时间)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static bool CheckTime(ConnectionContext context, long time)
        {
            context.Items.TryGetValue(LAST_RECV_TIME, out object objTime);
            if (objTime != null)
            {
                long lastTime = (long)objTime;
                if (lastTime > time)
                {
                    LOGGER.Error("时间戳出错，time=" + time + ", lastTime=" + lastTime + " " + context);
                    return false;
                }
            }
            context.Items[LAST_RECV_TIME] = time;
            return true;
        }

    }
}
