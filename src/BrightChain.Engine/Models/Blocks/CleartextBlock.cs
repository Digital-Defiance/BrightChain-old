namespace BrightChain.Engine.Models.Blocks
{
    using BrightChain.Engine.Models.Blocks.DataObjects;

    public class CleartextBlock : IdentifiableBlock
    {
        public CleartextBlock(BlockParams blockParams, ReadOnlyMemory<byte> cleartextData)
            : base(blockParams, cleartextData)
        {
        }
    }
}
