namespace BrightChain.Engine.Models.Blocks.Chains
{
using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using global::BrightChain.Engine.Exceptions;
    using global::BrightChain.Engine.Models.Blocks.DataObjects;
    using global::BrightChain.Engine.Services;

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

        public long Count() => this.Blocks.LongLength;

        public ChainLinqObjectBlock<T> First() =>
            this.Blocks[0];

        public ChainLinqObjectBlock<T> Last() =>
            this.Blocks[this.Blocks.Length - 1];

        public IEnumerable<T> All() =>
            this.Blocks.Select(b => b.BlockObject);

        public BrightChain BrightenAll(BrightBlockService brightBlockService) =>
            brightBlockService.BrightenBlocks(sourceBlocks: SetNextLinks(this.Blocks));

        public static BrightChain BrightenAll(BrightBlockService brightBlockService, IEnumerable<ChainLinqObjectBlock<T>> sourceBlocks)
         => brightBlockService.BrightenBlocks(sourceBlocks: SetNextLinks(sourceBlocks));
    }
}
