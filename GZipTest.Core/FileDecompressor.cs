using GZipTest.Core.Compressing;
using GZipTest.Core.Extensions;
using GZipTest.Core.Models;
using Parallel;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace GZipTest.Core
{
    public class FileDecompressor
    {
        private Block stopWorkingTask = null;
        private ConcurrentQueueBuffer<Block> readingQueue;
        private ConcurrentQueueBuffer<Block> writingQueue;
        private Worker decompressingWorker;
        private GZipOptions options;

        public FileDecompressor(GZipOptions options)
        {
            this.options = options;
            readingQueue = new ConcurrentQueueBuffer<Block>(options.TaskCount);
            writingQueue = new ConcurrentQueueBuffer<Block>(options.TaskCount);
            decompressingWorker = new Worker(options.ThreadCount, "DecompressingWork");
        }

        public void DecompressAndSaveFile()
        {
            try
            {
                var readingThread = new Thread(() => ReadingTask());
                readingThread.Name = "ReadingWork Thread";
                readingThread.Start();

                decompressingWorker.Start(() => DecompressingTask());
                var writingThread = new Thread(() => WritingTask());
                writingThread.Name = "ReadingWork Thread";
                writingThread.Start();
                decompressingWorker.Join();
                writingQueue.Enqueue(null); //for flushing writing
                writingThread.Join();
                //all work done
            }
            catch (IOException exception)
            {
                throw exception;
            }
        }

        void DecompressingTask()
        {
            while (true)
            {
                var block = readingQueue.Dequeue();
                if (block == null)
                {
                    break;
                }
                block.Bytes = block.Bytes.GetDecompressed();
                writingQueue.Enqueue(block);
            }
        }

        private void ReadingTask()
        {
            //buffer size of FileStream & Should be closed after closing adapter
            using (Stream fileStream = new FileStream(options.SourceFileName, FileMode.Open, FileAccess.Read))
            {
                using (BlockGZipStream gzipStream = new BlockGZipStream(fileStream, BlockGZipStreamMode.Decompressing))
                {
                    for (int i = 0; i < gzipStream.Lenght; i++)
                    {
                        readingQueue.Enqueue(gzipStream.ReadBlock(i));
                    }
                }
            }

            for (int i = 0; i < options.ThreadCount; i++)
            {
                readingQueue.Enqueue(stopWorkingTask);
            }
        }

        private void WritingTask()
        {
            var buffer = new List<Block>(options.ThreadCount);
            int blockNumberToWrite = 0;
            using (FileStream destination = new FileStream(options.ResultFileName, FileMode.Create))
            {
                while (true)
                {
                    Block block;
                    var blockIndex = buffer.FindIndex(b => b.Number == blockNumberToWrite);
                    if (blockIndex < 0)
                    {
                        block = writingQueue.Dequeue();
                    }
                    else
                    {
                        block = buffer[blockIndex];
                        buffer.RemoveAt(blockIndex);
                    }

                    if (block == null)
                    {
                        break;
                    }

                    if (block.Number != blockNumberToWrite)
                    {
                        buffer.Add(block);
                    }
                    else
                    {
                        destination.Write(block.Bytes, 0, block.Bytes.Length);
                        blockNumberToWrite++;
                    }
                }
            }
        }
    }
}
