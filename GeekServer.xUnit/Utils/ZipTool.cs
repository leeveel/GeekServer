using ICSharpCode.SharpZipLib.Zip;
using System.Buffers;

namespace Geek.Server.xUnit
{
    public static class ZipTool
    {

        public static byte[] UnGZip(byte[] before, int offset, int msgSize)
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
                        //var after = NetBufferPool.Alloc((int)file.Size);
                        var after = ArrayPool<byte>.Shared.Rent((int)file.Size);
                        zipStream.Read(after, 0, (int)file.Size);
                        //Logger.WriteLine($"{file.Size}-{file.CompressedSize}");
                        return after;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.WriteLine($"消息解压失败>{e}");
            }
            return null;
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
                Logger.WriteLine($"数据压缩失败{ex.Message}");
                return rawData;
            }
        }

    }
}
