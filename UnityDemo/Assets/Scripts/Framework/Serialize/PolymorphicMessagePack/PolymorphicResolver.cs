using MessagePack;
using MessagePack.Formatters;
using System;
using System.Collections.Concurrent;

namespace PolymorphicMessagePack
{

    internal sealed class PolymorphicResolver : IFormatterResolver
    {
        private PolymorphicMessagePackSettings _polymorphicSettings;
        private readonly ConcurrentDictionary<Type, PolymorphicDelegate> _innerFormatterCache;
        public PolymorphicResolver(PolymorphicMessagePackSettings polymorphicSettings)
        {
            _polymorphicSettings = polymorphicSettings;
            _innerFormatterCache = new ConcurrentDictionary<Type, PolymorphicDelegate>();
        }

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            //Have to check the type here, because of two reasons:
            //1. Deserialize will not work with non-polymorphic types, as it assumes a typeid as the first item in a two part array, the second being the object itself
            //2. We need the polymorphic settings, and they are an instance and are required to be.

            //If i had the object to be serialized or its actual type, I could make this a lot more efficient and remove the need for the Polymorphic delegate.

            //Can something be optimized here?
            if (_polymorphicSettings.BaseTypes.Contains(typeof(T)) ||
                _polymorphicSettings.TypeToId.ContainsKey(typeof(T)))
            {
                return FormatterCache<T>.Formatter;
            }
            else if (_polymorphicSettings.SerializeOnlyRegisteredTypes)
            {
                throw new MessagePackSerializationException($"Type '{ typeof(T).FullName }' is not registered in the { nameof(PolymorphicMessagePackSettings) } and { nameof(PolymorphicMessagePackSettings.SerializeOnlyRegisteredTypes) } is set to true");
            }

            return _polymorphicSettings.InnerResolver.GetFormatter<T>();
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

                ploymorphicDeletegate = (PolymorphicDelegate)Activator.CreateInstance(constructedType, _polymorphicSettings.InnerResolver);

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