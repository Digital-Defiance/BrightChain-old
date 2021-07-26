namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using BrightChain.Engine.Attributes;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Services;

    public class RootBlock : TransactableBlock, IBlock, IComparable<IBlock>, IEquatable<IBlock>
    {

        [BrightChainMetadata]
        public Guid Guid { get; set; }

        public RootBlock(Guid databaseGuid, BlockCacheManager blockCacheManager)
            : base(
                   blockParams: new TransactableBlockParams(
                       cacheManager: blockCacheManager,
                       allowCommit: true,
                       blockParams: new BlockParams(
                           blockSize: BlockSize.Unknown,
                           requestTime: DateTime.Now,
                           keepUntilAtLeast: DateTime.MaxValue,
                           redundancy: Enumerations.RedundancyContractType.HeapHighPriority,
                           privateEncrypted: false)),
                   data: new ReadOnlyMemory<byte>() { })
        {
            this.Guid = databaseGuid;
        }

        public bool Equals(IBlock other)
        {
            throw new NotImplementedException();
        }
    }
}
