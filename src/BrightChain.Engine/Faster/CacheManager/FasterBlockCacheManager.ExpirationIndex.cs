namespace BrightChain.Engine.Faster.CacheManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Faster.Indices;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Hashes;

    public partial class FasterBlockCacheManager
    {
        public static string BlockExpirationIndexKey(long date)
        {
            return string.Format("Expiration:{0}", date);
        }

        public override IEnumerable<BlockHash> GetBlocksExpiringAt(long date)
        {
            using var sessionContext = this.NewFasterSessionContext;
            {
                var resultTuple = sessionContext.SharedCacheSession.Read(BlockExpirationIndexKey(date));
                if (resultTuple.status == FASTER.core.Status.OK)
                {
                    if (resultTuple.output is null)
                    {
                        throw new BrightChainExceptionImpossible("Cache returned null value");
                    }
                    else if (resultTuple.output is BlockExpirationIndexValue expirationIndex)
                    {
                        return expirationIndex.ExpiringHashes;
                    }
                    else
                    {
                        throw new BrightChainException("Unexpected index value type for key");
                    }
                }
                else if (resultTuple.status == FASTER.core.Status.NOTFOUND)
                {
                    return new BlockHash[] { };
                }

                throw new BrightChainException(resultTuple.status.ToString());
            }
        }

        public override void AddExpiration(BrightenedBlock block, bool noCheckContains = false)
        {
            using var sessionContext = this.NewFasterSessionContext;
            {
                if (!noCheckContains && !sessionContext.Contains(block.Id))
                {
                    throw new IndexOutOfRangeException(block.Id.ToString());
                }

                var ticks = block.StorageContract.KeepUntilAtLeast.Ticks;
                var expiring = (BlockHash[])this.GetBlocksExpiringAt(ticks);
                if (!expiring.Contains(block.Id))
                {
                    var size = expiring.Count();
                    Array.Resize(ref expiring, size + 1);
                    expiring[size] = block.Id;
                }

                sessionContext.SharedCacheSession.Upsert(
                    key: BlockExpirationIndexKey(ticks),
                    desiredValue: new BlockExpirationIndexValue(expiring));
            }
        }

        public override void RemoveExpiration(BrightenedBlock block)
        {
            var ticks = block.StorageContract.KeepUntilAtLeast.Ticks;
            var expiring = new List<BlockHash>(this.GetBlocksExpiringAt(ticks));
            expiring.Remove(block.Id);

            using var sessionContext = this.NewFasterSessionContext;
            {
                sessionContext.SharedCacheSession.Upsert(
                    key: BlockExpirationIndexKey(ticks),
                    desiredValue: new BlockExpirationIndexValue(expiring.ToArray()));
            }
        }

        public override void ExpireBlocks(long date)
        {
            using var sessionContext = this.NewFasterSessionContext;
            {

                // checkpoint
                foreach (var blockHash in this.GetBlocksExpiringAt(date))
                {
                    sessionContext.Drop(blockHash);
                }
                // checkpoint
            }
        }

        public override void ExpireBlocksThrough(long date)
        {
            // determine/lookup oldest block in cache
            // expire all seconds between, inclusive, that time and specified time
        }
    }
}
