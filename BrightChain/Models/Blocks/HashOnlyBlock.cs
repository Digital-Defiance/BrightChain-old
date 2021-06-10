using BrightChain.Attributes;
using BrightChain.Enumerations;
using BrightChain.Interfaces;
using BrightChain.Models.Contracts;
using System;

namespace BrightChain.Models.Blocks
{
    public class HashOnlyBlock : IBlock
    {
        public ReadOnlyMemory<byte> Data { get => throw new NotImplementedException(); }

        public BlockHash Id { get; }
        public StorageDurationContract DurationContract { get => throw new NotImplementedException(); }
        public RedundancyContract RedundancyContract { get => throw new NotImplementedException(); }

        [BrightChainIgnore]
        public BlockSize BlockSize { get; }
        [BrightChainIgnore]
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
