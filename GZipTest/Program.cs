using GZipTest.Domain;
using System;

namespace GZipTest
{
    class Program
    {
        static void Main(string[] args)
        {
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
            var options = new GZipOptions(args);
            var compressor = new FileCompressor(options);
            compressor.CompressAndSaveFile();
            //var compressor = new MultitreadFileCompressor(4);
            //var resultFileName = @"C:\Users\Ilfat\source\repos\GZipTest\my.mp3.mzip";
            //compressor.CompressFile(@"C:\Users\Ilfat\source\repos\GZipTest\my.mp3", resultFileName);
            //compressor.DecompressFile(resultFileName, @"C:\Users\Ilfat\source\repos\GZipTest\my_decompressed.mp3");
            //UnauthorizedAccessException
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
