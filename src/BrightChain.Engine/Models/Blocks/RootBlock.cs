using System;
using BrightChain.Engine.Attributes;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Interfaces;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Services;

namespace BrightChain.Engine.Models.Blocks
{
    public class RootBlock : TransactableBlock, IBlock, IComparable<IBlock>, IEquatable<IBlock>
    {
        public RootBlock(Guid databaseGuid, BlockCacheManager blockCacheManager)
            : base(
                blockParams: new TransactableBlockParams(
                    cacheManager: blockCacheManager,
                    allowCommit: true,
                    blockParams: new BlockParams(
                        blockSize: BlockSize.Large,
                        requestTime: DateTime.Now,
                        keepUntilAtLeast: DateTime.MaxValue,
                        redundancy: RedundancyContractType.HeapHighPriority,
                        privateEncrypted: false)),
                data: Helpers.RandomDataHelper.DataFiller(default(ReadOnlyMemory<byte>), BlockSize.Large))
        {
            this.Guid = databaseGuid;
        }

        [BrightChainMetadata] public Guid Guid { get; set; }

        public bool Equals(IBlock other)
        {
            throw new NotImplementedException();
        }
    }
}
