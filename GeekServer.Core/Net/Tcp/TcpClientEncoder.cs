using NLog;
using System;
using System.IO;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using ICSharpCode.SharpZipLib.Zip;

namespace Geek.Server
{
    /// <summary>
    /// 消息编码
    /// </summary>
    public class TcpClientEncoder : MessageToByteEncoder<NMessage>
    {
        private static readonly NLog.Logger LOGGER = LogManager.GetCurrentClassLogger();

        private const int Magic = 0x1234;
        int count = 0;
        /// <summary>
        /// 编码消息
        /// </summary>
        protected override void Encode(IChannelHandlerContext context, NMessage msg, IByteBuffer output)
        {
            byte[] msgData = msg.Data;
            int len = 4 + 8 + 4 + 4 + msgData.Length;

            int magic = Magic + count++;
            magic ^= Magic << 8;
            magic ^= len;

            output.WriteInt(len);
            output.WriteLong(DateTime.Now.Ticks / 10000);
            output.WriteInt(magic);
            output.WriteInt(msg.MsgId);
            output.WriteBytes(msgData);
        }

        public static byte[] CompressGZip(byte[] rawData)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (ZipOutputStream compressedzipStream = new ZipOutputStream(ms))
                    {
                        var entry = new ZipEntry("m");
                        entry.Size = rawData.Length;
                        compressedzipStream.PutNextEntry(entry);
                        compressedzipStream.Write(rawData, 0, rawData.Length);
                        compressedzipStream.CloseEntry();
                        compressedzipStream.Close();
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                LOGGER.Error("数据压缩失败{}", ex.Message);
                return rawData;
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception e)
        {
            LOGGER.Error(e, "编码消息出现错误 {}", e.Message);
            context.CloseAsync();
        }

    }
}

