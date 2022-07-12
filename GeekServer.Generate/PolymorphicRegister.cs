using MessagePack;
using MessagePack.Resolvers;
using PolymorphicMessagePack;

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
                settings = new PolymorphicMessagePackSettings(StandardResolver.Instance);
                var options = new PolymorphicMessagePackSerializerOptions(settings);
                MessagePackSerializer.DefaultOptions = options;
                serializerRegistered = true;
            }
        }

        public static void Load() { }
    }
}
