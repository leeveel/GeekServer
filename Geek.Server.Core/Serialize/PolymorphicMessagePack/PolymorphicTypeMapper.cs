using Geek.Server.Core.Serialize.PolymorphicMessagePack;
using Geek.Server.Core.Utils;
using NLog;
using System.Collections.Concurrent;
using System.Reflection;

namespace PolymorphicMessagePack
{
    public class PolymorphicTypeMapper
    {
        static readonly Logger Log = LogManager.GetCurrentClassLogger();
        internal static ConcurrentDictionary<Type, int> TypeToId = new();
        internal static ConcurrentDictionary<int, Type> IdToType = new();

        public static void Clear()
        {
            TypeToId.Clear();
            IdToType.Clear();
        }

        public static bool Contains(Type t)
        {
            return TypeToId.ContainsKey(t);
        }

        public static bool TryGet(int id, out Type t)
        {
            return IdToType.TryGetValue(id, out t);
        }
        public static bool TryGet(Type t, out int id)
        {
            return TypeToId.TryGetValue(t, out id);
        }

        public static void Register<T>()
        {
            Register(typeof(T));
        }

        public static void Register(Type type)
        {
            var id = (int)MurmurHash3.Hash(type.FullName);
            if (IdToType.TryGetValue(id, out var t))
            {
                if (t.FullName != type.FullName)
                {
                    Log.Error($"typemapper注册错误,不同类型,id相同{t.FullName}  {type.FullName}");
                }
            }
            IdToType[id] = type;
            TypeToId[type] = id;
        }

        public static void Register(Assembly assembly)
        {
            var types = from h in assembly.GetTypes()
                        where h.IsClass && !(h.IsSealed && h.IsAbstract) && !h.ContainsGenericParameters && !h.FullName.Contains("<") && !h.IsSubclassOf(typeof(Attribute)) && h.GetCustomAttribute<PolymorphicIgnore>() == null
                        select h;
            foreach (var t in types)
            {
                Register(t);
            }
        }

        public static void Register(List<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                Register(assembly);
            }
        }
        public static void RegisterCore()
        {
            Register(typeof(PolymorphicTypeMapper).Assembly);
        }
    }
}
