using GZipTest.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZipTest.Core
{
    public class MultitreadFileCompressor
    {
        private ConcurrentQueueBuffer<Block> _readingQueue;
        private ConcurrentQueueBuffer<Block> _writingQueue;
        private Worker _worker;
        private int _readingBufferSize;
        private int _threadCount;

        public MultitreadFileCompressor(int threadCount)
        {
            _readingBufferSize = 999;
            _readingQueue = new ConcurrentQueueBuffer<Block>(20);
            _writingQueue = new ConcurrentQueueBuffer<Block>(20);
            _threadCount = threadCount;
            _worker = new Worker(threadCount);
        }

        public void CompressFile(string sourceFileName, string resultFileName)
        {
            try
            {
                var readingThread = new Thread(() => ReadBytesToQueue(sourceFileName));
                readingThread.Name = "readingThread";
                readingThread.Start();
                //in compressed block we can store size of block
                _worker.Start(() => CompressWork(), "CompressWork ");
                var writingThread = new Thread(() => WriteCompressedBytesToFile(resultFileName));
                writingThread.Name = "writingCompressedThread";
                writingThread.Start();
                _worker.Join();
                _writingQueue.Enqueue(null); //for flushing writing
                writingThread.Join();

                //check access to file .CanWrite / Read etc
                //check if file excist IOException
                //using (GZipStream gzipStream = new GZipStream(destination, CompressionMode.Compress))
                //{
                //    gzipStream.Write(buffer, 0, _readingBufferSize);
                //}
                //all work done

                DecompressFile(resultFileName, @"C:\Users\Ilfat\source\repos\GZipTest\sample_decompressed.txt");
            }
            catch (IOException exception)
            {
                throw exception;
            }
        }

        public void DecompressFile(string sourceFileName, string resultFileName)
        {
            try
            {
                _readingQueue = new ConcurrentQueueBuffer<Block>(20);
                _writingQueue = new ConcurrentQueueBuffer<Block>(20);
                var readingThread = new Thread(() => ReadCompressedBytesToQueue(sourceFileName));
                readingThread.Name = "readingCompressedThread";
                readingThread.Start();
                _worker.Start(() => DecompressWork(), "DeCompressWork ");
                var writingThread = new Thread(() => WriteBytesToFile(resultFileName));
                writingThread.Name = "writingThread";
                writingThread.Start();
                _worker.Join();
                _writingQueue.Enqueue(null); //for flushing writing
                writingThread.Join();
                //all work done
            }
            catch (IOException exception)
            {
                throw exception;
            }
        }


        public void ReadCompressedBytesToQueue(string sourceFileName)
        {
            using (Stream source = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read)) //buffer size of FileStream & Should be closed after closing adapter
            {
                long fileSize = source.Length;

                //offset information length                
                //read last 4 bytes

                //check file is my format validate
                var blocksCountBytes = new byte[4];
                source.Seek(fileSize - 4, SeekOrigin.Begin);
                source.Read(blocksCountBytes, 0, blocksCountBytes.Length);

                long blocksCount = BitConverter.ToInt32(blocksCountBytes, 0);
                long blockOffsetsArrayLeght = blocksCount * 8;
                long blockLeghtsArrayLeght = blocksCount * 4;
                var blockOffsets = new byte[blockOffsetsArrayLeght];
                var blockLeghts = new byte[blockLeghtsArrayLeght];
                source.Seek(fileSize - blockLeghtsArrayLeght - blockOffsetsArrayLeght - 4, SeekOrigin.Begin);
                source.Read(blockOffsets, 0, blockOffsets.Length);
                var offsets = Deserialize(blockOffsets);
                source.Read(blockLeghts, 0, blockLeghts.Length);
                var leghts = DeserializeLenght(blockLeghts);

                for (long i = 0; i < blocksCount; i++)
                {
                    int blockLength = (int)leghts[i]; //probably there is int
                    var buffer = new byte[blockLength];
                    if (source.Position != offsets[i])
                    {
                        source.Seek(offsets[i], SeekOrigin.Begin);
                    }
                    int readed = source.Read(buffer, 0, blockLength); // check readed
                    _readingQueue.Enqueue(new Block(i, buffer));
                }

                for (int i = 0; i < _threadCount; i++)
                {
                    _readingQueue.Enqueue(null); //ended task for flush file
                }
            }
        }

        public void ReadBytesToQueue(string sourceFileName)
        {
            using (Stream source = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read)) //buffer size of FileStream & Should be closed after closing adapter
            {
                long fileSize = source.Length;
                long blockCount = fileSize / _readingBufferSize;
                for (long i = 0; i < blockCount; i++)
                {
                    var buffer = new byte[_readingBufferSize];
                    int readed = source.Read(buffer, 0, _readingBufferSize);
                    _readingQueue.Enqueue(new Block(i, buffer));
                }

                int remainingBytes = (int)(fileSize % _readingBufferSize);
                if (remainingBytes != 0)
                {
                    var buffer = new byte[remainingBytes];
                    int readed = source.Read(buffer, 0, remainingBytes);
                    _readingQueue.Enqueue(new Block(blockCount, buffer));
                }

                for (int i = 0; i < _threadCount; i++)
                {
                    _readingQueue.Enqueue(null); //ended task for flush file
                }
            }
        }
        CompressedAdditionalInfo info = new CompressedAdditionalInfo();
        public void WriteCompressedBytesToFile(string resultFileName)
        {
            using (FileStream destination = new FileStream(resultFileName, FileMode.Create))
            {
                while (true)
                {
                    var block = _writingQueue.Dequeue();
                    if (block == null)
                    {
                        break;
                    }
                    info.BlockOffsets.Add(new CompressedBlockOffset(block.Number, destination.Position, block.Bytes.Length));
                    destination.Write(block.Bytes, 0, block.Bytes.Count());
                }

                var offsetsArray = Serialize(info);
                var lengthsArray = SerializeLenghts(info);
                byte[] length = BitConverter.GetBytes(info.BlockOffsets.Count());
                destination.Write(offsetsArray, 0, offsetsArray.Count());
                destination.Write(lengthsArray, 0, lengthsArray.Count());
                destination.Write(length, 0, length.Count());
            }
        }

        public void WriteBytesToFile(string resultFileName)
        {
            var buffer = new List<Block>(_threadCount);
            int blockNumberToWrite = 0;
            using (FileStream destination = new FileStream(resultFileName, FileMode.Create))
            {
                while (true)
                {
                    Block block;
                    var blockIndex = buffer.FindIndex(b => b.Number == blockNumberToWrite);
                    if (blockIndex < 0)
                    {
                        block = _writingQueue.Dequeue();
                    }
                    else
                    {
                        block = buffer[blockIndex];
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
                        destination.Write(block.Bytes, 0, block.Bytes.Count());
                        blockNumberToWrite++;
                    }
                }
            }
        }

        byte[] Serialize(CompressedAdditionalInfo info)
        {
            byte[] offsets = new byte[info.BlockOffsets.Count * 8];
            foreach (var block in info.BlockOffsets)
            {
                byte[] buffer = BitConverter.GetBytes(block.Offset);
                buffer.CopyTo(offsets, block.Number * 8);
            }

            return offsets;
        }

        byte[] SerializeLenghts(CompressedAdditionalInfo info)
        {
            byte[] lenghts = new byte[info.BlockOffsets.Count * 4];
            foreach (var block in info.BlockOffsets)
            {
                byte[] buffer = BitConverter.GetBytes(block.Length);
                buffer.CopyTo(lenghts, block.Number * 4);
            }

            return lenghts;
        }

        long[] DeserializeLenght(byte[] additionalInfo)
        {
            int offset = 0;
            int count = additionalInfo.Length / 4;
            long[] info = new long[count];
            for (int i = 0; i < count; i++)
            {
                info[i] = BitConverter.ToInt32(additionalInfo, offset);
                offset += 4;
            }
            return info;
        }

        long[] Deserialize(byte[] additionalInfo)
        {
            int offset = 0;
            int count = additionalInfo.Length / 8;
            long[] info = new long[count];
            for (int i = 0; i < count; i++)
            {
                info[i] = BitConverter.ToInt64(additionalInfo, offset);
                offset += 8;
            }
            return info;
        }

        void CompressWork()
        {
            while (true)
            {
                var block = _readingQueue.Dequeue();
                if (block == null)
                {
                    break;
                }
                block.Bytes = Compress(block.Bytes);
                _writingQueue.Enqueue(block);
            }
        }

        void DecompressWork()
        {
            while (true)
            {
                var block = _readingQueue.Dequeue();
                if (block == null)
                {
                    break;
                }
                block.Bytes = Decompress(block.Bytes);
                _writingQueue.Enqueue(block);
            }
        }

        byte[] Compress(byte[] data)
        {
            using (var compressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(data, 0, data.Length);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }

        byte[] Decompress(byte[] data)
        {
            using (var compressedStream = new MemoryStream(data))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }
    }
}
