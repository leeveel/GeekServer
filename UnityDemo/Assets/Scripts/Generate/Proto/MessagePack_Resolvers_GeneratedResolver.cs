// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY MPC(MessagePack-CSharp). DO NOT CHANGE IT.
// </auto-generated>

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168
#pragma warning disable CS1591 // document public APIs

#pragma warning disable SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1649 // File name should match first type name

namespace MessagePack.Resolvers
{
    public class GeneratedResolver : global::MessagePack.IFormatterResolver
    {
        public static readonly global::MessagePack.IFormatterResolver Instance = new GeneratedResolver();

        private GeneratedResolver()
        {
        }

        public global::MessagePack.Formatters.IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }

        private static class FormatterCache<T>
        {
            internal static readonly global::MessagePack.Formatters.IMessagePackFormatter<T> Formatter;

            static FormatterCache()
            {
                var f = GeneratedResolverGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    Formatter = (global::MessagePack.Formatters.IMessagePackFormatter<T>)f;
                }
            }
        }
    }

    internal static class GeneratedResolverGetFormatterHelper
    {
        private static readonly global::System.Collections.Generic.Dictionary<global::System.Type, int> lookup;

        static GeneratedResolverGetFormatterHelper()
        {
            lookup = new global::System.Collections.Generic.Dictionary<global::System.Type, int>(21)
            {
                { typeof(global::System.Collections.Generic.Dictionary<int, long>), 0 },
                { typeof(global::Geek.Server.Proto.TestEnum), 1 },
                { typeof(global::ClientProto.NetConnectMessage), 2 },
                { typeof(global::ClientProto.NetDisConnectMessage), 3 },
                { typeof(global::Geek.Server.Proto.A), 4 },
                { typeof(global::Geek.Server.Proto.B), 5 },
                { typeof(global::Geek.Server.Proto.HearBeat), 6 },
                { typeof(global::Geek.Server.Proto.ReqBagInfo), 7 },
                { typeof(global::Geek.Server.Proto.ReqComposePet), 8 },
                { typeof(global::Geek.Server.Proto.ReqLogin), 9 },
                { typeof(global::Geek.Server.Proto.ReqSellItem), 10 },
                { typeof(global::Geek.Server.Proto.ReqUseItem), 11 },
                { typeof(global::Geek.Server.Proto.ResBagInfo), 12 },
                { typeof(global::Geek.Server.Proto.ResComposePet), 13 },
                { typeof(global::Geek.Server.Proto.ResErrorCode), 14 },
                { typeof(global::Geek.Server.Proto.ResItemChange), 15 },
                { typeof(global::Geek.Server.Proto.ResLevelUp), 16 },
                { typeof(global::Geek.Server.Proto.ResLogin), 17 },
                { typeof(global::Geek.Server.Proto.ResPrompt), 18 },
                { typeof(global::Geek.Server.Proto.TestStruct), 19 },
                { typeof(global::Geek.Server.Proto.UserInfo), 20 },
            };
        }

        internal static object GetFormatter(global::System.Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key))
            {
                return null;
            }

            switch (key)
            {
                case 0: return new global::MessagePack.Formatters.DictionaryFormatter<int, long>();
                case 1: return new MessagePack.Formatters.Geek.Server.Proto.TestEnumFormatter();
                case 2: return new MessagePack.Formatters.ClientProto.NetConnectMessageFormatter();
                case 3: return new MessagePack.Formatters.ClientProto.NetDisConnectMessageFormatter();
                case 4: return new MessagePack.Formatters.Geek.Server.Proto.AFormatter();
                case 5: return new MessagePack.Formatters.Geek.Server.Proto.BFormatter();
                case 6: return new MessagePack.Formatters.Geek.Server.Proto.HearBeatFormatter();
                case 7: return new MessagePack.Formatters.Geek.Server.Proto.ReqBagInfoFormatter();
                case 8: return new MessagePack.Formatters.Geek.Server.Proto.ReqComposePetFormatter();
                case 9: return new MessagePack.Formatters.Geek.Server.Proto.ReqLoginFormatter();
                case 10: return new MessagePack.Formatters.Geek.Server.Proto.ReqSellItemFormatter();
                case 11: return new MessagePack.Formatters.Geek.Server.Proto.ReqUseItemFormatter();
                case 12: return new MessagePack.Formatters.Geek.Server.Proto.ResBagInfoFormatter();
                case 13: return new MessagePack.Formatters.Geek.Server.Proto.ResComposePetFormatter();
                case 14: return new MessagePack.Formatters.Geek.Server.Proto.ResErrorCodeFormatter();
                case 15: return new MessagePack.Formatters.Geek.Server.Proto.ResItemChangeFormatter();
                case 16: return new MessagePack.Formatters.Geek.Server.Proto.ResLevelUpFormatter();
                case 17: return new MessagePack.Formatters.Geek.Server.Proto.ResLoginFormatter();
                case 18: return new MessagePack.Formatters.Geek.Server.Proto.ResPromptFormatter();
                case 19: return new MessagePack.Formatters.Geek.Server.Proto.TestStructFormatter();
                case 20: return new MessagePack.Formatters.Geek.Server.Proto.UserInfoFormatter();
                default: return null;
            }
        }
    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning restore SA1649 // File name should match first type name
