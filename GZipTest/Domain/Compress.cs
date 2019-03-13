using GZipTest.Core;
using System;
using System.IO;

namespace GZipTest.Domain
{
    public class Compress : ICommand
    {
        private FileInfo fileInfo;
        public Compress(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
        }

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
