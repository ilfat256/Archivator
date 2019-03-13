using GZipTest.Core;
using GZipTest.Domain;
using GZipTest.Exceptions;

namespace GZipTest
{
    public static class CommandFactory
    {
        internal static ICommand GetCommand(string commandName)
        {
            switch (commandName.ToUpper())
            {
                case "COMPRESS":
                    return new Compress();        
                case "DECOMPRESS":
                    return new Decompress();
                default:
                    throw new UnsupportedCommandException($"{commandName} is not supported.");
            }
        }
    }
}
