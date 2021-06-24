using BrightChain.Attributes;
using BrightChain.Enumerations;
using BrightChain.Interfaces;
using BrightChain.Models.Contracts;
using System;

namespace BrightChain.Models.Blocks
{
    /// <summary>
    /// Block that only contains a hash and has not yet had data loaded in for verification.
    /// TODO: this process is not completely thought out.
    /// </summary>
    public class HashOnlyBlock : IBlock
    {
        public ReadOnlyMemory<byte> Data { get => throw new NotImplementedException(); }

        public BlockHash Id { get; }
        [BrightChainMetadata]
        public StorageDurationContract DurationContract { get => throw new NotImplementedException(); }
        [BrightChainMetadata]
        public RedundancyContract RedundancyContract { get => throw new NotImplementedException(); }

        public bool Committed { get; } = false;
        public bool AllowCommit { get; } = false;

        public BlockSize BlockSize { get; }
        public bool HashVerified { get; }

        public ReadOnlyMemory<byte> MetaData => throw new NotImplementedException();

        private HashOnlyBlock(BlockHash blockHash)
        {
            this.BlockSize = blockHash.BlockSize;
            this.HashVerified = false;
        }

        public void Dispose()
        {

        }

        public Block XOR(Block other) =>
            throw new NotImplementedException();

        public Block XOR(Block[] others) =>
            throw new NotImplementedException();
    }
}
