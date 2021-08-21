namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using ProtoBuf;

    /// <summary>
    /// The root block is the key / control node for the cache. Everything gets signed from here.
    /// There can only be one.
    /// </summary>
    [ProtoContract]
    public class RootBlock : BrightenedBlock, IBlock, IComparable<IBlock>
    {
        public RootBlock(Guid databaseGuid, BlockSize blockSize = BlockSize.Large)
            : base(
                blockParams: new BrightenedBlockParams(
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
            this.OriginalAssemblyTypeString = typeof(RootBlock).AssemblyQualifiedName;
        }

        [ProtoMember(30)]
        public Guid Guid { get; set; }
    }
}
