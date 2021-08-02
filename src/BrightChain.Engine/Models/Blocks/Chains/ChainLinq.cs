namespace BrightChain.Engine.Models.Blocks.Chains
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using global::BrightChain.Engine.Exceptions;
    using global::BrightChain.Engine.Models.Blocks.DataObjects;
    using global::BrightChain.Engine.Services;

    /// <summary>
    /// ChainLinq is the un-brightened/source array. ChainLinq helps build BrightChains.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChainLinq<T>
        where T : ISerializable
    {
        public ChainLinq(IEnumerable<ChainLinqObjectBlock<T>> blocks)
        {
            if (!blocks.Any())
            {
                throw new BrightChainException(nameof(blocks));
            }

            SetNextLinks(blocks);
            this.Blocks = (ChainLinqObjectBlock<T>[])blocks;
        }

        public ChainLinqObjectBlock<T>[] Blocks { get; }

        public static IEnumerable<ChainLinqObjectBlock<T>> SetNextLinks(IEnumerable<ChainLinqObjectBlock<T>> blocks)
        {
            for (int i = blocks.Count() - 1; i >= 1; i--)
            {
                var block = blocks.ElementAt(i);
                var previousBlock = blocks.ElementAt(i - 1);
                previousBlock.Next = block.Id;
            }

            return blocks;
        }

        public static BrightChain MakeChain(BrightBlockService brightBlockService, ChainLinqBlockParams blockParams, IEnumerable<T> blockObjects)
        {
            int i = 0;
            ChainLinqObjectBlock<T>[] blocks = new ChainLinqObjectBlock<T>[blockObjects.Count()];
            foreach (var blockObject in blockObjects)
            {
                blocks[i++] = ChainLinqObjectBlock<T>.MakeBlock(
                    blockParams: blockParams,
                    blockObject: blockObject);
            }

            return BrightenAll(brightBlockService, blocks);
        }

        public long Count()
        {
            return this.Blocks.LongLength;
        }

        public ChainLinqObjectBlock<T> First()
        {
            return this.Blocks[0];
        }

        public ChainLinqObjectBlock<T> Last()
        {
            return this.Blocks[this.Blocks.Length - 1];
        }

        public IEnumerable<T> All()
        {
            return this.Blocks.Select(b => b.BlockObject);
        }

        public async IAsyncEnumerable<T> AllAsync()
        {
            foreach (var block in this.All())
            {
                yield return block;
            }
        }

        /// <summary>
        /// Technically the "Id" field of the blocks, but as these are unbrightened source blocks, the Id will change and not be used.
        /// Do NOT rely on the Id of these blocks for anything other than comparison of whether the contents have changed, prior to being brightened into a BrightChain.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BlockHash> Hashes()
        {
            return this.Blocks.Select(b => b.Id);
        }

        public async IAsyncEnumerable<BlockHash> HashesAsync()
        {
            foreach (var blockHash in this.Hashes())
            {
                yield return blockHash;
            }
        }

        public BrightChain BrightenAll(BrightBlockService brightBlockService)
        {
            return brightBlockService.BrightenBlocks(sourceBlocks: SetNextLinks(this.Blocks));
        }

        public static BrightChain BrightenAll(BrightBlockService brightBlockService, IEnumerable<ChainLinqObjectBlock<T>> sourceBlocks)
        {
            return brightBlockService.BrightenBlocks(sourceBlocks: SetNextLinks(sourceBlocks));
        }
    }
}
