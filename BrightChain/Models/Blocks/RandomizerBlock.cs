using BrightChain.Enumerations;
using BrightChain.Services;
using System;
using System.Security.Cryptography;

namespace BrightChain.Models.Blocks
{
    /// <summary>
    /// Input blocks to the whitener service that consist of purely CSPRNG data of the specified block size
    /// </summary>
    public class RandomizerBlock : TransactableBlock
    {
        public MemoryBlockCacheManager pregeneratedRandomizerCache { get; }

        public RandomizerBlock(MemoryBlockCacheManager pregeneratedRandomizerCache, DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data, bool allowCommit) :
            base(
                cacheManager: pregeneratedRandomizerCache,
                requestTime: requestTime,
                keepUntilAtLeast: keepUntilAtLeast,
                redundancy: redundancy,
                data: data,
                allowCommit: allowCommit)
        {
            this.pregeneratedRandomizerCache.Set(this.Id, this);
        }

        public override Block NewBlock(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> _, bool allowCommit)
        {
            // replace incoming data (will be empty byte array to fit conventions) with random data
            var rnd = new byte[BlockSizeMap.BlockSize(this.BlockSize)];
            using (var rng = RandomNumberGenerator.Create()) // TODO: guarantee is CSPRNG
                rng.GetBytes(rnd);
            ReadOnlyMemory<byte> data = new ReadOnlyMemory<byte>(rnd);
            return new MemoryBlock(
                cacheManager: this.pregeneratedRandomizerCache,
                requestTime: requestTime,
                keepUntilAtLeast: keepUntilAtLeast,
                redundancy: redundancy,
                data: data,
                allowCommit: allowCommit);
        }

        public override void Dispose()
        {

        }
    }
}