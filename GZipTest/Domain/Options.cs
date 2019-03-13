using GZipTest.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest.Domain
{
    public class ParralelWorkerOptions
    {
        public ICommand Command { get; }
        public FileInfo SourceFile { get; }
        public FileInfo ResultFile { get; }
        public uint TasksCount { get; }
        public uint ReadingBlockSize { get; }
        public long FileStreamBufferSize { get;}
        public int ThreadCount => Environment.ProcessorCount;

        public ParralelWorkerOptions(string command, string sourceFileName, string resultFileName)
        {
            Command = CommandFactory.GetCommand(command);

            if (!File.Exists(sourceFileName))
            {
                throw new IOException("Source file is not found.");
            }

            if (File.Exists(resultFileName))
            {
                throw new IOException("Result file is exist.");
            }

            SourceFile = new FileInfo(sourceFileName);
            SourceFile = new FileInfo(resultFileName);
        }

        private void ConfigureResourceUtilizationOptions()
        {
            ulong installedRamMemoryBytes;
#warning check return?
            GetPhysicallyInstalledSystemMemory(out installedRamMemoryBytes);
        }

#warning disposing this?
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPhysicallyInstalledSystemMemory(out ulong MemoryInKilobytes);
    }
}
