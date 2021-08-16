namespace BrightChain.Engine.Models.Blocks.Chains
{
    using System.Collections.Generic;
    using global::BrightChain.Engine.Enumerations;
    using global::BrightChain.Engine.Exceptions;

    /// <summary>
    /// A tuple stripe is a representation of the blocks used to brighten or recover a source block.
    /// </summary>
    public struct TupleStripe
    {
        /// <summary>
        /// Gets the blocks within the stripd.
        /// Within a stripel block order doesn't technically matter.
        /// </summary>
        public readonly IEnumerable<Block> Blocks { get; }

        public TupleStripe(int tupleCountMatch, BlockSize blockSizeMatch, IEnumerable<Block> blocks)
        {
            if (tupleCountMatch != ((Block[])blocks).Length)
            {
                throw new BrightChainException("Block length mismatch");
            }

            foreach (Block block in blocks)
            {
                if (blockSizeMatch != block.BlockSize)
                {
                    throw new BrightChainException("block size mismatch");
                }
            }

            this.Blocks = blocks;
        }

        /// <summary>
        /// XOR's the stripe's blocks back into a SourceBlock.
        /// </summary>
        /// <returns></returns>
        public SourceBlock Consolidate()
        {
            var blocks = (Block[])this.Blocks;
            Block result = blocks[0];
            for (int i = 1; i < blocks.Length; i++)
            {
                result = result.XOR(blocks[i]);
            }

            return new SourceBlock(result.BlockParams, result.Bytes);
        }
    }
}
