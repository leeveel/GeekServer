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
using NLog;
using System;
using System.IO;
using System.Threading;
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
    public class ClientEncoder : MessageToByteEncoder<SMessage>
    {
        private static readonly NLog.Logger LOGGER = LogManager.GetCurrentClassLogger();

        int Magic = 258;
        /// <summary>
        /// 编码消息
        /// </summary>
        protected override void Encode(IChannelHandlerContext context, SMessage msg, IByteBuffer output)
        {
            byte[] msgData = msg.Data;
            int len = 4 + 8 + 4 + 4 + msgData.Length;

            Interlocked.Increment(ref Magic);
            int magic = Magic;
            magic ^= (0xFE98 << 8);
            magic ^= len;

            output.WriteInt(len);
            output.WriteLong(DateTime.Now.Ticks / 10000);
            output.WriteInt(magic);
            output.WriteInt(msg.Id);
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

