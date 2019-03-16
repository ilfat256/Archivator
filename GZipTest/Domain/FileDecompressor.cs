using GZipTest.Core;
using System;
using System.IO;

namespace GZipTest.Commands
{
    public class FileDecompressor
    {
        private GZipOptions options;
        public FileDecompressor(GZipOptions options)
        {
            this.options = options;
        }
    }


}
