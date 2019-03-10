using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZipTest.Core
{
    public class ConcurrentQueue<T>
    {
        private Queue<T> _queue;
        private object _lockObject = new object();

        public int Count => _queue.Count;

        public ConcurrentQueue(int capacity)
        {
            _queue = new Queue<T>(capacity);
        }

        public void Enqueue(T item)
        {
            lock (_lockObject)
            {
                _queue.Enqueue(item);
                Monitor.Pulse(_lockObject);
            }
        }

        public T Dequeue()
        {
            lock (_lockObject)
            {
                while (Count < 1)
                {
                    Monitor.Wait(_lockObject);
                }

                var item = _queue.Dequeue();
                return item;
            }
        }
    }
}
