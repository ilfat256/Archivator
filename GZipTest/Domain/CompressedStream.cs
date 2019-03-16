using GZipTest.Core.Models;
using System;
using System.IO;

namespace GZipTest.Domain
{
    public class CompressingStream : IDisposable
    {
        private Stream stream;
        private CompressedAdditionalInfo additionalInfo = new CompressedAdditionalInfo();

        public CompressingStream(Stream stream)
        {
            if (!IsSupportedStream(stream))
            {
                throw new NotSupportedException("Passed stream must support Seek, Read, Write operations.");
            }
            this.stream = stream;
        }

        private bool IsSupportedStream(Stream stream)
        {
            return stream.CanSeek && stream.CanRead && stream.CanWrite;
        }

        public void Dispose()
        {
            byte[] info = additionalInfo.ToByte();
            stream.Write(info, 0, info.Length);
            stream.Dispose();
        }

        public void WriteBlock(Block block)
        {
            additionalInfo.AddFrom(block, stream.Position);
            stream.Write(block.Bytes, 0, block.Bytes.Length);
        }
    }
}
