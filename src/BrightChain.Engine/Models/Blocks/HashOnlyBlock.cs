using System;
using System.Collections.Generic;
using BrightChain.Engine.Attributes;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Helpers;
using BrightChain.Engine.Interfaces;
using BrightChain.Engine.Models.Contracts;
using BrightChain.Engine.Models.Entities;

namespace BrightChain.Engine.Models.Blocks
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
        public StorageContract StorageContract { get; set; }

        [BrightChainMetadata]
        public bool Committed { get; } = false;

        public bool AllowCommit { get; } = false;

        public BlockSize BlockSize { get; }

        public bool HashVerified { get; }

        public ReadOnlyMemory<byte> Metadata => throw new NotImplementedException();

        public IEnumerable<BrightChainValidationException> ValidationExceptions => throw new NotImplementedException();

        public BlockSignature Signature => null;

        public bool Signed => false;

        public bool SignatureVerified => false;

        private HashOnlyBlock(BlockHash blockHash)
        {
            BlockSize = blockHash.BlockSize;
            HashVerified = false;
        }

        public void Dispose()
        {

        }

        public Block XOR(Block other)
        {
            throw new NotImplementedException();
        }

        public Block XOR(Block[] others)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(IBlock other)
        {
            return ReadOnlyMemoryComparer<byte>.Compare(Id.HashBytes, other.Id.HashBytes);
        }

        public Block XOR(IBlock other)
        {
            throw new NotImplementedException();
        }

        public Block XOR(IBlock[] others)
        {
            throw new NotImplementedException();
        }

        public bool Validate()
        {
            return false;
        }

        public BlockSignature Sign(Agent _, string __)
        {
            throw new NotImplementedException();
        }
    }
}
