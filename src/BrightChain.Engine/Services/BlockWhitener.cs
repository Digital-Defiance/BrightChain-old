using System;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Blocks.DataObjects;

namespace BrightChain.Engine.Services
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

        public BlockWhitener(MemoryBlockCacheManager pregeneratedRandomizerCache)
        {
            this.pregeneratedRandomizerCache = pregeneratedRandomizerCache;
        }

        public Block Whiten(SourceBlock block)
        {
            // the incoming block should be a raw disk block and is never used again
            Block[] tupleStripeBlocks = new Block[TupleCount - 1];
            for (int i = 0; i < tupleStripeBlocks.Length; i++)
            {
                // TODO: select or generate pre-generated random blocks (determine mixing)
                // for now just generate on demand, but these can be pre-seeded
                tupleStripeBlocks[i] = new RandomizerBlock(
                    new TransactableBlockParams(
                    cacheManager: pregeneratedRandomizerCache,
                    blockArguments: new BlockParams(
                        blockSize: block.BlockSize,
                        requestTime: DateTime.Now,
                        keepUntilAtLeast: block.StorageContract.KeepUntilAtLeast,
                        redundancy: block.RedundancyContract.RedundancyContractType,
                        allowCommit: true,
                        privateEncrypted: false)));
            }
            return block.XOR(tupleStripeBlocks);
        }
    }
}
