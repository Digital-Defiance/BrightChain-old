using BrightChain.Attributes;
using BrightChain.Enumerations;
using BrightChain.Exceptions;
using BrightChain.Interfaces;
using BrightChain.Models.Contracts;
using System;
using System.Collections.Generic;

namespace BrightChain.Models.Blocks
{
    /// <summary>
    /// The block is the base unit persisted to disk
    /// </summary>
    public abstract class Block : IBlock
    {
        public BlockHash Id { get; }
        [BrightChainMetadata]
        public StorageDurationContract DurationContract { get; }
        [BrightChainMetadata]
        public RedundancyContract RedundancyContract { get; }
        public ReadOnlyMemory<byte> Data { get; protected set; }
        [BrightChainDataIgnore]
        public bool Committed { get; protected set; } = false;
        [BrightChainDataIgnore]
        public bool AllowCommit { get; protected set; } = false;

        [BrightChainDataIgnore]
        public BlockSize BlockSize { get; }
        [BrightChainDataIgnore]
        public bool HashVerified { get; private set; }

        /// <summary>
        /// A list of the blocks, in order, required to complete this block. Not persisted to disk.
        /// </summary>
        [BrightChainDataIgnore]
        public IEnumerable<Block> ConstituentBlocks { get; private set; }

        /// <summary>
        /// Returns a block which contains only the constituent block hashes, ready to write to disk.
        /// </summary>
        [BrightChainDataIgnore]
        public ConstituentBlockListBlock ConstituentBlockListBlock { get => new ConstituentBlockListBlock(sourceBlock: this.AsBlock); }

        /// <summary>
        /// Emits the serialization of the block minus data and any ignored attributes (including itself).
        /// </summary>
        [BrightChainDataIgnore]
        public ReadOnlyMemory<byte> MetaData => throw new NotImplementedException();

        public abstract Block NewBlock(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data, bool allowCommit);

        public Block AsBlock { get => this as Block; }

        public Block(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data)
        {
            if (!BlockSizeMap.Map.ContainsValue(data.Length))
                throw new BrightChainException("Invalid Block Size"); // TODO: make (more) special exception type

            this.BlockSize = BlockSizeMap.BlockSize(data.Length);
            this.Id = new BlockHash(this);
            this.HashVerified = true;
            this.DurationContract = new StorageDurationContract(
                requestTime: requestTime,
                keepUntilAtLeast: keepUntilAtLeast,
                byteCount: (ulong)data.Length);
            this.RedundancyContract = new RedundancyContract(
                storageDurationContract: this.DurationContract,
                redundancy: redundancy);
            this.Data = data;
            this.ConstituentBlocks = new Block[] { };
        }

        /// <summary>
        /// XORs this block with another/randomizer block
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public Block XOR(Block other)
        {
            if (other is SourceBlock)
                throw new BrightChainException("Unexpected SourceBlock");
            if (this.BlockSize != other.BlockSize)
                throw new BrightChainException("BlockSize mismatch");

            DateTime keepUntil = DateTime.Compare(this.DurationContract.KeepUntilAtLeast, other.DurationContract.KeepUntilAtLeast) > 0 ?
                this.DurationContract.KeepUntilAtLeast :
                other.DurationContract.KeepUntilAtLeast;
            RedundancyContractType redundancy = this.RedundancyContract.RedundancyContractType > other.RedundancyContract.RedundancyContractType ?
                this.RedundancyContract.RedundancyContractType :
                other.RedundancyContract.RedundancyContractType;
            int blockSize = BlockSizeMap.Map[this.BlockSize];
            byte[] xorData = new byte[blockSize];
            for (int i = 0; i < blockSize; i++)
                xorData[i] = this.Data.Slice(start: i, length: 1).ToArray()[0];
            var result = NewBlock(
                requestTime: DateTime.Now,
                keepUntilAtLeast: keepUntil,
                redundancy: redundancy,
                data: new ReadOnlyMemory<byte>(xorData),
                allowCommit: true); // these XOR functions should be one of the only places this even happens
            var newList = new List<Block>(this.ConstituentBlocks);
            if (!(this is SourceBlock)) newList.Add(this);
            if (!(other is SourceBlock)) newList.Add(other);
            result.ConstituentBlocks = newList.ToArray();
            return result;
        }

        /// <summary>
        /// XORs this block with a list of other/randomizer blocks
        /// </summary>
        /// <param name="others"></param>
        /// <returns></returns>
        public Block XOR(Block[] others)
        {
            DateTime keepUntil = this.DurationContract.KeepUntilAtLeast;
            RedundancyContractType redundancy = this.RedundancyContract.RedundancyContractType;
            int blockSize = BlockSizeMap.Map[this.BlockSize];
            var newList = new List<Block>(this.ConstituentBlocks);
            if (!(this is SourceBlock)) newList.Add(this);
            byte[] xorData = this.Data.ToArray();

            foreach (Block b in others)
            {
                if (b is SourceBlock)
                    throw new BrightChainException("Unexpected SourceBlock");
                if (b.BlockSize != this.BlockSize)
                    throw new BrightChainException("BlockSize mismatch");

                keepUntil = (b.DurationContract.KeepUntilAtLeast > keepUntil) ? b.DurationContract.KeepUntilAtLeast : keepUntil;
                redundancy = (b.RedundancyContract.RedundancyContractType > redundancy) ? b.RedundancyContract.RedundancyContractType : redundancy;
                byte[] xorWith = b.Data.ToArray();
                for (int i = 0; i < blockSize; i++)
                    xorData[i] = (byte)(xorData[0] ^ xorWith[i]);
                newList.Add(b);
            }

            var result = NewBlock(
                requestTime: System.DateTime.Now,
                keepUntilAtLeast: keepUntil,
                redundancy: redundancy,
                data: new ReadOnlyMemory<byte>(xorData),
                allowCommit: true); // these XOR functions should be one of the only places this even happens
            result.ConstituentBlocks = newList.ToArray();
            return result;
        }

        public abstract void Dispose();
    }
}