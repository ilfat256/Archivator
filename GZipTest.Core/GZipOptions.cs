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
        private int availableRamBytes;

        public List<string> SupportedCommands { get; } = new List<string> { "compress", "decompress" };
        public string CommandName { get; private set; }
        public int TaskCount { get; private set; }
        public int ThreadsCount { get; private set; }
        public int ReadingBlockSizeBytes { get; private set; }
        public string SourceFileName => sourceFile.FullName;
        public long SourceFileBytesCount => sourceFile.Length;
        public string ResultFileName => resultFile.FullName;
        public long ResultFileBytesCount => resultFile.Length;

        public GZipOptions(string[] args, int availableRam, int availableThreadsCount)
        {
            if (args.Count() < 3)
            {
                PrintHelpMessage();
            }

            this.availableRamBytes = availableRam / 3 * 2;
            ThreadsCount = availableThreadsCount;
            CommandName = args[0];
            SetCommand(CommandName);

            var sourceFileName = args[1];
            var resultFileName = args[2];
            SetFiles(sourceFileName, resultFileName);
            ConfigureResourceUtilizationOptions();
        }

        private void ConfigureResourceUtilizationOptions()
        {
            var tasksByThread = 5;
            TaskCount = tasksByThread * ThreadsCount;
            ReadingBlockSizeBytes = availableRamBytes / TaskCount;
            int maxBlockSize = 10 * 1024 * 1024;
            ReadingBlockSizeBytes = ReadingBlockSizeBytes > maxBlockSize ? maxBlockSize : ReadingBlockSizeBytes;
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
    }
}
