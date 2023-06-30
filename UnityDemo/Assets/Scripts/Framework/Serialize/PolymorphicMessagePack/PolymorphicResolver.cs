using FormatterExtension;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PolymorphicMessagePack
{
    public sealed class PolymorphicResolver : IFormatterResolver
    {
        public static PolymorphicResolver Instance { get; private set; } = new PolymorphicResolver();

        static IFormatterResolver InnerResolver;
        static List<IFormatterResolver> innerResolver = new List<IFormatterResolver>
        {
            FormatterExtensionResolver.Instance,
            BuiltinResolver.Instance,
            StandardResolver.Instance,
            ContractlessStandardResolver.Instance
        };

        //先调用此函数注册需要的resolver，然后再调用init，比如客户端需要注册proto和配置表的resolver
        public static void AddInnerResolver(IFormatterResolver resolver, int index = 0)
        {
            if (innerResolver.IndexOf(resolver) < 0)
            {
                innerResolver.Insert(index, resolver);
            }
        }

        public void Init()
        {
            StaticCompositeResolver.Instance.Register(innerResolver.ToArray());
            InnerResolver = StaticCompositeResolver.Instance;
            MessagePackSerializer.DefaultOptions = new MessagePackSerializerOptions(PolymorphicResolver.Instance).WithCompression(MessagePackCompression.Lz4Block);
        }

        private readonly ConcurrentDictionary<Type, PolymorphicDelegate> _innerFormatterCache = new ConcurrentDictionary<Type, PolymorphicDelegate>();

        private PolymorphicResolver() { }

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            if (PolymorphicTypeMapper.Contains(typeof(T)))
            {
                return FormatterCache<T>.Formatter;
            }

            return InnerResolver.GetFormatter<T>();
        }

        public void RemoveFormatterDelegateCache(Type type)
        {
            _innerFormatterCache.TryRemove(type, out _);
        }

        //Bottleneck
        public void InnerSerialize(Type type, ref MessagePackWriter writer, object value, MessagePackSerializerOptions options)
        {
            GetDelegate(type).Serialize(ref writer, value, options);
        }

        //Bottleneck
        public object InnerDeserialize(Type type, ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            return GetDelegate(type).Deserialize(ref reader, options);
        }

        private PolymorphicDelegate GetDelegate(Type type)
        {
            if (!_innerFormatterCache.TryGetValue(type, out var ploymorphicDeletegate))
            {
                var constructedType = typeof(PolymorphicDelegate<>).MakeGenericType(type);

                ploymorphicDeletegate = (PolymorphicDelegate)Activator.CreateInstance(constructedType, InnerResolver);

                _innerFormatterCache.TryAdd(type, ploymorphicDeletegate);
            }

            return ploymorphicDeletegate;
        }

        private static class FormatterCache<T>
        {
            public static IMessagePackFormatter<T> Formatter;

            static FormatterCache()
            {
                Formatter = new PolymorphicFormatter<T>();
            }
        }
    }
}