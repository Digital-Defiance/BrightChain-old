namespace BrightChain.Engine.Models.Blocks.Chains
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using global::BrightChain.Engine.Exceptions;
    using global::BrightChain.Engine.Models.Blocks.DataObjects;
    using global::BrightChain.Engine.Models.Hashes;
    using global::BrightChain.Engine.Services.CacheManagers;
    using ProtoBuf;

    /// <summary>
    /// Brightened data chain, can be composed of file-based CBLs or brightened ChainLinq based data blocks.
    /// Although a BrightChain contains brightened data, the CBL block itself is not brightened.
    /// TODO: improve memory usage. Don't keep full copy, do all on async enumeration?
    /// </summary>
    [ProtoContract]
    public class BrightChain : ConstituentBlockListBlock, IEnumerable<BrightenedBlock>
    {
        private readonly IEnumerable<BrightenedBlock> _blocks;
        private readonly BrightenedBlock _head;
        private readonly BrightenedBlock _tail;
        private readonly int _count;

        public BrightChain(ConstituentBlockListBlockParams blockParams, IEnumerable<BrightenedBlock> brightenedBlocks)
            : base(blockParams)
        {
            if (!brightenedBlocks.Any())
            {
                throw new BrightChainException(nameof(brightenedBlocks));
            }

            this._blocks = new List<BrightenedBlock>(brightenedBlocks);
            this._head = brightenedBlocks.First();
            if (!this.VerifyHomogeneity(
                tail: out this._tail,
                blockCount: out this._count))
            {
                throw new BrightChainException(nameof(brightenedBlocks));
            }
        }

        public BrightChain(ConstituentBlockListBlockParams blockParams, BrightenedBlockCacheManager sourceCache)
            : base(blockParams)
        {
            if (!blockParams.ConstituentBlockHashes.Any())
            {
                throw new BrightChainException("Can not create empty chain");
            }

            var blocks = new List<BrightenedBlock>();
            int index = 0;
            foreach (var blockHash in blockParams.ConstituentBlockHashes)
            {
                var block = sourceCache.Get(blockHash);
                blocks.Add(block);
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
                throw new BrightChainException(nameof(blocks));
            }
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
                block.CompareOriginalType(this._head) &&
                block.GetType().Equals(this._head.GetType()) &&
                block.BlockSize.Equals(this._head.BlockSize);
        }

        /// <summary>
        /// Returns a Tuple of (BrightenedBlock, int) with the tail node and count.
        /// Future planning that this verification process will walk the stack and get the counts/tail anyway, regardless of Async/eager loaded.
        /// </summary>
        /// <returns></returns>
        public bool VerifyHomogeneity(out BrightenedBlock tail, out int blockCount)
        {
            if (this._head is null)
            {
                throw new BrightChainExceptionImpossible("Head is null despite having present hashes");
            }

            var allOk = true;
            int count = 0;
            BrightenedBlock movingTail = this._head;
            foreach (var block in this._blocks)
            {
                count++;
                movingTail = block;
                allOk = allOk && this.VerifyHomogeneityAgainstBlock(block);
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
            return this._blocks.Select(b => b.Id);
        }

        public async IAsyncEnumerable<BlockHash> IdsAsyncEnumerable()
        {
            foreach (var blockHash in this.Ids())
            {
                yield return blockHash;
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
    }
}
