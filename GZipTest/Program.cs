using GZipTest.Core;
using System;
using System.Runtime.InteropServices;

namespace GZipTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;

            var availableRam = GetAvailableRamBytes();
            var options = new GZipOptions(args, availableRam, Environment.ProcessorCount);
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

        static int GetAvailableRamBytes()
        {
            ulong installedRamMemoryKB;
            GetPhysicallyInstalledSystemMemory(out installedRamMemoryKB);

            int maxRam32bitProcessKB = int.MaxValue / 1024;
            int availableRamBytes = installedRamMemoryKB > (ulong)maxRam32bitProcessKB ? int.MaxValue : (int)installedRamMemoryKB * 1024;
            return availableRamBytes;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPhysicallyInstalledSystemMemory(out ulong MemoryInKilobytes);
    }
}
