using BrightChain.Models.Blocks.DataObjects;
using System;

namespace BrightChain.Models.Blocks
{
    public class RestoredBlock : Block
    {
        public RestoredBlock(BlockParams blockArguments, ReadOnlyMemory<byte> data) : base(
            blockArguments: blockArguments,
            data: data)
        {
        }

        public override void Dispose()
        {

        }

        public override Block NewBlock(BlockParams blockArguments, ReadOnlyMemory<byte> data) =>
            new RestoredBlock(blockArguments, data);
    }
}
