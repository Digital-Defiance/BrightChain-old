namespace BrightChain.Engine.Models.Blocks.Chains
{
    using System.Collections.Generic;
    using global::BrightChain.Engine.Enumerations;
    using global::BrightChain.Engine.Exceptions;
    using global::BrightChain.Engine.Models.Blocks.DataObjects;

    /// <summary>
    /// A tuple stripe is a representation of the blocks used to brighten or recover a source block.
    /// In each row, only the last block is expected to be the non-randomizer block, but order doesn't matter within a stripe.
    /// </summary>
    public struct TupleStripe
    {
        /// <summary>
        /// Gets the blocks within the stripd.
        /// Within a stripel block order doesn't technically matter.
        /// </summary>
        public readonly IEnumerable<Block> Blocks { get; }

        private readonly BlockSize blockSize;

        private readonly Type OriginalType;

        public TupleStripe(int tupleCountMatch, BlockSize blockSizeMatch, IEnumerable<Block> blocks, Type originalType)
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
            this.blockSize = blockSizeMatch;
            this.OriginalType = originalType;
        }

        /// <summary>
        /// XOR's the stripe's blocks back into an Identifiable Block.
        /// </summary>
        /// <returns></returns>
        public IdentifiableBlock Consolidate()
        {
            Block result = this.Blocks.First();
            return new IdentifiableBlock(result.BlockParams, result.XOR(this.Blocks));
        }

        public BrightHandle Handle
        {
            get
            {
                return new BrightHandle(
                blockSize: this.blockSize,
                blockHashes: this.Blocks
                    .Select(b => b.Id)
                    .ToArray(),
                originalType: this.OriginalType);
            }
        }
    }
}
