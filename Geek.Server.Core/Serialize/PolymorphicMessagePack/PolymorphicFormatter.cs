using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PolymorphicMessagePack
{

    public class PolymorphicFormatter<T> : IMessagePackFormatter<T>
    {
        private object lockObj = new object();

        public PolymorphicFormatter()
        {
        }

        public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
        {

            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            //Could remove this if the settings were part of the regular options 
            if (!(options is PolymorphicMessagePackSerializerOptions polyOptions))
                throw new ArgumentException($"You cannot use a { nameof(PolymorphicResolver) } without also using { nameof(PolymorphicMessagePackSerializerOptions) }", nameof(options));

            var actualtype = value.GetType();

            if (!polyOptions.PolymorphicSettings.TypeToId.TryGetValue(actualtype, out var typeId))
                throw new MessagePackSerializationException($"Type '{ actualtype.FullName }' is not registered in { nameof(PolymorphicMessagePackSerializerOptions) }");

            writer.WriteArrayHeader(2);
            writer.WriteInt32(typeId);

            //Bottleneck
            polyOptions.PolymorphicResolver.InnerSerialize(actualtype, ref writer, value, options);
        }

        public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {

            if (reader.TryReadNil())
                return default;


            //Could remove this if the settings were part of the regular options 
            if (!(options is PolymorphicMessagePackSerializerOptions polyOptions))
                throw new ArgumentException($"You cannot use a { nameof(PolymorphicResolver) } without also using { nameof(PolymorphicMessagePackSerializerOptions) }", nameof(options));

            options.Security.DepthStep(ref reader);

            try
            {
                var count = reader.ReadArrayHeader();

                if (count != 2)
                    throw new MessagePackSerializationException("Invalid polymorphic array count");

                var typeId = reader.ReadInt32();

                if (!polyOptions.PolymorphicSettings.IdToType.TryGetValue(typeId, out var type))
                    throw new MessagePackSerializationException($"Cannot find Type Id: { typeId } registered in { nameof(PolymorphicMessagePackSerializerOptions) }");

                //Bottleneck
                return (T)polyOptions.PolymorphicResolver.InnerDeserialize(type, ref reader, options);
            }
            finally
            {
                reader.Depth--;
            }

        }

    }

}