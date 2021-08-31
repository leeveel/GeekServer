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
    public class TcpServerEncoder : MessageToByteEncoder<NMessage>
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 编码消息
        /// </summary>
        protected override void Encode(IChannelHandlerContext context, NMessage msg, IByteBuffer output)
        {
            //消息长度=消息id+数据
            //4 + data
            byte[] msgData = msg.Data;
            int len = msgData.Length;
            bool zip = false;
            if (len >= 512)
            {
                //下行消息适当压缩
                zip = true;
                msgData = CompressGZip(msgData);
                LOGGER.Debug($"msg:{msg.MsgId} zip before:{len}, after:{msgData.Length}");
            }
            len = msgData.Length + sizeof(int) * 2;
            //用于标记数据包是否压缩
            if (zip)
                len = -len;

            output.WriteInt(len);
            output.WriteInt(msg.MsgId);
            output.WriteBytes(msgData);
        }

        public static byte[] CompressGZip(byte[] rawData)
        {
            try
            {
                using MemoryStream ms = new MemoryStream();
                using ZipOutputStream compressedzipStream = new ZipOutputStream(ms);
                var entry = new ZipEntry("m");
                entry.Size = rawData.Length;
                compressedzipStream.PutNextEntry(entry);
                compressedzipStream.Write(rawData, 0, rawData.Length);
                compressedzipStream.CloseEntry();
                compressedzipStream.Close();
                return ms.ToArray();
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