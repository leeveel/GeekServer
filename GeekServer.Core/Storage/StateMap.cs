using System.Collections;
using System.Collections.Generic;

namespace Geek.Server
{
    public sealed class StateMap<TKey, TValue> : BaseState, IDictionary<TKey, TValue>
    {
        readonly Dictionary<TKey, TValue> map;
        public StateMap()
        {
            map = new Dictionary<TKey, TValue>();
        }

        public StateMap(StateMap<TKey, TValue> dictionary)
        {
            map = new Dictionary<TKey, TValue>(dictionary.map);
        }

        public StateMap(IDictionary<TKey, TValue> dictionary)
        {
            map = new Dictionary<TKey, TValue>(dictionary);
        }

        public override bool IsChanged
        {
            get
            {
                if (_stateChanged)
                    return true;

                if (typeof(TKey).IsSubclassOf(typeof(BaseState)))
                {
                    foreach (var item in map)
                    {
                        if ((item.Key as BaseState).IsChanged)
                            return true;
                    }
                }

                if (typeof(TValue).IsSubclassOf(typeof(BaseState)))
                {
                    foreach (var item in map)
                    {
                        if ((item.Value as BaseState).IsChanged)
                            return true;
                    }
                }

                return false;
            }
        }

        public override void ClearChanges()
        {
            _stateChanged = false;
        }

        public bool ContainsKey(TKey key)
        {
            return map.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            map.Add(key, value);
            _stateChanged = true;
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void AddAnother(StateMap<TKey, TValue> otherMap)
        {
            foreach(var kv in otherMap)
                Add(kv);
        }

        public void MergeAnother(StateMap<TKey, TValue> otherMap)
        {
            foreach (var kv in otherMap)
            {
                if (ContainsKey(kv.Key))
                    continue;
                Add(kv);
            }
        }

        public bool Remove(TKey key)
        {
            if (map.ContainsKey(key))
            {
                map.Remove(key);
                _stateChanged = true;
                return true;
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return map.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get
            {
                return map[key];
            }
            set
            {
                _stateChanged = true;
                if (map.ContainsKey(key))
                    map[key] = value;
                else
                    map.Add(key, value);
            }
        }

        public ICollection<TKey> Keys
        {
            get { return map.Keys; }
        }

        public ICollection<TValue> Values
        {
            get { return map.Values; }
        }

        public void Clear()
        {
            map.Clear();
            _stateChanged = true;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)map).Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)map).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (((ICollection<KeyValuePair<TKey, TValue>>)map).Remove(item))
            {
                _stateChanged = true;
                return true;
            }
            return false;
        }

        public int Count
        {
            get { return map.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }


        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return map.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)map).GetEnumerator();
        }
    }
}