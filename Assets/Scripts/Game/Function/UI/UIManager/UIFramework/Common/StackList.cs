using System;
using System.Collections;
using System.Collections.Generic;

namespace GameFrame
{
    public class StackList<T>:IEnumerable<T>
    {
        private readonly List<T> _stackList = new List<T>();

        public T Pop()
        {
            if(_stackList.Count == 0)
            {
                throw new InvalidOperationException("This stack is empty");
            }
            T value = _stackList[_stackList.Count-1];
            _stackList.RemoveAt(_stackList.Count - 1);
            return value;
        }

        public void Push(T item)
        {
            _stackList.Add(item);
        }

        public T Peek()
        {
             if(_stackList.Count==0)
            {
                throw new InvalidOperationException("This stack is empty");
            }
            return _stackList[_stackList.Count - 1];
        }

        public void RemoveAt(int index)
        {
            _stackList.RemoveAt(index);
        }

        public bool Remove(T item)
        {
            return _stackList.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _stackList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T this[int index]
        {
            get { return _stackList[index]; }
            set { _stackList[index] = value; }
        }

        public int Count
        {
            get { return _stackList.Count; }
        }

        public void Clear()
        {
            _stackList.Clear();
        }

        public bool Contains(T item)
        {
            return _stackList.Contains(item);
        }

        public int IndexOf(T item)
        {
            return _stackList.Count - 1 - _stackList.IndexOf(item);
        }
    }
}
