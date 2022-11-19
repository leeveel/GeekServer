using MessagePack;

namespace Geek.Server.Core.Serialize.PolymorphicMessagePack
{
    public class PolymorphicMessagePackSettings
    {
        internal readonly Dictionary<Type, int> TypeToId = new Dictionary<Type, int>();
        internal readonly Dictionary<int, Type> IdToType = new Dictionary<int, Type>();
        internal readonly HashSet<Type> BaseTypes = new HashSet<Type>();
        internal IFormatterResolver InnerResolver;
        internal Type InnerResolverType;

        public PolymorphicMessagePackSettings(IFormatterResolver innerResolver)
        {
            InnerResolver = innerResolver;
            InnerResolverType = InnerResolver.GetType();
        }

        public bool SerializeOnlyRegisteredTypes { get; set; } = false;

        public void RegisterType<B, T>(int typeId)
            where B : class
            where T : class, B
        {

            if (typeof(T).IsInterface || typeof(T).IsAbstract)
                throw new ArgumentException($"Failed to register derived type '{ typeof(T).FullName }'. It cannot be an interface or an abstract class.", nameof(T));

            if (typeof(T).ContainsGenericParameters)
                throw new ArgumentException($"Failed to register derived type '{ typeof(T).FullName }'. It cannot have open generic parameters. You must replace the open generic parameters with specific types.", nameof(T));

            if (TypeToId.TryGetValue(typeof(T), out var currentId) && currentId != typeId)
                throw new ArgumentException($"Failed to register derived type '{ typeof(T).FullName }'. Type '{ typeof(T).FullName }' is already registered to Type Id: { currentId }", nameof(T));

            if (IdToType.TryGetValue(typeId, out var currentType) && currentType != typeof(T))
                throw new ArgumentException($"Failed to register derived type '{ typeof(T).FullName }'. Type Id: { typeId } is already registered to another type '{ currentType.FullName }'", nameof(typeId));

            //Use TryAdd, becasue the type could already exist and the user is simply trying to add another base class
            TypeToId.TryAdd(typeof(T), typeId);
            IdToType.TryAdd(typeId, typeof(T));
            BaseTypes.Add(typeof(B));
        }

    }
}
