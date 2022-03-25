using System;
using System.Collections;
using System.Collections.Generic;

namespace Geek.Server
{
    public sealed class StateList<T> : BaseDBState, IList<T>
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

        readonly object lockObj = new object();
        public StateList<T> ShallowCopy()
        {
            var copy = new StateList<T>();
            lock (lockObj)
            {
                copy.AddRange(list);
            }
            return copy;
        }

        public override bool IsChanged
        {
            get
            {
                if (_stateChanged)
                    return true;
                if(typeof(T).IsSubclassOf(typeof(BaseDBState)))
                {
                    foreach(var item in list)
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
                foreach (var item in list)
                {
                    if (item == null)
                        continue;
                    (item as BaseDBState).ClearChanges();
                }
            }
        }

        public List<T> FindAll(Predicate<T> match) 
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            return list.FindAll(match);
        }

        public T this[int index]
        {
            get
            {
#if DEBUG_MODE
                CheckIsInCompActorCallChain();
#endif
                return list[index];
            }
            set
            {
#if DEBUG_MODE
                CheckIsInCompActorCallChain();
#endif
                lock (lockObj)
                {
                    list[index] = value;
                }
                _stateChanged = true;
            }
        }

        public int IndexOf(T item)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            return list.IndexOf(item);
        }

        public int FindIndex(int startIndex, int count, Predicate<T> match) 
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            return list.FindIndex(startIndex, count, match);
        }

        public int FindIndex(int startIndex, Predicate<T> match)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            return list.FindIndex(startIndex,  match);
        }

        public int FindIndex(Predicate<T> match)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            return list.FindIndex(match);
        }

        public int RemoveAll(Predicate<T> match) 
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            lock (lockObj)
            {
                int removedNum = list.RemoveAll(match);
                if (removedNum > 0)
                    _stateChanged = true;
                return removedNum;
            }
        }



        public void Insert(int index, T item)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            lock (lockObj)
            {
                list.Insert(index, item);
            }
            _stateChanged = true;
        }

        public void RemoveAt(int index)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            lock (lockObj)
            {
                list.RemoveAt(index);
            }
            _stateChanged = true;
        }

        public void Add(T item)
        {
            Insert(list.Count, item);
        }

        public T Find(Predicate<T> predicate)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            return list.Find(predicate);
        }

        public void ForEach(Action<T> action)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            list.ForEach(action);
        }

        public void Sort(Comparison<T> comparison = null)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            lock (lockObj)
            {
                if (comparison == null)
                    list.Sort();
                else
                    list.Sort(comparison);
            }
            _stateChanged = true;
        }

        public List<T> GetRange(int index, int count) 
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            return list.GetRange(index, count);
        }

        public void AddRange(IEnumerable<T> collection)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            lock (lockObj)
            {
                list.AddRange(collection);
            }
            _stateChanged = true;
        }

        public T[] ToArray()
        {
            return list.ToArray();
        }

        public void Clear()
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            lock (lockObj)
            {
                list.Clear();
            }
            _stateChanged = true;
        }

        public bool Contains(T item)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            int index = list.IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }

            return false;
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
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
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

        public bool Exists(Predicate<T> match)
        {
            return list.Exists(match);
        }
    }
}
