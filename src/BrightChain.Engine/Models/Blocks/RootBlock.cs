using System;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Helpers;
using BrightChain.Engine.Interfaces;
using BrightChain.Engine.Models.Blocks.DataObjects;
using ProtoBuf;

namespace BrightChain.Engine.Models.Blocks;

/// <summary>
///     The root block is the key / control node for the cache. Everything gets signed from here.
///     There can only be one.
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
            data: RandomDataHelper.DataFiller(inputData: default,
                blockSize: blockSize))
    {
        this.Guid = databaseGuid;
        this.OriginalAssemblyTypeString = typeof(RootBlock).AssemblyQualifiedName;
    }

    [ProtoMember(tag: 30)] public Guid Guid { get; set; }
}
