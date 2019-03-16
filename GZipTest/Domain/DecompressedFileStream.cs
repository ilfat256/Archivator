using GZipTest.Core.Models;
using System;
using System.IO;

namespace GZipTest.Domain
{
    public class DecompressedFileStream : IDisposable
    {
        private Stream source;
        public DecompressedFileStream(string sourceFileName, int fileStreamBufferSize)
        {
            //buffer size of FileStream & Should be closed after closing adapter
            source = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.None, fileStreamBufferSize);
        }

        public void Dispose()
        {
            source.Dispose();
        }

        public byte[] ReadBytes(int byteCount)
        {
            var buffer = new byte[byteCount];
            int readed = source.Read(buffer, 0, byteCount);
            return buffer;
        }
    }
}
