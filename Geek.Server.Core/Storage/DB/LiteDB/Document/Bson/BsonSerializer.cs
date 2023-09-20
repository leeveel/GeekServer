using LiteDB.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace LiteDB
{
    /// <summary>
    /// Class to call method for convert BsonDocument to/from byte[] - based on http://bsonspec.org/spec.html
    /// In v5 this class use new BufferRead/Writer to work with byte[] segments. This class are just a shortchut
    /// </summary>
    public class BsonSerializer
    {
        /// <summary>
        /// Serialize BsonDocument into a binary array
        /// </summary>
        public static byte[] Serialize(BsonDocument doc, bool compress = false)
        {
            if (doc == null) throw new ArgumentNullException(nameof(doc));

            var buffer = new byte[doc.GetBytesCount(true)];

            using (BufferWriter writer = new(buffer))
            {
                writer.WriteDocument(doc, false);
            }

            if (compress)
            {
                using var memory = new MemoryStream();
                using (GZipStream gzip = new(memory, CompressionMode.Compress))
                {
                    gzip.Write(buffer, 0, buffer.Length);
                }
                return memory.ToArray();
            }

            return buffer;
        }

        /// <summary>
        /// Deserialize binary data into BsonDocument
        /// </summary>
        public static BsonDocument Deserialize(byte[] buffer, bool compress = false, HashSet<string> fields = null)
        {
            if (buffer == null || buffer.Length == 0) throw new ArgumentNullException(nameof(buffer));

            if (compress)
            {
                using var compressStream = new MemoryStream(buffer);
                using var zipStream = new GZipStream(compressStream, CompressionMode.Decompress);
                using var resultStream = new MemoryStream();
                zipStream.CopyTo(resultStream);
                buffer = resultStream.ToArray();
            } 
            using var reader = new BufferReader(buffer, false);
            return reader.ReadDocument(fields);
        }
    }
}