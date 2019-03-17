using GZipTest.Core.Compressing;
using GZipTest.Core.Extensions;
using GZipTest.Core.Models;
using Parallel;
using System;
using System.IO;
using System.Threading;

namespace GZipTest.Core
{
    public class FileCompressor
    {
        private Block stopWorkingTask = null;
        private ConcurrentQueueBuffer<Block> readingQueue;
        private ConcurrentQueueBuffer<Block> writingQueue;
        private Worker compressingWorker;
        private GZipOptions options;

        public FileCompressor(GZipOptions options)
        {
            this.options = options;
            readingQueue = new ConcurrentQueueBuffer<Block>(options.TaskCount);
            writingQueue = new ConcurrentQueueBuffer<Block>(options.TaskCount);
            compressingWorker = new Worker(options.ThreadsCount, "CompressingWork");
        }

        public void CompressAndSaveFile()
        {
            try
            {
                //in compressed block we can store size of block
                var readingThread = new Thread(() => ReadingTask());
                readingThread.Name = "ReadingWork Thread";
                readingThread.Start();

                compressingWorker.Start(() => CompressingTask());

                var writingThread = new Thread(() => WritingTask());
                writingThread.Name = "WritingWork Thread";
                writingThread.Start();

                compressingWorker.Join();
                writingQueue.Enqueue(stopWorkingTask); //for flushing writing
                writingThread.Join();
            }
            catch (Exception exception)
            {
                //what to do here, threadException?
                throw exception;
            }
        }

        void CompressingTask()
        {
            while (true)
            {
                var block = readingQueue.Dequeue();
                if (block == stopWorkingTask)
                {
                    break;
                }

                block.Bytes = block.Bytes.GetCompressed();
                writingQueue.Enqueue(block);
            }
        }

        public void ReadingTask()
        {
            using (var fileStream = new FileStream(options.SourceFileName, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                long fileSizeBytes = options.SourceFileBytesCount;
                int blockSize = options.ReadingBlockSizeBytes;
                long blockCount = fileSizeBytes / blockSize;
                for (long i = 0; i < blockCount; i++)
                {
                    byte[] buffer = new byte[blockSize];
                    int readed = fileStream.Read(buffer, 0, blockSize);
                    readingQueue.Enqueue(new Block(i, buffer));
                }

                int remainingBytes = (int)(fileSizeBytes % blockSize);
                if (remainingBytes != 0)
                {
                    byte[] buffer = new byte[remainingBytes];
                    int readed = fileStream.Read(buffer, 0, remainingBytes);
                    readingQueue.Enqueue(new Block(blockCount, buffer));
                }
            }
            AddStopTasksForWorker();
        }

        public void AddStopTasksForWorker()
        {
            for (int i = 0; i < options.ThreadsCount; i++)
            {
                readingQueue.Enqueue(stopWorkingTask);
            }
        }

        public void WritingTask()
        {
            using (FileStream stream = new FileStream(options.ResultFileName, FileMode.Create))
            {
                using (BlockGZipStream compressingStream = new BlockGZipStream(stream, BlockGZipStreamMode.Compressing))
                {
                    while (true)
                    {
                        var block = writingQueue.Dequeue();
                        if (block == stopWorkingTask)
                        {
                            break;
                        }
                        compressingStream.WriteBlock(block);
                    }
                }
            }
        }
    }
}
