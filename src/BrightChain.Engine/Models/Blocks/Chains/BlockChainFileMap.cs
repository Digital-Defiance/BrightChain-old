namespace BrightChain.Engine.Models.Blocks.Chains
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::BrightChain.Engine.Exceptions;
    using global::BrightChain.Engine.Services.CacheManagers;

    /// <summary>
    /// Represents a virtual map of all the contituent tuple-sets/blocks in a given source/reconstructed file. These cannot themselves be committed to disk
    /// The block datas may not actually be loaded in memory, but the appropriate blocks will be loaded (all non-local will be pulled into the cache first) relative to their access offsets.
    /// source file -> blockchainfilemap -> commit
    /// pull blocks from pool -> blockchainfilemap -> read.
    /// </summary>
    public class BlockChainFileMap
    {
        private IAsyncEnumerable<TupleStripe> TupleStripes { get; set; }

        public ConstituentBlockListBlock ConstituentBlockListBlock { get; }

        public static async IAsyncEnumerable<IEnumerable<T>> TakeIntoGroupsOf<T>(IEnumerable<T> list, int parts)
        {
            var i = 0;
            T[] items = new T[parts];
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

            Array.Resize<T>(ref items, i);
            yield return items;
        }

        public BlockChainFileMap(ConstituentBlockListBlock cblBlock, IAsyncEnumerable<TupleStripe> tupleStripes = null)
        {
            this.ConstituentBlockListBlock = cblBlock;
            this.TupleStripes = tupleStripes;
        }

        private BlockChainFileMap()
        {
        }

        public async IAsyncEnumerable<TupleStripe> ReconstructTupleStripes(BlockCacheManager blockCacheManager)
        {
            var constituentBlocks = this.ConstituentBlockListBlock.ConstituentBlocks;
            var constituentBlockCount = constituentBlocks.Count();
            if (constituentBlockCount == 0)
            {
                throw new BrightChainException("No hashes in constituent block list");
            }

            if ((constituentBlockCount % this.ConstituentBlockListBlock.TupleCount) != 0)
            {
                throw new BrightChainException("CBL length is not a multiple of the tuple count");
            }

            var tupleGroups = TakeIntoGroupsOf(constituentBlocks, this.ConstituentBlockListBlock.TupleCount);
            await foreach (var tupleGroup in tupleGroups)
            {
                TransactableBlock[] blockList = new TransactableBlock[this.ConstituentBlockListBlock.TupleCount];
                var i = 0;
                foreach (var blockHash in tupleGroup)
                {
                    blockList[i++] = blockCacheManager.Get(blockHash);
                }

                if (i == 0)
                {
                    yield break;
                }

                yield return new TupleStripe(
                    tupleCountMatch: this.ConstituentBlockListBlock.TupleCount,
                    blockSizeMatch: this.ConstituentBlockListBlock.BlockSize,
                    blocks: blockList);
            }
        }

        public async IAsyncEnumerable<Block> ConsolidateTuplesToChainAsync(BlockCacheManager blockCacheManager)
        {
            await foreach (TupleStripe tupleStripe in (this.TupleStripes is null) ? this.ReconstructTupleStripes(blockCacheManager) : this.TupleStripes)
            {
                yield return tupleStripe.Consolidate();
            }

            yield break;
        }

        public static async IAsyncEnumerator<byte> ReadValidatedChainToBytes(IAsyncEnumerable<Block> source)
        {
            await foreach (var block in source)
            {
                if (!block.Validate())
                {
                    throw new BrightChainValidationEnumerableException(block.ValidationExceptions, block.Id.ToString());
                }

                foreach (byte b in block.Bytes.ToArray())
                {
                    yield return b;
                }
            }
        }
    }
}
