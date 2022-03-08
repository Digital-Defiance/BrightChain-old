using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Models.Hashes;
using BrightChain.Engine.Services.CacheManagers.Block;
using ProtoBuf;

namespace BrightChain.Engine.Models.Blocks.Chains;

/// <summary>
///     Brightened data chain, can be composed of file-based CBLs or brightened ChainLinq based data blocks.
///     Although a BrightChain contains brightened data, the CBL block itself is not brightened.
///     TODO: improve memory usage. Don't keep full copy, do all on async enumeration?
/// </summary>
[ProtoContract]
public class BrightChain : ConstituentBlockListBlock, IEnumerable<BrightenedBlock>
{
    private readonly IEnumerable<BrightenedBlock> _blocks;
    private readonly int _count;
    private readonly BrightenedBlock _head;
    private readonly BrightenedBlock _tail;

    public BrightChain(ConstituentBlockListBlockParams blockParams, IEnumerable<BrightenedBlock> brightenedBlocks)
        : base(blockParams: blockParams)
    {
        if (!brightenedBlocks.Any())
        {
            throw new BrightChainException(message: nameof(brightenedBlocks));
        }

        this._blocks = new List<BrightenedBlock>(collection: brightenedBlocks);
        this._head = brightenedBlocks.First();
        if (!this.VerifyHomogeneity(
                tail: out this._tail,
                blockCount: out this._count))
        {
            throw new BrightChainException(message: nameof(brightenedBlocks));
        }
    }

    public BrightChain(ConstituentBlockListBlockParams blockParams, BrightenedBlockCacheManagerBase sourceCache)
        : base(blockParams: blockParams)
    {
        if (!blockParams.ConstituentBlockHashes.Any())
        {
            throw new BrightChainException(message: "Can not create empty chain");
        }

        var blocks = new List<BrightenedBlock>();
        var index = 0;
        foreach (var blockHash in blockParams.ConstituentBlockHashes)
        {
            var block = sourceCache.Get(blockHash: blockHash);
            blocks.Add(item: block);
            if (index++ == 0)
            {
                this._head = block;
            }
        }

        this._blocks = blocks;
        if (!this.VerifyHomogeneity(
                tail: out this._tail,
                blockCount: out this._count))
        {
            throw new BrightChainException(message: nameof(blocks));
        }
    }

    public IEnumerator<BrightenedBlock> GetEnumerator()
    {
        return this._blocks.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this._blocks.GetEnumerator();
    }

    public int Count()
    {
        return this._count;
    }

    public BrightenedBlock First()
    {
        return this._head;
    }

    public bool VerifyHomogeneityAgainstBlock(BrightenedBlock block)
    {
        return
            block.ValidateOriginalType() &&
            block.CompareOriginalType(other: this._head) &&
            block.GetType().Equals(o: this._head.GetType()) &&
            block.BlockSize.Equals(obj: this._head.BlockSize);
    }

    /// <summary>
    ///     Returns a Tuple of (BrightenedBlock, int) with the tail node and count.
    ///     Future planning that this verification process will walk the stack and get the counts/tail anyway, regardless of Async/eager loaded.
    /// </summary>
    /// <returns></returns>
    public bool VerifyHomogeneity(out BrightenedBlock tail, out int blockCount)
    {
        if (this._head is null)
        {
            throw new BrightChainExceptionImpossible(message: "Head is null despite having present hashes");
        }

        var allOk = true;
        var count = 0;
        var movingTail = this._head;
        foreach (var block in this._blocks)
        {
            count++;
            movingTail = block;
            allOk = allOk && this.VerifyHomogeneityAgainstBlock(block: block);
        }

        blockCount = count;
        tail = movingTail;
        return allOk;
    }

    public BrightenedBlock Last()
    {
        return this._tail;
    }

    public IEnumerable<BrightenedBlock> All()
    {
        return this._blocks;
    }

    public async IAsyncEnumerator<BrightenedBlock> AllAsyncEnumerable()
    {
        foreach (var block in this._blocks)
        {
            yield return block;
        }
    }

    public IEnumerable<BlockHash> Ids()
    {
        return this._blocks.Select(selector: b => b.Id);
    }

    public async IAsyncEnumerable<BlockHash> IdsAsyncEnumerable()
    {
        foreach (var blockHash in this.Ids())
        {
            yield return blockHash;
        }
    }
}
