using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest.Exceptions
{
    class AdditionalInfoException : Exception
    {
        public AdditionalInfoException(string message) : base(message)
        {
        }
    }
}
