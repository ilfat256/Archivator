using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZipTest.Core
{
    public class Worker
    {
        private Thread[] _threads;
        public Worker(int threadCount)
        {
            _threads = new Thread[threadCount];
            
        }

        public void Start(Action work, string name)
        {
            for (int i = 0; i < _threads.Count(); i++)
            {
                _threads[i] = new Thread(new ThreadStart(work));
                _threads[i].Name = name + i;
                _threads[i].Start();
            }
        }

        internal void Join()
        {
            for (int i = 0; i < _threads.Count(); i++)
            {
                _threads[i].Join();
            }
        }
    }
}
