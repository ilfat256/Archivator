using GZipTest.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest
{
    public class GZipOptions
    {
        private FileInfo sourceFile;
        private FileInfo resultFile;
        public int FileStreamBufferSize { get; private set; }

        public List<string> SupportedCommands { get; } = new List<string> { "compress", "decompress" };
        public string CommandName { get; private set; }

        public int TaskCount { get; private set; }
        public int ThreadCount { get; private set; }

        public int ReadingBlockSize { get; private set; }
        public string SourceFileName => sourceFile.FullName;
        public long SourceFileBytesCount => sourceFile.Length;
        public string ResultFileName => resultFile.FullName;
        public long ResultFileBytesCount => resultFile.Length;

        public GZipOptions(string[] args)
        {
            if (args.Count() < 3)
            {
                PrintHelpMessage();
            }

            CommandName = args[0];
            SetCommand(CommandName);

            var sourceFileName = args[1];
            var resultFileName = args[2];
            SetFiles(sourceFileName, resultFileName);
            ConfigureResourceUtilizationOptions();
        }

        private void ConfigureResourceUtilizationOptions()
        {
            FileStreamBufferSize = 1024 * 10;
            ThreadCount = Environment.ProcessorCount;
            
            ulong installedRamMemoryBytes;
#warning check return?
            GetPhysicallyInstalledSystemMemory(out installedRamMemoryBytes);
#warning why 25?
            var tasksByThread = 5; //должны занимать, не больше памяти деленной на 2
            TaskCount = tasksByThread * ThreadCount;
            var originalFileReadingBlockSize = 1024 * 8;//installedRamMemoryBytes / 25 * 8; blockCount should be smarller int.maxvalue
            
            if (originalFileReadingBlockSize > int.MaxValue)
            {
                originalFileReadingBlockSize = int.MaxValue;
            }
            ReadingBlockSize = (int)originalFileReadingBlockSize;
        }

        private void SetCommand(string commandName)
        {
            if (!SupportedCommands.Contains(commandName, StringComparer.CurrentCultureIgnoreCase))
            {
                BadOption("command");
            };

            CommandName = commandName;
        }

        private void SetFiles(string sourceFileName, string resultFileName)
        {
            if (!File.Exists(sourceFileName))
            {
                BadOption("source-filename", "Source file is not found.");
            }

            if (File.Exists(resultFileName))
            {
                BadOption("result-filename", "Result file is exist.");
            }

            sourceFile = new FileInfo(sourceFileName);
            resultFile = new FileInfo(resultFileName);
        }

        private void PrintHelpMessage()
        {
            Console.WriteLine($"Usage: [command] [source-filename] [result-filename]");
            Console.WriteLine($"Options: ");
            Console.WriteLine($"command        supports 'compress' or 'decompress'");
            Environment.Exit(0);
        }

        private void BadOption(string badOption, string message = null)
        {
            Console.WriteLine($"Bad option {badOption}. {message}");
            PrintHelpMessage();
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPhysicallyInstalledSystemMemory(out ulong MemoryInKilobytes);
    }
}
