using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZipTest.Core
{
    public class ParallelWorker
    {
        private Thread[] _threads;
        public string WorkName { get; }
        public ParallelWorker(int threadCount, string workName)
        {
            WorkName = workName;
            _threads = new Thread[threadCount];            
        }

        public void Start(Action work)
        {
            for (int i = 0; i < _threads.Count(); i++)
            {
                _threads[i] = new Thread(new ThreadStart(work));
                _threads[i].Name = $"{WorkName} thread {i}";
                _threads[i].Start();
            }
        }

        public void Join()
        {
            for (int i = 0; i < _threads.Count(); i++)
            {
                _threads[i].Join();
            }
        }
    }
}
