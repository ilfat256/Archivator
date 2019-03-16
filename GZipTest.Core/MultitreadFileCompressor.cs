//using GZipTest.Core.Models;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.IO.Compression;
//using System.Linq;
//using System.Runtime.Serialization.Formatters.Binary;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace GZipTest.Core
//{
//    public class MultitreadFileCompressor
//    {
//        private ConcurrentQueueBuffer<Block> _readingQueue;
//        private ConcurrentQueueBuffer<Block> _writingQueue;
//        private ParallelWorker _worker;
//        private int _readingBufferSize;
//        private int _threadCount;

//        public MultitreadFileCompressor(int threadCount)
//        {
//            _readingBufferSize = 8000;
//            _readingQueue = new ConcurrentQueueBuffer<Block>(20);
//            _writingQueue = new ConcurrentQueueBuffer<Block>(20);
//            _threadCount = threadCount;
//            _worker = new ParallelWorker(threadCount, "CompressWork"); //, "DeCompressWork "
//        }

//        public void DecompressFile(string sourceFileName, string resultFileName)
//        {
//            try
//            {
//                _readingQueue = new ConcurrentQueueBuffer<Block>(20);
//                _writingQueue = new ConcurrentQueueBuffer<Block>(20);
//                var readingThread = new Thread(() => ReadCompressedBytesToQueue(sourceFileName));
//                readingThread.Name = "readingCompressedThread";
//                readingThread.Start();
//                _worker.Start(() => DecompressWork());
//                var writingThread = new Thread(() => WriteBytesToFile(resultFileName));
//                writingThread.Name = "writingThread";
//                writingThread.Start();
//                _worker.Join();
//                _writingQueue.Enqueue(null); //for flushing writing
//                writingThread.Join();
//                //all work done
//            }
//            catch (IOException exception)
//            {
//                throw exception;
//            }
//        }


//        public void ReadCompressedBytesToQueue(string sourceFileName)
//        {
//            using (Stream source = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read)) //buffer size of FileStream & Should be closed after closing adapter
//            {
//                long fileSize = source.Length;

//                //offset information length                
//                //read last 4 bytes

//                //check file is my format validate
//                var blocksCountBytes = new byte[4];
//                source.Seek(fileSize - 4, SeekOrigin.Begin);
//                source.Read(blocksCountBytes, 0, blocksCountBytes.Length);

//                long blocksCount = BitConverter.ToInt32(blocksCountBytes, 0);
//                long blockOffsetsArrayLeght = blocksCount * 8;
//                long blockLeghtsArrayLeght = blocksCount * 4;
//                var blockOffsets = new byte[blockOffsetsArrayLeght];
//                var blockLeghts = new byte[blockLeghtsArrayLeght];
//                source.Seek(fileSize - blockLeghtsArrayLeght - blockOffsetsArrayLeght - 4, SeekOrigin.Begin);
//                source.Read(blockOffsets, 0, blockOffsets.Length);
//                var offsets = Deserialize(blockOffsets);
//                source.Read(blockLeghts, 0, blockLeghts.Length);
//                var leghts = DeserializeLenght(blockLeghts);

//                for (long i = 0; i < blocksCount; i++)
//                {
//                    int blockLength = (int)leghts[i]; //probably there is int
//                    var buffer = new byte[blockLength];
//                    if (source.Position != offsets[i])
//                    {
//                        source.Seek(offsets[i], SeekOrigin.Begin);
//                    }
//                    int readed = source.Read(buffer, 0, blockLength); // check readed
//                    _readingQueue.Enqueue(new Block(i, buffer));
//                }

//                for (int i = 0; i < _threadCount; i++)
//                {
//                    _readingQueue.Enqueue(null); //ended task for flush file
//                }
//            }
//        }

        
        

//        public void WriteBytesToFile(string resultFileName)
//        {
//            var buffer = new List<FileBlock>(_threadCount);
//            int blockNumberToWrite = 0;
//            using (FileStream destination = new FileStream(resultFileName, FileMode.Create))
//            {
//                while (true)
//                {
//                    Block block;
//                    var blockIndex = buffer.FindIndex(b => b.Number == blockNumberToWrite);
//                    if (blockIndex < 0)
//                    {
//                        block = _writingQueue.Dequeue();
//                    }
//                    else
//                    {
//                        block = buffer[blockIndex];
//                        buffer.RemoveAt(blockIndex);
//                    }

//                    if (block == null)
//                    {
//                        break;
//                    }

//                    if (block.Number != blockNumberToWrite)
//                    {
//                        buffer.Add(block);
//                    }
//                    else
//                    {
//                        destination.Write(block.Bytes, 0, block.Bytes.Count());
//                        blockNumberToWrite++;
//                    }
//                }
//            }
//        }
        
//        void DecompressWork()
//        {
//            while (true)
//            {
//                var block = _readingQueue.Dequeue();
//                if (block == null)
//                {
//                    break;
//                }
//                block.Bytes = Decompress(block.Bytes);
//                _writingQueue.Enqueue(block);
//            }
//        }

        
//    }
//}
