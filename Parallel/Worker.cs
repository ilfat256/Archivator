using System;
using System.Linq;
using System.Threading;

namespace Parallel
{
    public class Worker
    {
        private Thread[] threads;
        public string WorkName { get; }
        public Worker(int threadCount, string workName)
        {
            WorkName = workName;
            threads = new Thread[threadCount];            
        }

        public void Start(Action work)
        {
            for (int i = 0; i < threads.Count(); i++)
            {
                threads[i] = new Thread(new ThreadStart(work));
                threads[i].Name = $"{WorkName} thread {i}";
                threads[i].Start();
            }
        }

        public void Join()
        {
            for (int i = 0; i < threads.Count(); i++)
            {
                threads[i].Join();
            }
        }
    }
}
