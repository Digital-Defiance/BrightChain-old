using BrightChain.Attributes;
using BrightChain.Enumerations;
using BrightChain.Exceptions;
using BrightChain.Helpers;
using BrightChain.Interfaces;
using BrightChain.Models.Contracts;
using System;
using System.Collections.Generic;

namespace BrightChain.Models.Blocks
{
    /// <summary>
    /// Block that only contains a hash and has not yet had data loaded in for verification.
    /// TODO: this process is not completely thought out.
    /// </summary>
    public class HashOnlyBlock : IBlock
    {
        public ReadOnlyMemory<byte> Data => throw new NotImplementedException();

        public BlockHash Id { get; }
        [BrightChainMetadata]
        public StorageDurationContract StorageContract => throw new NotImplementedException();
        [BrightChainMetadata]
        public RedundancyContract RedundancyContract => throw new NotImplementedException();

        public bool Committed { get; } = false;
        public bool AllowCommit { get; } = false;

        public BlockSize BlockSize { get; }
        public bool HashVerified { get; }

        public ReadOnlyMemory<byte> Metadata => throw new NotImplementedException();

        StorageDurationContract IBlock.StorageContract { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        RedundancyContract IBlock.RedundancyContract { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IEnumerable<BrightChainValidationException> ValidationExceptions => throw new NotImplementedException();

        private HashOnlyBlock(BlockHash blockHash)
        {
            this.BlockSize = blockHash.BlockSize;
            this.HashVerified = false;
        }

        public void Dispose()
        {

        }

        public Block XOR(Block other) => throw new NotImplementedException();

        public Block XOR(Block[] others) => throw new NotImplementedException();

        public int CompareTo(IBlock other) => ReadOnlyMemoryComparer<byte>.Compare(this.Id.HashBytes, other.Id.HashBytes);
        public Block XOR(IBlock other) => throw new NotImplementedException();
        public Block XOR(IBlock[] others) => throw new NotImplementedException();
        public bool Validate() => false;
    }
}
