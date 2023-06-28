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

            var actualtype = value.GetType();

            if (!PolymorphicTypeMapper.TryGet(actualtype, out var typeId))
                throw new MessagePackSerializationException($"Type '{actualtype.FullName}' is not registered in {nameof(PolymorphicTypeMapper)}");


            writer.WriteArrayHeader(2);
            writer.WriteInt32(typeId);

            //Bottleneck
            (options.Resolver as PolymorphicResolver).InnerSerialize(actualtype, ref writer, value, options);
        }

        public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
                return default;

            options.Security.DepthStep(ref reader);

            try
            {
                Type type = null;
                switch (reader.NextMessagePackType)
                {
                    //如果是数组，说明里面存了类型id
                    case MessagePackType.Array:
                        var count = reader.ReadArrayHeader();
                        if (count != 2)
                            throw new MessagePackSerializationException("Invalid polymorphic array count");
                        switch (reader.NextMessagePackType)
                        {
                            case MessagePackType.Integer:
                                var typeId = reader.ReadInt32();
                                if (!PolymorphicTypeMapper.TryGet(typeId, out type))
                                    throw new MessagePackSerializationException($"Cannot find Type Id: {typeId} registered in {nameof(PolymorphicTypeMapper)}");
                                break;
                            case MessagePackType.String:
                                var typeStr = reader.ReadString();
                                if (!PolymorphicTypeMapper.TryGet(typeStr, typeof(T), out type))
                                    throw new MessagePackSerializationException($"Cannot find Type Id: {typeStr} registered in {nameof(PolymorphicTypeMapper)}");
                                break;
                        }
                        break;
                    default:
                        type = typeof(T);
                        break;
                }
                return (T)(options.Resolver as PolymorphicResolver).InnerDeserialize(type, ref reader, options);
            }
            finally
            {
                reader.Depth--;
            }
        }
    }

}