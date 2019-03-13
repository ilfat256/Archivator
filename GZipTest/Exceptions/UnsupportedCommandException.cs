using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest.Exceptions
{
    class UnsupportedCommandException : Exception
    {
        public UnsupportedCommandException(string message) : base(message)
        {
        }
    }
}
