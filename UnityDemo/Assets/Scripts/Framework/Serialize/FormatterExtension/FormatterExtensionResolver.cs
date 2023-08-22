using System;
using MessagePack;
using MessagePack.Formatters;

namespace FormatterExtension
{
    public class FormatterExtensionResolver : IFormatterResolver
    {
        public static readonly FormatterExtensionResolver Instance = new FormatterExtensionResolver();
        private FormatterExtensionResolver()
        {
        }

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            if (typeof(T) == typeof(DateTime))
                return LocalDateTimeFormatter.Instance as IMessagePackFormatter<T>;
            return null;
        }
    }
}
