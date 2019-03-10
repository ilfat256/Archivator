using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZipTest.Core
{
    public class PulseAllConcurrentQueueBuffered<T>
    {
        private Queue<T> _queue;
        private object _lockObject = new object();
        private int _bufferSize;
        private bool _pulseFromEnqueue = false;

        public int Count => _queue.Count;

        public PulseAllConcurrentQueueBuffered(int bufferSize)
        {
            _bufferSize = bufferSize;
            _queue = new Queue<T>(bufferSize);
        }

        public void Enqueue(T item)
        {
            lock (_lockObject)
            {
                while (Count >= _bufferSize)
                {
                    Console.WriteLine("enqueed while");
                    //if (!_pulseFromEnqueue)
                    {
                        Monitor.PulseAll(_lockObject);
                    }
                    _pulseFromEnqueue = true;
                    Monitor.Wait(_lockObject);
                    Thread.Sleep(20);
                }

                _queue.Enqueue(item);
                Monitor.PulseAll(_lockObject);
                Console.WriteLine("enqueed " + item);

            }
        }

        public T Dequeue(string threadNumer)
        {
            lock (_lockObject)
            {
                while (Count < 1)
                {
                    Console.WriteLine(threadNumer + " while");
                    //if (_pulseFromEnqueue)
                    {
                        Monitor.PulseAll(_lockObject);
                    }
                    _pulseFromEnqueue = false;
                    Monitor.Wait(_lockObject);
                    Thread.Sleep(20);
                }

                var item = _queue.Dequeue();
                Monitor.PulseAll(_lockObject);
                Console.WriteLine(threadNumer + " readed " + item);
                return item;

            }
        }
    }
}
