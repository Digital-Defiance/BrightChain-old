using System;

namespace BrightChain.Models.Blocks
{
    public class RestoredBlock : Block
    {
        public RestoredBlock(BlockArguments blockArguments, ReadOnlyMemory<byte> data) : base(
            blockArguments: blockArguments,
            data: data)
        {
        }

        public override void Dispose()
        {

        }

        public override Block NewBlock(BlockArguments blockArguments, ReadOnlyMemory<byte> data) =>
            new RestoredBlock(blockArguments, data);
    }
}
