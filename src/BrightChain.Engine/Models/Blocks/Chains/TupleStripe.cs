using System.Collections.Generic;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Exceptions;

namespace BrightChain.Engine.Models.Blocks.Chains
{
    public struct TupleStripe
    {
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

        public SourceBlock Consolidate()
        {
            var blocks = (Block[])this.Blocks;
            Block result = blocks[0];
            for (int i = 1; i < blocks.Length; i++)
            {
                result = result.XOR(blocks[i]);
            }

            return new SourceBlock(result.BlockParams, result.Data);
        }
    }
}
