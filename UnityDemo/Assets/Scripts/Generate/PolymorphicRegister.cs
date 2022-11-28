using MessagePack;
using MessagePack.Resolvers;
using PolymorphicMessagePack;
using Resolvers;

namespace Geek.Server.Proto
{
    public partial class PolymorphicRegister
    {

        static bool serializerRegistered = false;
        private static PolymorphicMessagePackSettings settings;
        private static void Init()
        {
            if (!serializerRegistered)
            {
                StaticCompositeResolver.Instance.Register(
                    MessagePack.Resolvers.GeneratedResolver.Instance,
                    MessagePack.Resolvers.BuiltinResolver.Instance,
                    ConfigDataResolver.Instance
                );

                settings = new PolymorphicMessagePackSettings(StaticCompositeResolver.Instance);
                var options = new PolymorphicMessagePackSerializerOptions(settings);
                MessagePackSerializer.DefaultOptions = options;
                serializerRegistered = true;
            }
        }

        public static void Load() { }
    }
}
