namespace BrightChain.Engine.Faster.CacheManager
{
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Hashes;

    public partial class FasterBlockCacheManager
    {
        public override List<BlockHash> GetBlocksExpiringAt(long date)
        {
            var resultTuple = this.sessionContext.ExpirationSession.Read(date);
            if (resultTuple.status == FASTER.core.Status.OK)
            {
                return resultTuple.output;
            } else if (resultTuple.status == FASTER.core.Status.NOTFOUND)
            {
                return new List<BlockHash>();
            } else
            {
                throw new BrightChainException(resultTuple.status.ToString());
            }
        }

        public override void AddExpiration(BrightenedBlock block, bool noCheckContains = false)
        {
            if (!noCheckContains && !this.sessionContext.Contains(block.Id))
            {
                throw new IndexOutOfRangeException(block.Id.ToString());
            }

            var ticks = block.StorageContract.KeepUntilAtLeast.Ticks;
            var expiring = this.GetBlocksExpiringAt(ticks);
            if (!expiring.Contains(block.Id))
            {
                expiring.Add(block.Id);
            }

            this.sessionContext.ExpirationSession.Upsert(ticks, expiring);
        }

        public override void RemoveExpiration(BrightenedBlock block)
        {
            var ticks = block.StorageContract.KeepUntilAtLeast.Ticks;
            var expiring = this.GetBlocksExpiringAt(ticks);
            expiring.Remove(block.Id);
            this.sessionContext.ExpirationSession.Upsert(ticks, expiring);
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
