namespace BrightChain.Engine.Models.Blocks.Chains
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using global::BrightChain.Engine.Exceptions;
    using global::BrightChain.Engine.Models.Blocks.DataObjects;
    using global::BrightChain.Engine.Models.Hashes;
    using global::BrightChain.Engine.Services;
    using ProtoBuf;

    /// <summary>
    /// ChainLinq is the un-brightened/source array. ChainLinq helps build BrightChains.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [ProtoContract]
    public class ChainLinq<T>
        where T : new()
    {
        public ChainLinq(IEnumerable<ChainLinqObjectBlock<T>> blocks)
        {
            if (!blocks.Any())
            {
                throw new BrightChainException(nameof(blocks));
            }

            SetNextLinks(blocks);
            this.ObjectBlocks = blocks;
        }

        public IEnumerable<ChainLinqObjectBlock<T>> ObjectBlocks { get; }

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

        public static ChainLinq<T> ForgeChainLinq(BlockParams blockParams, IEnumerable<T> objects)
        {
            List<ChainLinqObjectBlock<T>> blocks = new List<ChainLinqObjectBlock<T>>();
            foreach (var o in objects)
            {
                blocks.Add(ChainLinqObjectBlock<T>.MakeBlock(
                    blockParams: blockParams,
                    blockObject: o));
            }

            return new ChainLinq<T>(blocks);
        }

        public static async Task<ChainLinq<T>> ForgeChainLinqAsync(BlockParams blockParams, IAsyncEnumerable<T> objects)
        {
            List<ChainLinqObjectBlock<T>> blocks = new List<ChainLinqObjectBlock<T>>();
            await foreach (var o in objects)
            {
                blocks.Add(ChainLinqObjectBlock<T>.MakeBlock(
                    blockParams: blockParams,
                    blockObject: o));
            }

            return new ChainLinq<T>(blocks);
        }

        public long Count()
        {
            return this.ObjectBlocks.LongCount();
        }

        public ChainLinqObjectBlock<T> First()
        {
            return this.ObjectBlocks.First();
        }

        public ChainLinqObjectBlock<T> Last()
        {
            return this.ObjectBlocks.Last();
        }

        public IEnumerable<T> All()
        {
            return this.ObjectBlocks.Select(b => b.BlockObject);
        }

        public async IAsyncEnumerable<T> AllAsync()
        {
            foreach (var block in this.All())
            {
                yield return block;
            }
        }

        public async IAsyncEnumerable<ChainLinqObjectBlock<T>> ObjectBlocksAsync()
        {
            foreach (var objectBlock in this.ObjectBlocks)
            {
                yield return objectBlock;
            }
        }

        /// <summary>
        /// Technically the "Id" field of the blocks, but as these are unbrightened source blocks, the Id will change and not be used.
        /// Do NOT rely on the Id of these blocks for anything other than comparison of whether the contents have changed, prior to being brightened into a BrightChain.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BlockHash> Hashes()
        {
            return this.ObjectBlocks.Select(b => b.Id);
        }

        public async IAsyncEnumerable<BlockHash> HashesAsync()
        {
            foreach (var blockHash in this.Hashes())
            {
                yield return blockHash;
            }
        }

        public async Task<BrightChain> BrightenAllAsync(BrightBlockService brightBlockService)
        {
            return await BrightenAllAsync(brightBlockService, this.ObjectBlocksAsync())
.ConfigureAwait(false);
        }

        public static async Task<BrightChain> BrightenAllAsync(BrightBlockService brightBlockService, IAsyncEnumerable<ChainLinqObjectBlock<T>> objectBlocks)
        {
            using (SHA256 sha = SHA256.Create())
            {
                int received = 0;
                var expected = await objectBlocks.CountAsync().ConfigureAwait(false);
                long bytesProcessed = 0;
                await foreach (var block in objectBlocks)
                {
                    var blockLength = block.Bytes.Length;
                    bytesProcessed += blockLength;
                    if (++received == expected)
                    {
                        sha.TransformFinalBlock(block.Bytes.ToArray(), 0, blockLength);
                    }
                    else
                    {
                        sha.TransformBlock(block.Bytes.ToArray(), 0, blockLength, null, 0);
                    }
                }

                var brightBlocks = brightBlockService
                .BrightenBlocksAsyncEnumerable(
                    identifiableBlocks: objectBlocks);

                return await brightBlockService.ForgeChainAsync(
                    sourceId: new DataHash(
                        providedHashBytes: sha.Hash,
                        sourceDataLength: bytesProcessed,
                        computed: true),
                    brightenedBlocks: brightBlocks)
                    .ConfigureAwait(false);
            }
        }
    }
}
