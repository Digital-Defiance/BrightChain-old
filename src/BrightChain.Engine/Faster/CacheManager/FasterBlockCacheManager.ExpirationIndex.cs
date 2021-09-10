namespace BrightChain.Engine.Faster.CacheManager
{
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
            var resultTuple = this.sessionContext.CblIndicesSession.Read(BlockExpirationIndexKey(date));
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

        public override void AddExpiration(BrightenedBlock block, bool noCheckContains = false)
        {
            if (!noCheckContains && !this.sessionContext.Contains(block.Id))
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

            this.sessionContext.CblIndicesSession.Upsert(
                key: BlockExpirationIndexKey(ticks),
                desiredValue: new BlockExpirationIndexValue(expiring));
        }

        public override void RemoveExpiration(BrightenedBlock block)
        {
            var ticks = block.StorageContract.KeepUntilAtLeast.Ticks;
            var expiring = new List<BlockHash>(this.GetBlocksExpiringAt(ticks));
            expiring.Remove(block.Id);
            this.sessionContext.CblIndicesSession.Upsert(
                key: BlockExpirationIndexKey(ticks),
                desiredValue: new BlockExpirationIndexValue(expiring.ToArray()));
        }

        public override void ExpireBlocks(long date)
        {
            // checkpoint
            foreach (var blockHash in this.GetBlocksExpiringAt(date))
            {
                this.sessionContext.Drop(blockHash);
            }
            // checkpoint
        }

        public override void ExpireBlocksThrough(long date)
        {
            // determine/lookup oldest block in cache
            // expire all seconds between, inclusive, that time and specified time
        }
    }
}
