namespace GZipTest.Core.Models
{
    public class Block
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
