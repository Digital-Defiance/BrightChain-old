using System;
using System.Collections.Generic;
using System.Linq;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Services.CacheManagers.Block;

namespace BrightChain.Engine.Models.Blocks.Chains;

/// <summary>
///     Represents a virtual map of all the contituent tuple-sets/blocks in a given source/reconstructed file. These cannot themselves be
///     committed to disk
///     The block datas may not actually be loaded in memory, but the appropriate blocks will be loaded (all non-local will be pulled into the
///     cache first) relative to their access offsets.
///     source file -> blockchainfilemap -> commit
///     pull blocks from pool -> blockchainfilemap -> read.
/// </summary>
public class BrightMap
{
    public BrightMap(ConstituentBlockListBlock cblBlock, IAsyncEnumerable<TupleStripe> tupleStripes = null)
    {
        this.ConstituentBlockListBlock = cblBlock;
        this.TupleStripes = tupleStripes;
    }

    private BrightMap()
    {
    }

    private IAsyncEnumerable<TupleStripe> TupleStripes { get; }

    public ConstituentBlockListBlock ConstituentBlockListBlock { get; }

    public static async IAsyncEnumerable<IEnumerable<T>> TakeIntoGroupsOf<T>(IEnumerable<T> list, int parts)
    {
        var i = 0;
        var items = new T[parts];
        foreach (var item in list)
        {
            items[i++] = item;

            if (i == parts)
            {
                yield return items;
                i = 0;
                items = new T[parts];
            }
        }

        Array.Resize(array: ref items,
            newSize: i);
        yield return items;
    }

    public static async IAsyncEnumerable<IEnumerable<T>> TakeIntoGroupsOf<T>(IAsyncEnumerable<T> list, int parts)
    {
        var i = 0;
        var items = new T[parts];
        await foreach (var item in list)
        {
            items[i++] = item;

            if (i == parts)
            {
                yield return items;
                i = 0;
                items = new T[parts];
            }
        }

        Array.Resize(array: ref items,
            newSize: i);
        yield return items;
    }

    public async IAsyncEnumerable<TupleStripe> ReconstructTupleStripes(BrightenedBlockCacheManagerBase blockCacheManager)
    {
        var constituentBlocks = this.ConstituentBlockListBlock.ConstituentBlocks;
        var constituentBlockCount = constituentBlocks.Count();
        if (constituentBlockCount == 0)
        {
            throw new BrightChainException(message: "No hashes in constituent block list");
        }

        if (constituentBlockCount % this.ConstituentBlockListBlock.TupleCount != 0)
        {
            throw new BrightChainException(message: "CBL length is not a multiple of the tuple count");
        }

        var tupleGroups = TakeIntoGroupsOf(list: constituentBlocks,
            parts: this.ConstituentBlockListBlock.TupleCount);
        await foreach (var tupleGroup in tupleGroups)
        {
            var blockList = new BrightenedBlock[this.ConstituentBlockListBlock.TupleCount];
            var i = 0;
            foreach (var blockHash in tupleGroup)
            {
                blockList[i++] = blockCacheManager.Get(blockHash: blockHash);
            }

            if (i == 0)
            {
                yield break;
            }

            yield return new TupleStripe(
                tupleCountMatch: this.ConstituentBlockListBlock.TupleCount,
                blockSizeMatch: this.ConstituentBlockListBlock.BlockSize,
                brightenedBlocks: blockList,
                originalType: this.ConstituentBlockListBlock.OriginalType);
        }
    }

    public async IAsyncEnumerable<Block> ConsolidateTuplesToChainAsync(BrightenedBlockCacheManagerBase blockCacheManager)
    {
        await foreach (var tupleStripe in this.TupleStripes is null
                           ? this.ReconstructTupleStripes(blockCacheManager: blockCacheManager)
                           : this.TupleStripes)
        {
            yield return tupleStripe.Consolidate();
        }
    }

    public static async IAsyncEnumerator<byte> ReadValidatedChainToBytes(IAsyncEnumerable<Block> source)
    {
        await foreach (var block in source)
        {
            if (!block.Validate())
            {
                throw new BrightChainValidationEnumerableException(exceptions: block.ValidationExceptions,
                    message: block.Id.ToString());
            }

            foreach (var b in block.Bytes.ToArray())
            {
                yield return b;
            }
        }
    }
}
