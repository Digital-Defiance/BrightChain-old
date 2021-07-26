namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Helpers;
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Blocks.Chains;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Contracts;
    using BrightChain.Engine.Models.Entities;
    using BrightChain.Engine.Models.Hashes;
    using BrightChain.Engine.Services;

    public class RootBlock : Block, IBlock, IComparable<IBlock>, IEquatable<IBlock>
    {
        public RootHash Id { get; }

        public RootBlock(Guid databaseGuid, BlockCacheManager blockCacheManager, long totalLength)
            : base(
                   blockParams: new BlockParams(
                       blockSize: BlockSize.Unknown,
                       requestTime: DateTime.Now,
                       keepUntilAtLeast: DateTime.MaxValue,
                       redundancy: Enumerations.RedundancyContractType.HeapHighPriority,
                       privateEncrypted: false),
                   data: new ReadOnlyMemory<byte>() { })
        {
            // Create a Forged Zero Hash/ID!
            var emptyHashBytes = new byte[BlockHash.HashSizeBytes];
            Array.Fill<byte>(emptyHashBytes, 0);
            var emptyHash = new RootHash(
                blockSize: this.BlockSize);
            this.Id = emptyHash;
        }

        public int CompareTo(IBlock other)
        {
            throw new NotImplementedException();
        }

        public Block XOR(IBlock other)
        {
            throw new NotImplementedException();
        }

        public Block XOR(IBlock[] others)
        {
            throw new NotImplementedException();
        }

        public BlockSignature Sign(Agent user, string password)
        {
            throw new NotImplementedException();
        }

        public bool Validate()
        {
            throw new NotImplementedException();
        }

        public bool Equals(IBlock other)
        {
            throw new NotImplementedException();
        }

        public override Block NewBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
