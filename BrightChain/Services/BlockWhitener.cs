using BrightChain.Interfaces;
using BrightChain.Models.Blocks;
using System;

namespace BrightChain.Services
{
    /// <summary>
    /// This tiny little class is actually the lynchpin of the owner free filesystem.
    /// It is responsible for XORing blocks with random data blocks in order to correlate
    /// the user data with random data and other user blocks.
    /// </summary>
    public class BlockWhitener
    {
        public static byte TupleCount { get; } = 5;

        private MemoryBlockCacheManager pregeneratedRandomizerCache;

        public BlockWhitener(MemoryBlockCacheManager pregeneratedRandomizerCache) => this.pregeneratedRandomizerCache = pregeneratedRandomizerCache;

        public IBlock Whiten(SourceBlock block)
        {
            // the incoming block should be a raw disk block and is never used again
            Block[] tuples = new Block[TupleCount - 1];
            for (int i = 0; i < tuples.Length; i++)
            {
                // select or generate pre-generated random blocks
                // for now just generate on demand, but these can be pre-seeded
                tuples[i] = new RandomizerBlock(
                    pregeneratedRandomizerCache: this.pregeneratedRandomizerCache,
                    blockSize: block.BlockSize,
                    requestTime: DateTime.Now,
                    keepUntilAtLeast: block.StorageContract.KeepUntilAtLeast,
                    redundancy: block.RedundancyContract.RedundancyContractType,
                    allowCommit: true);
            }
            return block.XOR(tuples);
        }
    }
}
