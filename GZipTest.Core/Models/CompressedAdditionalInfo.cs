using GZipTest.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;

namespace GZipTest.Core.Models
{
    internal class CompressedFileAdditionalInfo
    {
        private List<CompressedBlockInfo> blockOffsets;
        public int Lenght => blockOffsets.Count;
        
        public CompressedFileAdditionalInfo(int blocksCapacity)
        {
            blockOffsets = new List<CompressedBlockInfo>(blocksCapacity);
        }

        private CompressedFileAdditionalInfo(long[] blockOffsets, int[] blockLeghts, int blocksCapacity) : this(blocksCapacity)
        {
            for (int i = 0; i < blockOffsets.Length; i++)
            {
                this.blockOffsets.Add(new CompressedBlockInfo(i, blockOffsets[i], blockLeghts[i]));
            }
        }

        public void AddFrom(Block block, long blockOffset)
        {
            blockOffsets.Add(new CompressedBlockInfo(block.Number, blockOffset, block.Bytes.Length));
        }

        public byte[] ToByte()
        {
            byte[] blockOffsetsBytes = GetBlockOffsetsBytes();
            byte[] blockLenghtsBytes = GetBlockSizesBytes();
            byte[] blocksCountBytes = BitConverter.GetBytes(Lenght);
            var originalBlockOffsetsLenght = blockOffsetsBytes.Length;
            long lenght = blockOffsetsBytes.LongLength + blockLenghtsBytes.Length + blocksCountBytes.Length;
            if (lenght > int.MaxValue)
            {
                throw new AdditionalInfoException("Additional info size too big.");
            }

            Array.Resize<byte>(ref blockOffsetsBytes, (int)lenght);
            Array.Copy(blockLenghtsBytes, 0, blockOffsetsBytes, originalBlockOffsetsLenght, blockLenghtsBytes.Length);
            Array.Copy(blocksCountBytes, 0, blockOffsetsBytes, originalBlockOffsetsLenght + blockLenghtsBytes.Length, blocksCountBytes.Length);
            return blockOffsetsBytes;
        }

        private byte[] GetBlockOffsetsBytes()
        {
            var size = sizeof(long);
            byte[] offsets = new byte[blockOffsets.Count * size];
            foreach (var block in blockOffsets)
            {
                byte[] buffer = BitConverter.GetBytes(block.Offset);
                buffer.CopyTo(offsets, block.Number * size);
            }

            return offsets;
        }
                
        private byte[] GetBlockSizesBytes()
        {
            var size = sizeof(int);
            byte[] lenghts = new byte[blockOffsets.Count * size];
            foreach (var block in blockOffsets)
            {
                byte[] buffer = BitConverter.GetBytes(block.Length);
                buffer.CopyTo(lenghts, block.Number * size);
            }

            return lenghts;
        }

        public static CompressedFileAdditionalInfo GetFrom(Stream stream)
        {
            try
            {
                //check file is my format validate??
                int blocksCount = GetBlocksCount(stream);
                //get addit info lenght
                long BlocksOffsetsBytesCount = blocksCount * 8;
                long BlocksSizesBytesCount = blocksCount * 4;            
                //set position to start of additInfo
                stream.Seek(stream.Length - sizeof(int) - BlocksSizesBytesCount - BlocksOffsetsBytesCount, SeekOrigin.Begin);
                //read offsets
                byte[] blockOffsets = new byte[BlocksOffsetsBytesCount];
                stream.Read(blockOffsets, 0, blockOffsets.Length);
                long[] offsets = Deserialize(blockOffsets);
                //read leghts
                byte[] blockSizes = new byte[BlocksSizesBytesCount];
                stream.Read(blockSizes, 0, blockSizes.Length);
                int[] sizes = DeserializeLenght(blockSizes);

                return new CompressedFileAdditionalInfo(offsets, sizes, blocksCount);
            }
            catch (Exception ex)
            {
                throw new AdditionalInfoException($"Compressed file is not valid: {ex.Message}");
            }


        }

        private static int GetBlocksCount(Stream stream)
        {
            //blocksCount stored in last 4 bytes
            var blocksCountBytes = new byte[sizeof(int)];
            stream.Seek(stream.Length - 4, SeekOrigin.Begin);
            stream.Read(blocksCountBytes, 0, blocksCountBytes.Length);
            return BitConverter.ToInt32(blocksCountBytes, 0);
        }

        private static long[] Deserialize(byte[] additionalInfo)
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

        private static int[] DeserializeLenght(byte[] additionalInfo)
        {
            int offset = 0;
            int count = additionalInfo.Length / 4;
            int[] info = new int[count];
            for (int i = 0; i < count; i++)
            {
                info[i] = BitConverter.ToInt32(additionalInfo, offset);
                offset += 4;
            }
            return info;
        }

        public CompressedBlockInfo this[int index]
        {
            get
            {
                return blockOffsets[index];
            }
        }
    }
}
