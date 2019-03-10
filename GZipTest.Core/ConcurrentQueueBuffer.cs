using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZipTest.Core
{
    public class ConcurrentQueueBuffer<T>
    {
        private Queue<T> _queue;
        private readonly int _bufferSize;
        private int _count = 0;
        private object _lockObjectQueue = new object();
        private object _lockObjectCount = new object();

        public ConcurrentQueueBuffer(int capacity)
        {
            _bufferSize = capacity;
            _queue = new Queue<T>(capacity);
        }

        public void Enqueue(T item) //if 1 thread , will stuck
        {
            lock (_lockObjectCount)
            {
                while (_count >= _bufferSize)
                {
                    Console.WriteLine("Enqueue wait");
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

        public T Dequeue() //1 thread stuck
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
