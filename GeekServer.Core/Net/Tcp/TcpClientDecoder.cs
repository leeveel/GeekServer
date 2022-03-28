using System;
using System.IO;
using DotNetty.Buffers;
using DotNetty.Codecs;
using System.Collections.Generic;
using DotNetty.Transport.Channels;
using ICSharpCode.SharpZipLib.Zip;
using NLog;

namespace Geek.Server
{
    /// <summary>
    /// 消息解码器
    /// </summary>
    public class TcpClientDecoder : ByteToMessageDecoder
    {
        private static readonly NLog.Logger LOGGER = LogManager.GetCurrentClassLogger();
        const int MSG_HEAD_LENGTH = sizeof(int) * 2;
        /// <summary>
        /// 解析消息包
        /// </summary>
        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            try
            {
                //数据包长度消息头两个int 8位
                if (input.ReadableBytes < MSG_HEAD_LENGTH)
                    return;
                
                //仅仅是Get没有Read
                int msgLen = input.GetInt(input.ReaderIndex);
                //是否压缩
                bool isZip = msgLen < 0;
                if(msgLen < 0)
                    msgLen = -msgLen;

                if (msgLen < MSG_HEAD_LENGTH)
                {
                    LOGGER.Error("接受数据异常, 数据长度最小: " + MSG_HEAD_LENGTH + "  当前长度：" + msgLen);
                    context.CloseAsync();
                    return;
                }

                if (input.ReadableBytes < msgLen)
                    return;

                //在buffer中读取掉消息长度
                input.ReadInt();

                //消息id
                int msgId = input.ReadInt();

                byte[] msgData = null;
                if(isZip)
                {
                    msgData = new byte[msgLen - MSG_HEAD_LENGTH];
                    input.ReadBytes(msgData);
                    msgData = UnZip(msgId, msgData);
                }
                else
                {
                    //xbuffer
                    msgData = new byte[msgLen - MSG_HEAD_LENGTH];
                    input.ReadBytes(msgData);
                }

                var msg = TcpHandlerFactory.GetMsg(msgId);
                if (msg == null)
                {
                    LOGGER.Error("消息ID:{} 找不到对应的Msg.", msgId);
                    return;
                }

                if (msg.MsgId == msgId)
                {
                    msg.Deserialize(msgData);
                }
                else
                {
                    LOGGER.Error("后台解析消息失败，注册消息id和消息无法对应.real:{0}, register:{1}", msg.MsgId, msgId);
                    return;
                }
                output.Add(msg);
            }
            catch (Exception e)
            {
                LOGGER.Error(e, "解析数据异常," + e.Message + "\n" + e.StackTrace);
            }
        }

        byte[] UnZip(int msgId, byte[] before)
        {
            try
            {
                if (before == null)
                    return null;
                using MemoryStream ms = new MemoryStream(before);
                using ZipInputStream zipStream = new ZipInputStream(ms);
                zipStream.IsStreamOwner = true;
                var file = zipStream.GetNextEntry();
                var after = new byte[file.Size];
                zipStream.Read(after, 0, after.Length);
                return after;
            }
            catch (Exception e)
            {
                LOGGER.Error("消息解压失败>{}\n{}", msgId, e.ToString());
            }
            return null;
        }
    }

}

