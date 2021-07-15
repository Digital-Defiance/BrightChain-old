using System.Collections.Generic;

namespace BrightChain.Models.Blocks.Chains
{
    public struct TupleStripe
    {
        public readonly int TupleCount { get; }
        public readonly IEnumerable<Block> Blocks { get; }

        public TupleStripe(int tupleCount, IEnumerable<Block> blocks)
        {
            TupleCount = tupleCount;
            Blocks = blocks;
        }
    }
}
