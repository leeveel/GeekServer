// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY MPC(MessagePack-CSharp). DO NOT CHANGE IT.
// </auto-generated>

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168
#pragma warning disable CS1591 // document public APIs

#pragma warning disable SA1129 // Do not use default value type constructor
#pragma warning disable SA1309 // Field names should not begin with underscore
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1403 // File may only contain a single namespace
#pragma warning disable SA1649 // File name should match first type name

namespace MessagePack.Formatters.Geek.Server.Proto
{
    public sealed class BFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Geek.Server.Proto.B>
    {
        // Name
        private static global::System.ReadOnlySpan<byte> GetSpan_Name() => new byte[1 + 4] { 164, 78, 97, 109, 101 };
        // Age
        private static global::System.ReadOnlySpan<byte> GetSpan_Age() => new byte[1 + 3] { 163, 65, 103, 101 };
        // E
        private static global::System.ReadOnlySpan<byte> GetSpan_E() => new byte[1 + 1] { 161, 69 };
        // TS
        private static global::System.ReadOnlySpan<byte> GetSpan_TS() => new byte[1 + 2] { 162, 84, 83 };

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::Geek.Server.Proto.B value, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNil();
                return;
            }

            var formatterResolver = options.Resolver;
            writer.WriteMapHeader(4);
            writer.WriteRaw(GetSpan_Name());
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<string>(formatterResolver).Serialize(ref writer, value.Name, options);
            writer.WriteRaw(GetSpan_Age());
            writer.Write(value.Age);
            writer.WriteRaw(GetSpan_E());
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::Geek.Server.Proto.TestEnum>(formatterResolver).Serialize(ref writer, value.E, options);
            writer.WriteRaw(GetSpan_TS());
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::Geek.Server.Proto.TestStruct>(formatterResolver).Serialize(ref writer, value.TS, options);
        }

        public global::Geek.Server.Proto.B Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            options.Security.DepthStep(ref reader);
            var formatterResolver = options.Resolver;
            var length = reader.ReadMapHeader();
            var ____result = new global::Geek.Server.Proto.B();

            for (int i = 0; i < length; i++)
            {
                var stringKey = global::MessagePack.Internal.CodeGenHelpers.ReadStringSpan(ref reader);
                switch (stringKey.Length)
                {
                    default:
                    FAIL:
                      reader.Skip();
                      continue;
                    case 4:
                        if (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey) != 1701667150UL) { goto FAIL; }

                        ____result.Name = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<string>(formatterResolver).Deserialize(ref reader, options);
                        continue;
                    case 3:
                        if (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey) != 6645569UL) { goto FAIL; }

                        ____result.Age = reader.ReadInt32();
                        continue;
                    case 1:
                        if (stringKey[0] != 69) { goto FAIL; }

                        ____result.E = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::Geek.Server.Proto.TestEnum>(formatterResolver).Deserialize(ref reader, options);
                        continue;
                    case 2:
                        if (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey) != 21332UL) { goto FAIL; }

                        ____result.TS = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::Geek.Server.Proto.TestStruct>(formatterResolver).Deserialize(ref reader, options);
                        continue;

                }
            }

            reader.Depth--;
            return ____result;
        }
    }

}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning restore SA1129 // Do not use default value type constructor
#pragma warning restore SA1309 // Field names should not begin with underscore
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning restore SA1403 // File may only contain a single namespace
#pragma warning restore SA1649 // File name should match first type name