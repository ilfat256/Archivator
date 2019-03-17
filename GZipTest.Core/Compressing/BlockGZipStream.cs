using GZipTest.Core.Models;
using System;
using System.IO;

namespace GZipTest.Core.Compressing
{
    public class BlockGZipStream : IDisposable
    {
        private Stream stream;
        private CompressedFileAdditionalInfo additionalInfo;
        private bool shouldWriteAdditionalInfoOnDisposing;

        public int Lenght => additionalInfo.Lenght;

        public BlockGZipStream(Stream stream, BlockGZipStreamMode mode)
        {
            if (mode == BlockGZipStreamMode.Compressing)
            {
                additionalInfo = new CompressedFileAdditionalInfo(1000);
                shouldWriteAdditionalInfoOnDisposing = true;
            }
            else
            {
                additionalInfo = CompressedFileAdditionalInfo.GetFrom(stream);
            }
            IsSupportedStream(stream, mode);
            this.stream = stream;
        }

        private void IsSupportedStream(Stream stream, BlockGZipStreamMode mode)
        {
            if (!stream.CanSeek)
            {
                throw new NotSupportedException("Stream must support Seek operations.");
            }
            if (mode == BlockGZipStreamMode.Compressing && !stream.CanWrite)
            {
                throw new NotSupportedException("Stream stream must support Write operations.");
            }
            if (mode == BlockGZipStreamMode.Decompressing && !stream.CanRead)
            {
                throw new NotSupportedException("Stream stream must support Read operations.");
            }
        }

        public void Dispose()
        {
            if (shouldWriteAdditionalInfoOnDisposing)
            {
                byte[] info = additionalInfo.ToByte();
                stream.Write(info, 0, info.Length);
            }
            stream.Dispose();
        }

        public void WriteBlock(Block block)
        {
            additionalInfo.AddFrom(block, stream.Position);
            stream.Write(block.Bytes, 0, block.Bytes.Length);
        }

        public Block ReadBlock(int number)
        {
            var blockInfo = additionalInfo[number];
            int blockLength = blockInfo.Length;
            var buffer = new byte[blockLength];
            if (stream.Position != blockInfo.Offset)
            {
                stream.Seek(blockInfo.Offset, SeekOrigin.Begin);
            }
            int readed = stream.Read(buffer, 0, blockLength); // check readed
            return new Block(number, buffer);
        }
    }
}
