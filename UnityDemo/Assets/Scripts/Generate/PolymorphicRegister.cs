
using PolymorphicMessagePack;
using Resolvers;

namespace Geek.Server.Proto //这里名字必须要与PolymorphicRegisterGen的命名空间完全一样
{
    public partial class PolymorphicRegister
    {

        static bool serializerRegistered = false;
        private static void Init()
        {
            if (!serializerRegistered)
            {
                PolymorphicResolver.AddInnerResolver(ConfigDataResolver.Instance);
                PolymorphicResolver.AddInnerResolver(MessagePack.Resolvers.GeneratedResolver.Instance);
                PolymorphicTypeMapper.Register<Message>();
                PolymorphicResolver.Instance.Init();
                serializerRegistered = true;
            }
        }

        public static void Load() { Init(); }
    }
}
