using System;
using System.Collections;
using System.Collections.Generic;

namespace Geek.Server
{
    public class StateSet<T> : BaseState, ISet<T>
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

        public int Count => set.Count;
        public bool IsReadOnly => false;

        public override bool IsChanged
        {
            get
            {
                if (_stateChanged)
                    return true;
                if (typeof(T).IsSubclassOf(typeof(BaseState)))
                {
                    foreach (var item in set)
                    {
                        if (item == null)
                            continue;
                        if ((item as BaseState).IsChanged)
                            return true;
                    }
                }
                return false;
            }
        }

        public override void ClearChanges()
        {
            _stateChanged = false;
            if (typeof(T).IsSubclassOf(typeof(BaseState)))
            {
                foreach (var item in set)
                {
                    if (item == null)
                        continue;
                    (item as BaseState).ClearChanges();
                }
            }
        }

        public bool Add(T item)
        {
            _stateChanged = true;
            return set.Add(item);
        }

        public void Clear()
        {
            _stateChanged = true;
            set.Clear();
        }

        public bool Contains(T item)
        {
            return set.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            set.CopyTo(array, arrayIndex);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            _stateChanged = true;
            set.ExceptWith(other);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return set.GetEnumerator();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            _stateChanged = true;
            set.IntersectWith(other);
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
            _stateChanged = true;
            return set.Remove(item);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return set.SetEquals(other);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            _stateChanged = true;
            set.SymmetricExceptWith(other);
        }

        public void UnionWith(IEnumerable<T> other)
        {
            _stateChanged = true;
            set.UnionWith(other);
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)set).GetEnumerator();
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
