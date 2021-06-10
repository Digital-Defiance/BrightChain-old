using BrightChain.Enumerations;
using BrightChain.Interfaces;
using BrightChain.Services;
using System;
using System.Security.Cryptography;

namespace BrightChain.Models.Blocks
{
    public class RandomizerBlock : Block, IBlock
    {
        public MemoryBlockCacheManager pregeneratedRandomizerCache { get; }

        public RandomizerBlock(MemoryBlockCacheManager pregeneratedRandomizerCache, DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data) :
            base(requestTime: requestTime, keepUntilAtLeast: keepUntilAtLeast, redundancy: redundancy, data: data)
        {
            this.pregeneratedRandomizerCache = pregeneratedRandomizerCache;
            this.pregeneratedRandomizerCache.Set(this.Id, this);
        }

        public override Block NewBlock(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> _)
        {
            // replace incoming data (will be empty byte array to fit conventions) with random data
            var rnd = new byte[BlockSizeMap.BlockSize(this.BlockSize)];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(rnd);
            ReadOnlyMemory<byte> data = new ReadOnlyMemory<byte>(rnd);
            return new MemoryBlock(
                cacheManager: this.pregeneratedRandomizerCache,
                requestTime: requestTime,
                keepUntilAtLeast: keepUntilAtLeast,
                redundancy: redundancy,
                data: data);
        }

        public override void Dispose()
        {

        }
    }
}