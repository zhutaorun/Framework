namespace GameFrame.Utils
{
    using System;

    public class KeyedPriorityQueueHeadChangedEventArgs<T> :EventArgs where T:class
    {
        private T newFirstElement;
        private T oldFirstElement;

        public KeyedPriorityQueueHeadChangedEventArgs(T oldFirstElement,T newFirstElement)
        {
            this.newFirstElement = newFirstElement;
            this.oldFirstElement = oldFirstElement;
        }


        public T NewFirstElement
        {
            get
            {
                return newFirstElement;
            }
        }

        public T OldFirstElement
        {
            get
            {
                return oldFirstElement;
            }
        }

    }
}
