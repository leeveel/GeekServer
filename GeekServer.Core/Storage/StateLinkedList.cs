using System;
using System.Collections;
using System.Collections.Generic;

namespace Geek.Server
{
    public sealed class StateLinkedList<T> : BaseDBState, IEnumerable<T>
    {
        readonly LinkedList<T> list;
        public StateLinkedList()
        {
            list = new LinkedList<T>();
        }

        public StateLinkedList(IEnumerable<T> collection)
        {
            list = new LinkedList<T>(collection);
        }

        readonly object lockObj = new object();
        public StateLinkedList<T> ShallowCopy()
        {
            lock (lockObj)
            {
                var copy = new StateLinkedList<T>(list);
                return copy;
            }
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


        public void AddLast(T item)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            lock (lockObj)
            {
                list.AddLast(item);
            }
            _stateChanged = true;
        }

        public void AddLast(LinkedListNode<T> node)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            lock (lockObj)
            {
                list.AddLast(node);
            }
            _stateChanged = true;
        }

        public void RemoveLast()
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            lock (lockObj)
            {
                list.RemoveLast();
            }
            _stateChanged = true;
        }

        public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            lock (lockObj)
            {
                list.AddAfter(node, newNode);
            }
            _stateChanged = true;
        }

        public LinkedListNode<T> AddFirst(T item)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            _stateChanged = true;
            lock (lockObj)
            {
                var node = list.AddFirst(item);
                return node;
            }
        }

        public void AddFirst(LinkedListNode<T> node)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            lock (lockObj)
            {
                list.AddFirst(node);
            }
            _stateChanged = true;
        }

        public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            lock (lockObj)
            {
                list.AddBefore(node, newNode);
            }
            _stateChanged = true;
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

        public LinkedListNode<T> First
        {
            get { return list.First; }
        }

        public LinkedListNode<T> Last
        {
            get { return list.Last; }
        }

        public int Count
        {
            get { return list.Count; }
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

        public bool Remove(T item)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            _stateChanged = true;
            lock (lockObj)
            {
                return list.Remove(item);
            }
        }

        public void Remove(LinkedListNode<T> node)
        {
#if DEBUG_MODE
            CheckIsInCompActorCallChain();
#endif
            _stateChanged = true;
            lock (lockObj)
            {
                list.Remove(node);
            }
        }

        public static implicit operator StateLinkedList<T>(LinkedList<T> list)
        {
            return new StateLinkedList<T>(list);
        }

        public static implicit operator LinkedList<T>(StateLinkedList<T> list)
        {
            return list.list;
        }
    }
}
