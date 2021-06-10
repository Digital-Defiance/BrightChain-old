using BrightChain.Attributes;
using BrightChain.Enumerations;
using BrightChain.Exceptions;
using BrightChain.Interfaces;
using BrightChain.Models.Contracts;
using System;
using System.Collections.Generic;

namespace BrightChain.Models.Blocks
{
    public abstract class Block : IBlock
    {
        public BlockHash Id { get; }
        public StorageDurationContract DurationContract { get; }
        public RedundancyContract RedundancyContract { get; }
        public ReadOnlyMemory<byte> Data { get; protected set; }

        [BrightChainIgnore]
        public BlockSize BlockSize { get; }
        [BrightChainIgnore]
        public bool HashVerified { get; private set; }

        /// <summary>
        /// A list of the blocks, in order, required to complete this block. Not persisted to disk.
        /// </summary>
        [BrightChainIgnore]
        public IEnumerable<IBlock> ConstituentBlocks { get; private set; }

        /// <summary>
        /// Returns a block which contains only the constituent block hashes, ready to write to disk.
        /// </summary>
        [BrightChainIgnore]
        public ConstituentBlockListBlock ConstituentBlockListBlock { get => new ConstituentBlockListBlock(sourceBlock: this); }

        /// <summary>
        /// Emits the json serialization of the block minus data and any ignored attributes (including itself).
        /// </summary>
        [BrightChainIgnore]
        public ReadOnlyMemory<byte> MetaData => throw new NotImplementedException();

        public abstract Block NewBlock(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data);

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
            this.ConstituentBlocks = new IBlock[] { };
        }

        public Block XOR(Block other)
        {
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
                data: new ReadOnlyMemory<byte>(xorData));
            var newList = new List<IBlock>(this.ConstituentBlocks);
            if (!(this is SourceBlock)) newList.Add(this);
            if (!(other is SourceBlock)) newList.Add(other);
            result.ConstituentBlocks = newList.ToArray();
            return result;
        }

        public Block XOR(Block[] others)
        {
            DateTime keepUntil = this.DurationContract.KeepUntilAtLeast;
            RedundancyContractType redundancy = this.RedundancyContract.RedundancyContractType;
            int blockSize = BlockSizeMap.Map[this.BlockSize];
            var newList = new List<IBlock>(this.ConstituentBlocks);
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
                data: new ReadOnlyMemory<byte>(xorData));
            result.ConstituentBlocks = newList.ToArray();
            return result;
        }

        public abstract void Dispose();
    }
}