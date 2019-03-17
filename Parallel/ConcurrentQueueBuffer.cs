using System;
using System.Collections.Generic;
using System.Threading;

namespace Parallel
{
    public class ConcurrentQueueBuffer<T>
    {
        private Queue<T> _queue;
        private readonly int _bufferSize;
        private int _count = 0;
        private object _lockObjectQueue = new object();
        private object _lockObjectCount = new object();

        public ConcurrentQueueBuffer(int maxCapacity)
        {
            _bufferSize = maxCapacity;
            _queue = new Queue<T>(maxCapacity);
        }

        public void Enqueue(T item)
        {
            lock (_lockObjectCount)
            {
                while (_count >= _bufferSize)
                {
                    Monitor.Wait(_lockObjectCount);
                }
                _count++;
            }

            lock (_lockObjectQueue)
            {
                _queue.Enqueue(item);
                Monitor.Pulse(_lockObjectQueue);
            }
        }

        public T Dequeue()
        {
            T item;
            lock (_lockObjectQueue)
            {
                while (_queue.Count < 1)
                {
                    Monitor.Wait(_lockObjectQueue);
                }

                item = _queue.Dequeue();
            }

            lock (_lockObjectCount)
            {
                Monitor.Pulse(_lockObjectCount);
                _count--;
                return item;
            }
        }
    }
}
