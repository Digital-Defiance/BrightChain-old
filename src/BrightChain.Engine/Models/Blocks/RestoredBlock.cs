namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using BrightChain.Engine.Models.Blocks.DataObjects;

    public class RestoredBlock : Block
    {
        public RestoredBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
            : base(blockParams: blockParams, data: data)
        {
        }

        public override void Dispose()
        {

        }

        public override Block NewBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
        {
            return new RestoredBlock(blockParams, data);
        }
    }
}
