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
using NLog;
using System.IO;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using ICSharpCode.SharpZipLib.Zip;
using Geek.Core.Net.Message;

namespace Geek.Core.Net.Codecs
{
    /// <summary>
    /// 消息编码
    /// </summary>
    public class ServerEncoder : MessageToByteEncoder<SMessage>
    {
        private static readonly NLog.Logger LOGGER = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 编码消息
        /// </summary>
        protected override void Encode(IChannelHandlerContext context, SMessage msg, IByteBuffer output)
        {
            //消息长度=消息id+数据
            //4 + data
            byte[] msgData = msg.Data;
            int len = msgData.Length;
            bool zip = false;
            if (len > 256)
            {
                zip = true;
                msgData = CompressGZip(msgData);
                LOGGER.Debug($"msg:{msg.Id} zip before:{len}, after:{msgData.Length}");
            }
            len = 4 + 4 + msgData.Length;
            //用于标记数据包是否加密
            if (zip)
                len = -len;

            output.WriteInt(len);
            output.WriteInt(msg.Id);
            output.WriteBytes(msgData);
            LogMsg(msg);
        }

        private static void LogMsg(SMessage msg)
        {
            switch (msg.Id)
            {
                case 105119:
                    LOGGER.Debug($"-------------sendMsg 解锁屏 {msg.Id}-------------");
                    break;
                case 101101:
                    //LOGGER.Info($"-------------sendMsg 背包更新 {msg.Id}-------------");
                    break;
                default:
                    break;
            }
            //LOGGER.Info($"-------------send msg {msg.Id} {msg.GetType()}-------------");
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
            //base.ExceptionCaught(context, exception);
            LOGGER.Error(e, "编码消息出现错误 {}", e.Message);
            context.CloseAsync();
        }

    }
}

