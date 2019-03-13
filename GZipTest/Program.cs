using GZipTest.Core;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using GZipTest.Domain;

namespace GZipTest
{
    class Program
    {
        static void Main(string[] args)
        {
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
            ParseOptions(args);
            //var compressor = new MultitreadFileCompressor(4);
            //var resultFileName = @"C:\Users\Ilfat\source\repos\GZipTest\my.mp3.mzip";
            //compressor.CompressFile(@"C:\Users\Ilfat\source\repos\GZipTest\my.mp3", resultFileName);
            //compressor.DecompressFile(resultFileName, @"C:\Users\Ilfat\source\repos\GZipTest\my_decompressed.mp3");
            //UnauthorizedAccessException


        }

        private static ParralelWorkerOptions ParseOptions(string[] args)
        {
            if (args.Count() < 3)
            {
                PrintHelpMessage();
            }

            var commandName = args[0];
            Type commandType = typeof(ICommand);
            var supportedCommands = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => commandType.IsAssignableFrom(type) && type.IsClass)
                .Select(type => type.Name)
                .ToList();

            if (!supportedCommands.Contains(commandName, StringComparer.CurrentCultureIgnoreCase))
            {
                BadOption("command");
            }

            return new ParralelWorkerOptions(commandName, args[1], args[2]);
        }

        private static void PrintHelpMessage()
        {
            Console.WriteLine($"Usage: [command] [source-filename] [result-filename]");
            Console.WriteLine($"Options: ");
            Console.WriteLine($"command        supports 'compress' or 'decompress'");
            Environment.Exit(0);
        }

        private static void BadOption(string badOption)
        {
            Console.WriteLine($"Bad option {badOption}.");
            PrintHelpMessage();
        }

        static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
            Console.WriteLine("Press Enter any key...");
            Console.ReadKey();
            Environment.Exit(1);
        }
    }
}
