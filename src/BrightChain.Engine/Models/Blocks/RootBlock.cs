namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using BrightChain.Engine.Attributes;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Blocks.DataObjects;

    /// <summary>
    /// The root block is the key / control node for the cache. Everything gets signed from here.
    /// There can only be one.
    /// </summary>
    public class RootBlock : TransactableBlock, IBlock, IComparable<IBlock>
    {
        public RootBlock(Guid databaseGuid, BlockSize blockSize = BlockSize.Large)
            : base(
                blockParams: new TransactableBlockParams(
                    cacheManager: null,
                    allowCommit: false,
                    blockParams: new BlockParams(
                        blockSize: blockSize,
                        requestTime: DateTime.Now,
                        keepUntilAtLeast: DateTime.MaxValue,
                        redundancy: RedundancyContractType.HeapHighPriority,
                        privateEncrypted: false,
                        originalType: typeof(RootBlock))),
                data: Helpers.RandomDataHelper.DataFiller(default(ReadOnlyMemory<byte>), blockSize))
        {
            this.Guid = databaseGuid;
            this.OriginalType = typeof(RootBlock).AssemblyQualifiedName;
        }

        [BrightChainMetadata]
        public Guid Guid { get; set; }
    }
}
