namespace GameFrame.UI
{

    public class DoubleLinkNode2<T>
    {
        internal DoubleLinkNode2<T> _next;
        internal DoubleLinkNode2<T> _pre;
        public T Data;

        public DoubleLinkNode2()
        {

        }

        public DoubleLinkNode2(T data)
        {
            this.Data = data;
        }


        public void Detach()
        {
            if(this._pre != null)
            {
                this._pre._next = this._next;
                this._next._pre = this._pre;
                this._pre = null;
                this._next = null;
            }
        }


        public bool IsAttach() => (this._pre != null);
    }


    public class DoubleLink2<T>
    {
        internal DoubleLinkNode2<T> _root;


        public DoubleLink2()
        {
            this._root = new DoubleLinkNode2<T>();
            this._Init();
        }

        private void _Init()
        {
            DoubleLink2<T>._Link(this._root,this._root);
        }


        private static void _Link(DoubleLinkNode2<T> pre,DoubleLinkNode2<T> next)
        {
            pre._next = next;
            next._pre = pre;
        }
        private static void _PushAfter(DoubleLinkNode2<T> before , DoubleLink2<T> list)
        {
            if(!list.IsEmpty())
            {
                DoubleLinkNode2<T> next = list._root._next;
                DoubleLinkNode2<T> pre = list._root._pre;
                DoubleLinkNode2<T> node3 = before._next;
                DoubleLink2<T>._Link(before,next);
                DoubleLink2<T>._Link(pre,node3);
                list._Init();
            }
        }

        private static void _PushAfter(DoubleLinkNode2<T> before,DoubleLinkNode2<T> node)
        {
            DoubleLinkNode2<T> ptr = before._next;
            UnityEngine.Debug.Assert(ptr!=null);
            DoubleLink2<T>._Link(before,node);
            DoubleLink2<T>._Link(node,ptr);
        }

        private static void _PushBefore(DoubleLinkNode2<T> after,DoubleLink2<T> list)
        {
            if(!list.IsEmpty())
            {
                DoubleLinkNode2<T> next = list._root._next;
                DoubleLinkNode2<T> pre = list._root._pre;
                DoubleLink2<T>._Link(after._pre,next);
                DoubleLink2<T>._Link(pre,after);
                list._Init();
            }
        }

        private static void _PushBefore(DoubleLinkNode2<T> after, DoubleLinkNode2<T> node)
        {
            DoubleLinkNode2<T> ptr = after._pre;
            UnityEngine.Debug.Assert(ptr != null);
            DoubleLink2<T>._Link(ptr,node);
            DoubleLink2<T>._Link(node,after);
        }

        public void Clear()
        {
            DoubleLinkNode2<T> node2;
            for(DoubleLinkNode2<T> node =this._root._next;node != this._root; node = node2)
            {
                node2 = node._next;
                node._pre = null;
                node._next = null;
            }
            this._Init();
        }


        public void ClearAndClearContent()
        {
            DoubleLinkNode2<T> node2;
            for(DoubleLinkNode2<T> node = this._root._next;node!= this._root;node = node2)
            {
                node2 = node._next;
                node._pre = null;
                node._next = null;
                node.Data = default(T);
            }
            this._Init();
        }

        public DoubleLinkNode2<T> GetHead() => this._root._next;

        public DoubleLinkNode2<T> GetTail() => this._root._pre;

        public bool IsEmpty() => (this._root._next == this._root);

        public bool IsEnd(DoubleLinkNode2<T> node) => (node == this._root);

        public bool IsNoEmpty() => (this._root._next != this._root);

        public static void Next(ref DoubleLinkNode2<T> node)
        {
            UnityEngine.Debug.Assert((node != null) && (node._next!= null));
            node = node._next;
        }

        public static void Pre(ref DoubleLinkNode2<T> node)
        {
            UnityEngine.Debug.Assert((node != null) && (node._pre != null));
            node = node._pre;
        }

        public static void PushAfter(DoubleLinkNode2<T> before,DoubleLink2<T> list)
        {
            if(before.IsAttach() && !list.IsEmpty())
            {
                DoubleLink2<T>._PushAfter(before,list);
            }
        }

        public static void PushAfter(DoubleLinkNode2<T> before, DoubleLinkNode2<T> node)
        {
            node.Detach();
            DoubleLink2<T>._PushAfter(before, node);
        }

        public void PushBack(DoubleLink2<T> list)
        {
            DoubleLink2<T>._PushBefore(this._root,list);
        }

        public void PushBack(DoubleLinkNode2<T> node)
        {
            node.Detach();
            DoubleLink2<T>._PushBefore(this._root, node);
        }

        public static void PushBefore(DoubleLinkNode2<T> after,DoubleLink2<T> list)
        {
            if(after.IsAttach() && !list.IsEmpty())
            {
                DoubleLink2<T>._PushBefore(after,list);
            }
        }

        public static void PushBefore(DoubleLinkNode2<T> after,DoubleLinkNode2<T> node)
        {
            node.Detach();
            DoubleLink2<T>._PushBefore(after,node);
        }

        public void PushFront(DoubleLink2<T> list)
        {
            DoubleLink2<T>._PushAfter(this._root,list);
        }

        public void PushFront(DoubleLinkNode2<T> node)
        {
            node.Detach();
            DoubleLink2<T>._PushAfter(this._root, node);
        }

        public void SetValue(T value)
        {
            for(DoubleLinkNode2<T> node = this._root._next;node!= this._root;node = node._next)
            {
                node.Data = value;
            }
        }
    }
}
