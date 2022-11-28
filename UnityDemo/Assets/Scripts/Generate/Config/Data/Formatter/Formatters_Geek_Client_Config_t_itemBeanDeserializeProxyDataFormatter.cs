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

namespace Formatters.Geek.Client.Config
{
    public sealed class t_itemBeanDeserializeProxyDataFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Geek.Client.Config.t_itemBeanDeserializeProxyData>
    {
        // t_id
        private static global::System.ReadOnlySpan<byte> GetSpan_t_id() => new byte[1 + 4] { 164, 116, 95, 105, 100 };
        // t_name
        private static global::System.ReadOnlySpan<byte> GetSpan_t_name() => new byte[1 + 6] { 166, 116, 95, 110, 97, 109, 101 };
        // t_can_sell
        private static global::System.ReadOnlySpan<byte> GetSpan_t_can_sell() => new byte[1 + 10] { 170, 116, 95, 99, 97, 110, 95, 115, 101, 108, 108 };
        // t_show
        private static global::System.ReadOnlySpan<byte> GetSpan_t_show() => new byte[1 + 6] { 166, 116, 95, 115, 104, 111, 119 };
        // t_sell_num
        private static global::System.ReadOnlySpan<byte> GetSpan_t_sell_num() => new byte[1 + 10] { 170, 116, 95, 115, 101, 108, 108, 95, 110, 117, 109 };
        // t_desc
        private static global::System.ReadOnlySpan<byte> GetSpan_t_desc() => new byte[1 + 6] { 166, 116, 95, 100, 101, 115, 99 };
        // t_use_type
        private static global::System.ReadOnlySpan<byte> GetSpan_t_use_type() => new byte[1 + 10] { 170, 116, 95, 117, 115, 101, 95, 116, 121, 112, 101 };
        // t_param
        private static global::System.ReadOnlySpan<byte> GetSpan_t_param() => new byte[1 + 7] { 167, 116, 95, 112, 97, 114, 97, 109 };

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::Geek.Client.Config.t_itemBeanDeserializeProxyData value, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNil();
                return;
            }

            var formatterResolver = options.Resolver;
            writer.WriteMapHeader(8);
            writer.WriteRaw(GetSpan_t_id());
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<int>>(formatterResolver).Serialize(ref writer, value.t_id, options);
            writer.WriteRaw(GetSpan_t_name());
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>(formatterResolver).Serialize(ref writer, value.t_name, options);
            writer.WriteRaw(GetSpan_t_can_sell());
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<int>>(formatterResolver).Serialize(ref writer, value.t_can_sell, options);
            writer.WriteRaw(GetSpan_t_show());
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<int>>(formatterResolver).Serialize(ref writer, value.t_show, options);
            writer.WriteRaw(GetSpan_t_sell_num());
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>(formatterResolver).Serialize(ref writer, value.t_sell_num, options);
            writer.WriteRaw(GetSpan_t_desc());
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>(formatterResolver).Serialize(ref writer, value.t_desc, options);
            writer.WriteRaw(GetSpan_t_use_type());
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<int>>(formatterResolver).Serialize(ref writer, value.t_use_type, options);
            writer.WriteRaw(GetSpan_t_param());
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>(formatterResolver).Serialize(ref writer, value.t_param, options);
        }

        public global::Geek.Client.Config.t_itemBeanDeserializeProxyData Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            options.Security.DepthStep(ref reader);
            var formatterResolver = options.Resolver;
            var length = reader.ReadMapHeader();
            var ____result = new global::Geek.Client.Config.t_itemBeanDeserializeProxyData();

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
                        if (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey) != 1684627316UL) { goto FAIL; }

                        ____result.t_id = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<int>>(formatterResolver).Deserialize(ref reader, options);
                        continue;
                    case 6:
                        switch (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey))
                        {
                            default: goto FAIL;
                            case 111520460463988UL:
                                ____result.t_name = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>(formatterResolver).Deserialize(ref reader, options);
                                continue;
                            case 131320377466740UL:
                                ____result.t_show = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<int>>(formatterResolver).Deserialize(ref reader, options);
                                continue;
                            case 109347273465716UL:
                                ____result.t_desc = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>(formatterResolver).Deserialize(ref reader, options);
                                continue;
                        }
                    case 10:
                        switch (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey))
                        {
                            default: goto FAIL;
                            case 7310291547837390708UL:
                                if (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey) != 27756UL) { goto FAIL; }

                                ____result.t_can_sell = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<int>>(formatterResolver).Deserialize(ref reader, options);
                                continue;

                            case 7953194679773912948UL:
                                if (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey) != 28021UL) { goto FAIL; }

                                ____result.t_sell_num = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>(formatterResolver).Deserialize(ref reader, options);
                                continue;

                            case 8751724865221123956UL:
                                if (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey) != 25968UL) { goto FAIL; }

                                ____result.t_use_type = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<int>>(formatterResolver).Deserialize(ref reader, options);
                                continue;

                        }
                    case 7:
                        if (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey) != 30787916350381940UL) { goto FAIL; }

                        ____result.t_param = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>(formatterResolver).Deserialize(ref reader, options);
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