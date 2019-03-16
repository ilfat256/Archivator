using GZipTest.Core;
using GZipTest.Core.Models;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipTest.Domain
{
    public class FileCompressor
    {
        private Block stopWorkingTask = null;
        private ConcurrentQueueBuffer<Block> readingQueue;
        private ConcurrentQueueBuffer<Block> writingQueue;
        private ParallelWorker compressingWorker;

        private GZipOptions options;
        public FileCompressor(GZipOptions options)
        {
            this.options = options;
            readingQueue = new ConcurrentQueueBuffer<Block>(options.TaskCount);
            writingQueue = new ConcurrentQueueBuffer<Block>(options.TaskCount);
            compressingWorker = new ParallelWorker(options.ThreadCount, "CompressingWork");
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
            using (var source = new DecompressedFileStream(options.SourceFileName, options.FileStreamBufferSize))
            {
                long fileSizeBytes = options.SourceFileBytesCount;
                int blockSize = options.ReadingBlockSize;
                long blockCount = fileSizeBytes / blockSize;
                for (long i = 0; i < blockCount; i++)
                {
                    byte[] buffer = source.ReadBytes(blockSize);
                    readingQueue.Enqueue(new Block(i, buffer));
                }

                int remainingBytes = (int)(fileSizeBytes % blockSize);
                if (remainingBytes != 0)
                {
                    byte[] buffer = source.ReadBytes(remainingBytes);
                    readingQueue.Enqueue(new Block(blockCount, buffer ));
                }
            }

            AddStopTasksForWorker();
        }

        public void AddStopTasksForWorker()
        {
            for (int i = 0; i < options.ThreadCount; i++)
            {
                readingQueue.Enqueue(stopWorkingTask); //ended task for flush file
            }
        }

        public void WritingTask()
        {
            using (FileStream stream = new FileStream(options.ResultFileName, FileMode.Create))
            {
                using (CompressingStream compressingStream = new CompressingStream(stream))
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
