using System;
using BrightChain.Engine.Models.Blocks.DataObjects;

namespace BrightChain.Engine.Models.Blocks
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

        public override Block NewBlock(BlockParams blockArguments, ReadOnlyMemory<byte> data)
        {
            return new RestoredBlock(blockArguments, data);
        }
    }
}
