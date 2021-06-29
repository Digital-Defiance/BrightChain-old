using BrightChain.Attributes;
using BrightChain.Enumerations;
using BrightChain.Exceptions;
using BrightChain.Extensions;
using BrightChain.Helpers;
using BrightChain.Interfaces;
using BrightChain.Models.Contracts;
using System;
using System.Collections.Generic;

namespace BrightChain.Models.Blocks
{
    /// <summary>
    /// The block is the base unit persisted to disk
    /// </summary>
    public abstract class Block : IBlock, IComparable<IBlock>, IComparable<Block>
    {
        public BlockHash Id { get; }
        public StorageDurationContract StorageContract { get; set; }
        [BrightChainMetadata]
        public RedundancyContract RedundancyContract { get; set; }
        public ReadOnlyMemory<byte> Data { get; protected set; }

        public BlockSize BlockSize { get; }
        public bool HashVerified { get; private set; }

        /// <summary>
        /// A list of the blocks, in order, required to complete this block. Not persisted to disk.
        /// Generally only used during construction of a chain
        /// </summary>
        public IEnumerable<IBlock> ConstituentBlocks { get; protected set; }

        /// <summary>
        /// Emits the serialization of the block minus data and any ignored attributes (including itself).
        /// </summary>
        public ReadOnlyMemory<byte> Metadata =>
            this.MetadataBytes();

        public abstract Block NewBlock(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data, bool allowCommit);

        public Block AsBlock => this;

        public IEnumerable<BrightChainValidationException> ValidationExceptions { get; internal set; }

        public Block(BlockSize blockSize, DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data)
        {
            if (!BlockSizeMap.Map.ContainsValue(data.Length))
            {
                throw new BrightChainException("Invalid Block Size"); // TODO: make (more) special exception type
            }

            this.BlockSize = BlockSizeMap.BlockSize(data.Length);
            if (this.BlockSize != blockSize)
            {
                throw new BrightChainException("Block size mismatch");
            }

            this.StorageContract = new StorageDurationContract(
                requestTime: requestTime,
                keepUntilAtLeast: keepUntilAtLeast,
                byteCount: data.Length);
            this.RedundancyContract = new RedundancyContract(
                storageDurationContract: this.StorageContract,
                redundancy: redundancy);
            this.Data = data;
            this.Id = new BlockHash(this); // must happen after data is in place
            this.ConstituentBlocks = new Block[] { };
            this.HashVerified = this.Validate(); // also fills in any validation errors in the array
        }

        /// <summary>
        /// XORs this block with another/randomizer block
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public Block XOR(IBlock other)
        {
            if (other is SourceBlock)
            {
                throw new BrightChainException("Unexpected SourceBlock");
            }

            if (this.Data.Length != other.Data.Length)
            {
                throw new BrightChainException("BlockSize mismatch");
            }

            DateTime keepUntil = DateTime.Compare(this.StorageContract.KeepUntilAtLeast, other.StorageContract.KeepUntilAtLeast) > 0 ?
                this.StorageContract.KeepUntilAtLeast :
                other.StorageContract.KeepUntilAtLeast;
            RedundancyContractType redundancy = this.RedundancyContract.RedundancyContractType > other.RedundancyContract.RedundancyContractType ?
                this.RedundancyContract.RedundancyContractType :
                other.RedundancyContract.RedundancyContractType;
            int blockSize = BlockSizeMap.Map[this.BlockSize];
            byte[] xorData = new byte[blockSize];
            for (int i = 0; i < blockSize; i++)
            {
                xorData[i] = this.Data.Slice(start: i, length: 1).ToArray()[0];
            }

            var result = this.NewBlock(
                requestTime: DateTime.Now,
                keepUntilAtLeast: keepUntil,
                redundancy: redundancy,
                data: new ReadOnlyMemory<byte>(xorData),
                allowCommit: true); // these XOR functions should be one of the only places this even happens
            var newList = new List<IBlock>(this.ConstituentBlocks);
            if (!(this is SourceBlock))
            {
                newList.Add(this);
            }

            if (!(other is SourceBlock))
            {
                newList.Add(other);
            }

            result.ConstituentBlocks = newList.ToArray();
            return result;
        }

        /// <summary>
        /// XORs this block with a list of other/randomizer blocks
        /// </summary>
        /// <param name="others"></param>
        /// <returns></returns>
        public Block XOR(IBlock[] others)
        {
            DateTime keepUntil = this.StorageContract.KeepUntilAtLeast;
            RedundancyContractType redundancy = this.RedundancyContract.RedundancyContractType;
            int blockSize = BlockSizeMap.Map[this.BlockSize];
            var newList = new List<IBlock>(this.ConstituentBlocks);
            if (!(this is SourceBlock))
            {
                newList.Add(this);
            }

            byte[] xorData = this.Data.ToArray();

            foreach (Block b in others)
            {
                if (b is SourceBlock)
                {
                    throw new BrightChainException("Unexpected SourceBlock");
                }

                if (b.BlockSize != this.BlockSize)
                {
                    throw new BrightChainException("BlockSize mismatch");
                }

                keepUntil = (b.StorageContract.KeepUntilAtLeast > keepUntil) ? b.StorageContract.KeepUntilAtLeast : keepUntil;
                redundancy = (b.RedundancyContract.RedundancyContractType > redundancy) ? b.RedundancyContract.RedundancyContractType : redundancy;
                byte[] xorWith = b.Data.ToArray();
                for (int i = 0; i < blockSize; i++)
                {
                    xorData[i] = (byte)(xorData[0] ^ xorWith[i]);
                }

                newList.Add(b);
            }

            var result = this.NewBlock(
                requestTime: System.DateTime.Now,
                keepUntilAtLeast: keepUntil,
                redundancy: redundancy,
                data: new ReadOnlyMemory<byte>(xorData),
                allowCommit: true); // these XOR functions should be one of the only places this even happens
            result.ConstituentBlocks = newList.ToArray();
            return result;
        }

        public static bool operator ==(Block a, Block b) => ReadOnlyMemoryComparer<byte>.Compare(a.Data, b.Data) == 0;

        public static bool operator !=(Block a, Block b) => !a.Equals(b);

        public override bool Equals(object obj) => ReadOnlyMemoryComparer<byte>.Compare(this.Data, (obj as Block).Data) == 0;

        public override int GetHashCode() => this.Data.GetHashCode();

        public bool Validate()
        {
            List<BrightChainValidationException> validationExceptions;
            var result = this.PerformValidation(out validationExceptions);
            this.ValidationExceptions = validationExceptions;
            return result;
        }

        public abstract void Dispose();

        public int CompareTo(IBlock other) => ReadOnlyMemoryComparer<byte>.Compare(this.Data, (other as TransactableBlock).Data);

        public int CompareTo(Block other) => ReadOnlyMemoryComparer<byte>.Compare(this.Data, (other as TransactableBlock).Data);
    }
}