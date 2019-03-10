using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest.Core.Models
{
    [Serializable]
    public class Block //struct?
    {
        public long Number { get; }
        public byte[] Bytes { get; set; }

        public Block(long number, byte[] bytes)
        {
            Number = number;
            Bytes = bytes;
        }
    }
}
