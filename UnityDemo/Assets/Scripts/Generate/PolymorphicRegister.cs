using MessagePack;
using MessagePack.Resolvers;
using PolymorphicMessagePack;
using Resolvers;

namespace Geek.Server.Proto
{
    public partial class PolymorphicRegister
    {

        static bool serializerRegistered = false;
        private static void Init()
        {
            if (!serializerRegistered)
            {
                PolymorphicResolver.AddInnerResolver(ConfigDataResolver.Instance);
                PolymorphicResolver.AddInnerResolver(GeneratedResolver.Instance);
                PolymorphicTypeMapper.Register<Message>();
                PolymorphicResolver.Instance.Init();
                serializerRegistered = true;
            }
        }

        public static void Load() { Init(); }
    }
}
