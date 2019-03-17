namespace GZipTest.Core.Models
{
    public class CompressedBlockInfo
    {
        public long Offset { get; }
        public long Number { get; }
        public int Length { get; }
        public CompressedBlockInfo(long number, long offset, int lenght)
        {
            Offset = offset;
            Number = number;
            Length = lenght;
        }
    }
}
