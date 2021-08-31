using System;
using System.Collections;
using System.Collections.Generic;

namespace Geek.Server
{
    public sealed class StateList<T> : BaseState, IList<T>
    {
        readonly List<T> list;
        public StateList()
        {
            list = new List<T>();
        }

        public StateList(IEnumerable<T> collection)
        {
            list = new List<T>(collection);
        }

        public override bool IsChanged
        {
            get
            {
                if (_stateChanged)
                    return true;
                if(typeof(T).IsSubclassOf(typeof(BaseState)))
                {
                    foreach(var item in list)
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
                foreach (var item in list)
                {
                    if (item == null)
                        continue;
                    if (item is BaseState bs)
                        bs.ClearChanges();
                }
            }
        }

        public T this[int index]
        {
            get
            {
                return list[index];
            }
            set
            {
                list[index] = value;
                _stateChanged = true;
            }
        }

        public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            list.Insert(index, item);
            _stateChanged = true;
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
            _stateChanged = true;
        }

        public void Add(T item)
        {
            Insert(list.Count, item);
        }

        public T Find(Predicate<T> predicate)
        {
            return list.Find(predicate);
        }

        public List<T> FindAll(Predicate<T> match)
        {
            return list.FindAll(match);
        }

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            return list.FindIndex(startIndex, count, match);
        }

        public int FindIndex(int startIndex, Predicate<T> match)
        {
            return list.FindIndex(startIndex, match);
        }

        public int FindIndex(Predicate<T> match)
        {
            return list.FindIndex(match);
        }

        public void ForEach(Action<T> action)
        {
            list.ForEach(action);
        }

        public void Sort(Comparison<T> comparison = null)
        {
            if (comparison == null)
                list.Sort();
            else
                list.Sort(comparison);
            _stateChanged = true;
        }

        public void AddRange(IEnumerable<T> collection)
        {
            list.AddRange(collection);
            _stateChanged = true;
        }

        public T[] ToArray()
        {
            return list.ToArray();
        }

        public void Clear()
        {
            list.Clear();
            _stateChanged = true;
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            int index = list.IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                _stateChanged = true;
                return true;
            }
            return false;
        }

        public int RemoveAll(Predicate<T> match)
        {
            int removedNum = list.RemoveAll(match);
            if (removedNum > 0)
                _stateChanged = true;
            return removedNum;
        }

        public bool Exists(Predicate<T> match)
        {
            return list.Exists(match);
        }

        public int Count
        {
            get { return list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)list).GetEnumerator();
        }

        public static implicit operator StateList<T>(List<T> list)
        {
            return new StateList<T>(list);
        }

        public static implicit operator List<T>(StateList<T> list)
        {
            return list.list;
        }
    }
}
