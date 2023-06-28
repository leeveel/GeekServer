using MessagePack.Formatters;
using MessagePack;
using System.Diagnostics;
using System;

namespace FormatterExtension
{
    public class LocalDateTimeFormatter : IMessagePackFormatter<DateTime>
    {
        public static readonly LocalDateTimeFormatter Instance = new LocalDateTimeFormatter();
        private LocalDateTimeFormatter() { }

        public DateTime Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.NextMessagePackType == MessagePackType.Array)
            {
                var count = reader.ReadArrayHeader();
                if (count != 2)
                    throw new MessagePackSerializationException("Invalid polymorphic array count");
                var kind = (DateTimeKind)reader.ReadByte();
                return new DateTime(reader.ReadDateTime().Ticks, kind);
            }
            else
            {
                //本地时间
                return new DateTime(reader.ReadDateTime().Ticks, DateTimeKind.Local);
            }
        }

        public void Serialize(ref MessagePackWriter writer, DateTime value, MessagePackSerializerOptions options)
        {
            if (value.Kind == DateTimeKind.Local)
            {
                //这里写入的时候 加个时间偏移,因为messagepack会转成utc，这里相当于做个抵消，同时也是为了ui预览的时候能看到正确时间
                writer.Write(value.Add(TimeZoneInfo.Local.BaseUtcOffset));
            }
            else
            {
                //如果是非local时间，需要加额外标志
                writer.WriteArrayHeader(2);
                writer.Write((byte)value.Kind);
                writer.Write(value);
            }
        }
    }
}
