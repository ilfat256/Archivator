using GZipTest.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GZipTest.Core.Models
{
    internal class CompressedAdditionalInfo
    {
        private List<CompressedBlockOffset> blockOffsets;
        public CompressedAdditionalInfo()
        {
            blockOffsets = new List<CompressedBlockOffset>();
        }

        public void AddFrom(Block block, long blockOffset)
        {
            blockOffsets.Add(new CompressedBlockOffset(block.Number, blockOffset, block.Bytes.Length));
        }

        public byte[] ToByte()
        {
            byte[] blockOffsets = GetBlockOffsetsBytes();
            byte[] blockLenghts = GetBlockSizesBytes();
            byte[] blocksCount = BitConverter.GetBytes(blockOffsets.Count());
            long lenght = (long)blockOffsets.Length + blockLenghts.Length + blocksCount.Length;
            if (lenght > int.MaxValue)
            {
                throw new AdditionalInfoException("Additional info size too big.");
            }

            Array.Resize<byte>(ref blockOffsets, (int)lenght);
            Array.Copy(blockLenghts, 0, blockOffsets, blockOffsets.Length, blockLenghts.Length);
            Array.Copy(blocksCount, 0, blockOffsets, blockOffsets.Length, blocksCount.Length);
            return blockOffsets;
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

        private long[] DeserializeLenght(byte[] additionalInfo)
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

        private long[] Deserialize(byte[] additionalInfo)
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
    }
}
