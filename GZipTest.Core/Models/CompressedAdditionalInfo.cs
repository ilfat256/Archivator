using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest.Core.Models
{
    [Serializable]
    public class CompressedAdditionalInfo
    {
        public List<CompressedBlockOffset> BlockOffsets { get; set; }
        public CompressedAdditionalInfo()
        {
            BlockOffsets = new List<CompressedBlockOffset>();
        }
    }
}
