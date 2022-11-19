using Geek.Server.Core.Serialize.PolymorphicMessagePack;
using MessagePack;
using MessagePack.Resolvers;

namespace Geek.Server.Proto
{
    public partial class PolymorphicRegister
    {
        static bool serializerRegistered = false;
        private static PolymorphicMessagePackSettings settings;
        /// <summary>
        /// 为True时需要把Geek.MsgPackTool配置中的gen-first打开
        /// 建议使用 GeneratedFirst = false
        /// </summary>
        private static bool GeneratedFirst = false;
        private static void Init()
        {
            if (!serializerRegistered)
            {
                if (GeneratedFirst)
                {
                    StaticCompositeResolver.Instance.Register(
                       /**GeneratedFirst 为true打开此注释**/
                       //MessagePack.Resolvers.GeneratedResolver.Instance, 
                       MessagePack.Resolvers.BuiltinResolver.Instance
                    );
                    settings = new PolymorphicMessagePackSettings(StaticCompositeResolver.Instance);
                }
                else
                {
                    settings = new PolymorphicMessagePackSettings(StandardResolver.Instance);
                }
                var options = new PolymorphicMessagePackSerializerOptions(settings);
                MessagePackSerializer.DefaultOptions = options;
                serializerRegistered = true;
            }
        }

        public static void Load() { }
    }
}
