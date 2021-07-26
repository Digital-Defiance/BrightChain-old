namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Helpers;
    using BrightChain.Engine.Models.Blocks.Chains;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Hashes;
    using BrightChain.Engine.Services;

    public class RootBlock : ConstituentBlockListBlock
    {
        public new RootHash Id { get; }

        public RootBlock(Guid databaseGuid, BlockCacheManager blockCacheManager, long totalLength)
            : base(
                blockParams: new ConstituentBlockListBlockParams(
                    blockParams: new TransactableBlockParams(
                        cacheManager: blockCacheManager,
                        allowCommit: true,
                        blockParams: new BlockParams(
                            blockSize: BlockSize.Unknown,
                            requestTime: DateTime.Now,
                            keepUntilAtLeast: DateTime.MaxValue,
                            redundancy: Enumerations.RedundancyContractType.HeapHighPriority,
                            privateEncrypted: false)),
                    sourceId: new GuidId(
                        guid: databaseGuid,
                        sourceDataLength: totalLength),
                    segmentId: null,
                    totalLength: totalLength,
                    constituentBlocks: new List<BlockHash>() { },
                    previous: null,
                    next: null))
        {
            // Create a Forged Zero Hash/ID!
            var emptyHashBytes = new byte[BlockHash.HashSizeBytes];
            Array.Fill<byte>(emptyHashBytes, 0);
            var emptyHash = new RootHash(
                blockSize: this.BlockSize);
            this.Id = emptyHash;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override RootBlock NewBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
        {
            throw new NotImplementedException();
        }
    }
}
