using GZipTest.Core;
using System;
using System.IO;

namespace GZipTest.Domain
{
    public class Decompress : ICommand
    {
        private FileInfo fileInfo;
        public Decompress(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
        }

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
