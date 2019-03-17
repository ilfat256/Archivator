using GZipTest.Core;
using System;

namespace GZipTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
            var options = new GZipOptions(args);
            switch (options.CommandName)
            {
                case "compress":
                    var compressor = new FileCompressor(options);
                    compressor.CompressAndSaveFile();
                    break;
                case "decompress":
                    var decompressor = new FileDecompressor(options);
                    decompressor.DecompressAndSaveFile();
                    break;
            }
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
