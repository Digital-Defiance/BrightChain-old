namespace BrightChain.Engine.Models.Blocks.Chains
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using global::BrightChain.Engine.Exceptions;
    using global::BrightChain.Engine.Models.Blocks.DataObjects;
    using global::BrightChain.Engine.Models.Hashes;

    /// <summary>
    /// Brightened data chain, can be composed of file-based CBLs or ChainLinq based data blocks.
    /// TODO: improve memory usage. Don't keep full copy, do all on async enumeration?
    /// </summary>
    public class BrightChain : ConstituentBlockListBlock, IEnumerable<BrightenedBlock>
    {
        private readonly IEnumerable<BrightenedBlock> _blocks;
        private readonly BrightenedBlock _head;
        private readonly BrightenedBlock _tail;
        private readonly int _count;
        private readonly BlockParams _blockParams;

        public BrightChain(ConstituentBlockListBlockParams blockParams, IEnumerable<BrightenedBlock> sourceBlocks)
            : base(blockParams)
        {
            if (!sourceBlocks.Any())
            {
                throw new BrightChainException(nameof(sourceBlocks));
            }

            this._blocks = sourceBlocks;
            this._head = this._blocks.First();
            this._blockParams = this._head.BlockParams;

            var blockStats = this.VerifyHomogeneity();
            this._tail = blockStats.Item1;
            this._count = blockStats.Item2;
        }

        public int Count()
        {
            return this._count;
        }

        public BrightenedBlock First()
        {
            return this._head;
        }

        /// <summary>
        /// Returns a Tuple of (BrightenedBlock, int) with the tail node and count.
        /// Future planning that this verification process will walk the stack and get the counts/tail anyway, regardless of Async/eager loaded.
        /// </summary>
        /// <returns></returns>
        public Tuple<BrightenedBlock, int> VerifyHomogeneity()
        {
            Type blockType = this._blocks.First().GetType();
            foreach (var block in this._blocks)
            {
                if ((block.OriginalType != blockType.AssemblyQualifiedName) || (block.BlockSize != this._blockParams.BlockSize))
                {
                    throw new BrightChainException("Block type mismatch");
                }
            }

            return new Tuple<BrightenedBlock, int>(this._tail, this._blocks.Count());
        }

        public BrightenedBlock Last()
        {
            return this._tail;
        }

        public IEnumerable<BrightenedBlock> All()
        {
            return this._blocks;
        }

        public async IAsyncEnumerator<BrightenedBlock> AllAsync()
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

        public async IAsyncEnumerable<BlockHash> IdsAsync()
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
