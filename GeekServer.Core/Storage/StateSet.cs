using System;
using System.Collections;
using System.Collections.Generic;

namespace Geek.Server
{
    public class StateSet<T> : BaseDBState, ISet<T>
    {
        readonly HashSet<T> set;
        public StateSet()
        {
            set = new HashSet<T>();
        }

        public StateSet(IEnumerable<T> collection)
        {
            set = new HashSet<T>(collection);
        }

        readonly object lockObj = new object();
        public StateSet<T> ShallowCopy()
        {
            lock (lockObj)
            {
                var copy = new StateSet<T>(set);
                return copy;
            }
        }

        public int Count => set.Count;
        public bool IsReadOnly => false;

        public override bool IsChanged
        {
            get
            {
                if (_stateChanged)
                    return true;
                if (typeof(T).IsSubclassOf(typeof(BaseDBState)))
                {
                    foreach (var item in set)
                    {
                        if (item == null)
                            continue;
                        if ((item as BaseDBState).IsChanged)
                            return true;
                    }
                }
                return false;
            }
        }

        public override void ClearChanges()
        {
            _stateChanged = false;
            if (typeof(T).IsSubclassOf(typeof(BaseDBState)))
            {
                foreach (var item in set)
                {
                    if (item == null)
                        continue;
                    (item as BaseDBState).ClearChanges();
                }
            }
        }

        public bool Add(T item)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            _stateChanged = true;
            lock (lockObj)
            {
                return set.Add(item);
            }
        }

        public void Clear()
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            _stateChanged = true;
            lock (lockObj)
            {
                set.Clear();
            }
        }

        public bool Contains(T item)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            return set.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            set.CopyTo(array, arrayIndex);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            _stateChanged = true;
            lock (lockObj)
            {
                set.ExceptWith(other);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            return set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            return ((IEnumerable)set).GetEnumerator();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            _stateChanged = true;
            lock (lockObj)
            {
                set.IntersectWith(other);
            }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return set.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return set.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return set.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return set.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return set.Overlaps(other);
        }

        public bool Remove(T item)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            _stateChanged = true;
            lock (lockObj)
            {
                return set.Remove(item);
            }
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return set.SetEquals(other);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            _stateChanged = true;
            lock (lockObj)
            {
                set.SymmetricExceptWith(other);
            }
        }

        public void UnionWith(IEnumerable<T> other)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            _stateChanged = true;
            lock (lockObj)
            {
                set.UnionWith(other);
            }
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotImplementedException();
        }

        public static implicit operator StateSet<T>(HashSet<T> hash)
        {
            return new StateSet<T>(hash);
        }

        public static implicit operator HashSet<T>(StateSet<T> set)
        {
            return set.set;
        }
    }
}
