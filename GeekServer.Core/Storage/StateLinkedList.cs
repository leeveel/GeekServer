using System;
using System.Collections;
using System.Collections.Generic;

namespace Geek.Server
{
    public sealed class StateLinkedList<T> : BaseState, IEnumerable<T>
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
                    (item as BaseState).ClearChanges();
                }
            }
        }


        public void AddLast(T item)
        {
            list.AddLast(item);
            _stateChanged = true;
        }

        public void AddLast(LinkedListNode<T> node)
        {
            list.AddLast(node);
            _stateChanged = true;
        }

        public void RemoveLast()
        {
            list.RemoveLast();
            _stateChanged = true;
        }

        public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)
        {
            list.AddAfter(node, newNode);
            _stateChanged = true;
        }

        public LinkedListNode<T> AddFirst(T item)
        {
            var node = list.AddFirst(item);
            _stateChanged = true;
            return node;
        }

        public void AddFirst(LinkedListNode<T> node)
        {
            list.AddFirst(node);
            _stateChanged = true;
        }

        public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
        {
            list.AddBefore(node, newNode);
            _stateChanged = true;
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
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)list).GetEnumerator();
        }

        public bool Remove(T item)
        {
            _stateChanged = true;
            return list.Remove(item);
        }

        public void Remove(LinkedListNode<T> node)
        {
            _stateChanged = true;
            list.Remove(node);
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
