
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PolymorphicMessagePack
{
    public class PolymorphicTypeMapper
    {
        internal static ConcurrentDictionary<Type, int> TypeToId = new ConcurrentDictionary<Type, int>();
        internal static ConcurrentDictionary<int, Type> IdToType = new ConcurrentDictionary<int, Type>();
        internal static ConcurrentDictionary<string, List<Type>> classBaseNameToType = new ConcurrentDictionary<string, List<Type>>();

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
                    Logger.Error($"typemapper注册错误,不同类型,id相同{t.FullName}  {type.FullName}");
                }
            }

            IdToType[id] = type;
            TypeToId[type] = id;

            //这里是为了兼容mongodb转换后的数据
            if (!classBaseNameToType.TryGetValue(type.Name, out var tlist))
            {
                tlist = new List<Type>();
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


        public static void UnRegister(Type type)
        {
            var id = (int)MurmurHash3.Hash(type.FullName);
            if (!IdToType.TryGetValue(id, out _))
            {
                return;
            }
            PolymorphicResolver.Instance.RemoveFormatterDelegateCache(type);

            IdToType.TryRemove(id, out _);
            TypeToId.TryRemove(type, out _);

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

    }
}
