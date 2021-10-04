namespace BrightChain.Engine.Models.Blocks.DataObjects
{
    using System;
    using System.Linq;
    using BBP;

    public class PiBlockData : BlockData
    {
        public readonly long PiOffset;
        public readonly int BlockSize;

        public PiBlockData(long nOffset, int blockSize)
        {
            this.PiOffset = nOffset;
            this.BlockSize = blockSize;
        }

        public override ReadOnlyMemory<byte> Bytes
        {
            get
            {
                PiDigit pd = new PiDigit(nOffset: this.PiOffset);
                return new ReadOnlyMemory<byte>(pd.PiBytes(
                    n: this.PiOffset,
                    count: this.BlockSize).ToArray());
            }
        }
    }
}
