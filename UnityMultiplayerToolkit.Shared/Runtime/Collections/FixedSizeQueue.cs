using System.Collections.Generic;

namespace UnityMultiplayerToolkit.Shared
{
    public class FixedSizeQueue<T> : Queue<T>
    {
        private int _size;

        public FixedSizeQueue(int size) : base(size)
        {
            _size = size;
        }

        public new void Enqueue(T item)
        {
            while (this.Count >= _size)
            {
                Dequeue();
            }
            base.Enqueue(item);
        }
    }
}
