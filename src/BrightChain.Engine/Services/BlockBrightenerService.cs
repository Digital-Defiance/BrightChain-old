namespace BrightChain.Engine.Services
{
    using System.Linq;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Services.CacheManagers;

    /// <summary>
    /// This tiny little class is actually the lynchpin of the owner free filesystem.
    /// It is responsible for XORing blocks with random data blocks in order to correlate
    /// the user data with random data and other user blocks.
    /// </summary>
    public class BlockBrightenerService
    {
        /// <summary>
        /// This value that determines how many blocks get XOR'd with a given input block.
        /// TupleCount-1. Setting TupleCount = 5 will XOR input with 4 randomizers.
        /// </summary>
        public const byte TupleCount = 5;

        private readonly BlockCacheManager resultCache;

        public BlockBrightenerService(BlockCacheManager resultCache)
        {
            this.resultCache = resultCache;
        }

        /// <summary>
        /// Brightening is the process of XORing to correlate the data with block of random data and make it appear more random or "white" as white light is broad multi-spectrum light.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public BrightenedBlock Brighten(SourceBlock block, out Block[] randomizersUsed)
        {
            // the incoming block should be a raw disk block and is never used again
            randomizersUsed = new Block[TupleCount - 1];
            for (int i = 0; i < randomizersUsed.Length; i++)
            {
                // TODO: select or generate pre-generated random blocks (determine mixing)
                // for now just generate on demand, but these can be pre-seeded, and
                // technically any block in cache we haven't already used within a chain can be used.
                // TODO: add a mixing ratio and re-use blocks as appropriately as possible
                randomizersUsed[i] = new RandomizerBlock(
                    destinationCache: this.resultCache,
                    blockSize: block.BlockSize,
                    keepUntilAtLeast: block.StorageContract.KeepUntilAtLeast,
                    redundancyContractType: block.StorageContract.RedundancyContractType,
                    requestTime: block.StorageContract.RequestTime);
            }

            var xorBlock = block.XOR(randomizersUsed);
            return new BrightenedBlock(
                blockParams: new TransactableBlockParams(
                    cacheManager: this.resultCache,
                    allowCommit: true,
                    blockParams: block.BlockParams),
                data: xorBlock.Bytes,
                constituentBlocks: randomizersUsed.Select(b => b.Id).ToArray());
        }
    }
}
