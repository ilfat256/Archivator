using System;

namespace GZipTest.Core.Exceptions
{
    class UnsupportedCommandException : Exception
    {
        public UnsupportedCommandException(string message) : base(message)
        {
        }
    }
}
