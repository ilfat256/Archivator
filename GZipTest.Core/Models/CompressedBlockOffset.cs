using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest.Core.Models
{
    public class CompressedBlockOffset
    {
        public long Offset { get; }
        public long Number { get; }
        public int Length { get; }
        public CompressedBlockOffset(long number, long offset, int lenght)
        {
            Offset = offset;
            Number = number;
            Length = lenght;
        }
    }
}
