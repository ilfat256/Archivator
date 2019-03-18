using GZipTest.Core;
using GZipTest.Core.Compressing;
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
        public int TasksByTreadByDefault = 5;
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

            this.availableRamBytes = availableRam;
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
            availableRamBytes = availableRamBytes / 2;
            if (CommandName == "compress")
            {
                ReadingBlockSizeBytes = availableRamBytes / ThreadsCount;
                int maxBlockSizeBytes = 1 * 1024 * 1024;
                ReadingBlockSizeBytes = ReadingBlockSizeBytes > maxBlockSizeBytes ? maxBlockSizeBytes : ReadingBlockSizeBytes;               
            }
            else
            {
                using (Stream fileStream = new FileStream(SourceFileName, FileMode.Open, FileAccess.Read))
                {
                    using (BlockGZipStream gzipStream = new BlockGZipStream(fileStream, BlockGZipStreamMode.Decompressing))
                    {
                        ReadingBlockSizeBytes = gzipStream.GetMaxStoredBlockSizeBytes();
                    }
                }
            }

            if (availableRamBytes < ReadingBlockSizeBytes)
            {
                BadOption("source-filename", "Sorry. You don't have enough ram.");
            }

            if (availableRamBytes / ReadingBlockSizeBytes < ThreadsCount)
            {
                ThreadsCount = availableRamBytes / ReadingBlockSizeBytes;
            }

            var taskByThread = availableRamBytes / ReadingBlockSizeBytes / ThreadsCount;
            if (taskByThread > TasksByTreadByDefault)
            {
                taskByThread = TasksByTreadByDefault;
            }
            TaskCount = taskByThread * ThreadsCount;
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
