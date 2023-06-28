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
        internal static ConcurrentDictionary<string, List<Type>> classBaseNameToType = new();

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
        public static bool TryGet(string tname, Type baseType, out Type type)
        {
            type = null;
            classBaseNameToType.TryGetValue(tname, out var tlsit);
            if (tlsit != null)
            {
                foreach (var t in tlsit)
                {
                    if (t.IsSubclassOf(baseType) || t == baseType)
                    {
                        type = t;
                        return true;
                    }

                }
            }
            return false;
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

            //这里是为了兼容mongodb转换后的数据
            if (!classBaseNameToType.TryGetValue(type.Name, out var tlist))
            {
                tlist = new();
                classBaseNameToType[type.Name] = tlist;
            }
            for (int i = tlist.Count - 1; i >= 0; i--)
            {
                var t1 = tlist[i];
                if (t1.FullName == type.FullName)
                {
                    tlist.RemoveAt(i);
                }
            }
            tlist.Add(type);
        }

        public static void Register(Assembly assembly)
        {
            var types = from h in assembly.GetTypes()
                        where h.IsClass && !h.ContainsGenericParameters && !h.FullName.Contains("<") && !h.FullName.EndsWith("Handler") && !h.IsSubclassOf(typeof(Attribute)) && h.GetCustomAttribute<PolymorphicIgnore>() == null
                        select h;
            foreach (var t in types)
            {
                Register(t);
            }
        }

        public static void UnRegister(Type type)
        {
            var id = (int)MurmurHash3.Hash(type.FullName);
            if (!IdToType.TryGetValue(id, out _))
            {
                return;
            }
            PolymorphicResolver.Instance.RemoveFormatterDelegateCache(type);

            IdToType.Remove(id, out _);
            TypeToId.Remove(type, out _);

            if (!classBaseNameToType.TryGetValue(type.Name, out var tlist))
            {
                return;
            }

            for (int i = tlist.Count - 1; i >= 0; i--)
            {
                var t1 = tlist[i];
                if (t1 == type)
                {
                    tlist.RemoveAt(i);
                }
            }
        }

        public static void UnRegister(Assembly assembly)
        {
            var types = assembly.GetTypes();
            foreach (var t in types)
            {
                UnRegister(t);
            }
        }

        public static void RegisterCore()
        {
            Register(typeof(PolymorphicTypeMapper).Assembly);
        }
    }
}
