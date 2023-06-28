using MessagePack.Formatters;
using MessagePack;
using System;

namespace PolymorphicMessagePack
{
    public class LocalDateTimeFormatter : IMessagePackFormatter<DateTime>
    {
        public static readonly LocalDateTimeFormatter Instance = new LocalDateTimeFormatter();
        private LocalDateTimeFormatter() { }

        public DateTime Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            return reader.ReadDateTime();
        }

        public void Serialize(ref MessagePackWriter writer, DateTime value, MessagePackSerializerOptions options)
        {
            if (value.Kind == DateTimeKind.Utc)
            {
                writer.Write(value);
            }
            else
            {
                writer.Write(value.Add(TimeZoneInfo.Local.BaseUtcOffset));
            }
        }
    }
}