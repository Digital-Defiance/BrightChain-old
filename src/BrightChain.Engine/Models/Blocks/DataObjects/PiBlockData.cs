using System;
using System.Linq;
using BBP;

namespace BrightChain.Engine.Models.Blocks.DataObjects;

public class PiBlockData : BlockData
{
    public readonly int BlockSize;
    public readonly long PiOffset;

    public PiBlockData(long nOffset, int blockSize)
    {
        this.PiOffset = nOffset;
        this.BlockSize = blockSize;
    }

    public override ReadOnlyMemory<byte> Bytes => new ReadOnlyMemory<byte>(array: BBPCalculator.PiBytes(
        offsetInHexDigitChars: this.PiOffset,
        count: this.BlockSize).ToArray());
}
