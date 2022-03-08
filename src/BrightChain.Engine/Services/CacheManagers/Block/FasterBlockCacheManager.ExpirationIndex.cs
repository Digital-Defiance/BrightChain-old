using System;
using System.Collections.Generic;
using System.Linq;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Faster.Indices;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Hashes;
using FASTER.core;

namespace BrightChain.Engine.Faster.CacheManager;

public partial class FasterBlockCacheManager
{
    public static string BlockExpirationIndexKey(long date)
    {
        return string.Format(format: "Expiration:{0}",
            arg0: date);
    }

    public override IEnumerable<BlockHash> GetBlocksExpiringAt(long date)
    {
        using var sessionContext = this.NewFasterSessionContext;
        {
            var resultTuple = sessionContext.SharedCacheSession.Read(key: BlockExpirationIndexKey(date: date));
            if (resultTuple.status == Status.OK)
            {
                if (resultTuple.output is null)
                {
                    throw new BrightChainExceptionImpossible(message: "Cache returned null value");
                }

                if (resultTuple.output is BlockExpirationIndexValue expirationIndex)
                {
                    return expirationIndex.ExpiringHashes;
                }

                throw new BrightChainException(message: "Unexpected index value type for key");
            }

            if (resultTuple.status == Status.NOTFOUND)
            {
                return new BlockHash[] { };
            }

            throw new BrightChainException(message: resultTuple.status.ToString());
        }
    }

    public override void AddExpiration(BrightenedBlock block, bool noCheckContains = false)
    {
        using var sessionContext = this.NewFasterSessionContext;
        {
            if (!noCheckContains && !sessionContext.Contains(blockHash: block.Id))
            {
                throw new IndexOutOfRangeException(message: block.Id.ToString());
            }

            var ticks = block.StorageContract.KeepUntilAtLeast.Ticks;
            var expiring = (BlockHash[])this.GetBlocksExpiringAt(date: ticks);
            if (!expiring.Contains(value: block.Id))
            {
                var size = expiring.Count();
                Array.Resize(array: ref expiring,
                    newSize: size + 1);
                expiring[size] = block.Id;
            }

            sessionContext.SharedCacheSession.Upsert(
                key: BlockExpirationIndexKey(date: ticks),
                desiredValue: new BlockExpirationIndexValue(hashes: expiring));
        }
    }

    public override void RemoveExpiration(BrightenedBlock block)
    {
        var ticks = block.StorageContract.KeepUntilAtLeast.Ticks;
        var expiring = new List<BlockHash>(collection: this.GetBlocksExpiringAt(date: ticks));
        expiring.Remove(item: block.Id);

        using var sessionContext = this.NewFasterSessionContext;
        {
            sessionContext.SharedCacheSession.Upsert(
                key: BlockExpirationIndexKey(date: ticks),
                desiredValue: new BlockExpirationIndexValue(hashes: expiring.ToArray()));
        }
    }

    public override void ExpireBlocks(long date)
    {
        using var sessionContext = this.NewFasterSessionContext;
        {
            // checkpoint
            foreach (var blockHash in this.GetBlocksExpiringAt(date: date))
            {
                sessionContext.Drop(blockHash: blockHash);
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
