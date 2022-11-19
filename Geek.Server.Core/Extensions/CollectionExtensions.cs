using Geek.Server.Core.Utils;

namespace System.Collections.Generic
{
    public static class CollectionExtensions
    {

        #region DictionaryExtensions
        public static void Merge<K, V>(this Dictionary<K, V> self, K k, V v, Func<V, V, V> func)
        {
            self[k] = self.ContainsKey(k) ? func(self[k], v) : v;
        }

        /// <summary>
        /// 根据key获取value值，当不存在时通过valueGetter生成value，放入Dictionary并返回value
        /// </summary>
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key, Func<TKey, TValue> valueGetter)
        {
            if (!self.TryGetValue(key, out var value))
            {
                value = valueGetter(key);
                self[key] = value;
            }
            return value;
        }

        /// <summary>
        /// 根据key获取value值，当不存在时通过默认构造函数生成value，放入Dictionary并返回value
        /// </summary>
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key) where TValue : new()
        {
            return GetOrAdd(self, key, k => new TValue());
        }

        /// <summary>
        /// 根据条件移除
        /// </summary>
        public static int RemoveIf<TKey, TValue>(this Dictionary<TKey, TValue> self, Func<TKey, TValue, bool> predict)
        {
            int count = 0;
            foreach (var kv in self)
            {
                if (predict(kv.Key, kv.Value))
                {
                    self.Remove(kv.Key);
                    count++;
                }
            }
            return count;
        }
        #endregion



        #region ICollectionExtensions
        public static bool IsNullOrEmpty<T>(this ICollection<T> self)
        {
            return self == null || self.Count <= 0;
        }
        #endregion

        #region List<T>
        /// <summary>
        /// 打乱
        /// </summary>
        public static void Shuffer<T>(this List<T> list)
        {
            int n = list.Count;
            var r = ThreadLocalRandom.Current;
            for (int i = 0; i < n; i++)
            {
                int rand = r.Next(i, n);
                T t = list[i];
                list[i] = list[rand];
                list[rand] = t;
            }
        }

        public static void RemoveIf<T>(this List<T> list, Predicate<T> condition)
        {
            var idx = list.FindIndex(condition);
            while (idx >= 0)
            {
                list.RemoveAt(idx);
                idx = list.FindIndex(condition);
            }
        }
        #endregion

        public static void AddRange<T>(this HashSet<T> c, IEnumerable<T> e)
        {
            foreach (var item in e)
            {
                c.Add(item);
            }
        }
    }
}
