using BrightChain.Attributes;
using BrightChain.Enumerations;
using BrightChain.Exceptions;
using BrightChain.Extensions;
using BrightChain.Helpers;
using BrightChain.Interfaces;
using BrightChain.Models.Contracts;
using CSharpTest.Net.IO;
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
        public StorageDurationContract StorageContract { get; internal set; }
        [BrightChainMetadata]
        public RedundancyContract RedundancyContract { get; internal set; }
        public ReadOnlyMemory<byte> Data { get; protected set; }

        public BlockSize BlockSize { get; }
        public bool HashVerified { get; private set; }

        /// <summary>
        /// A list of the blocks, in order, required to complete this block. Not persisted to disk.
        /// Generally only used during construction of a chain
        /// </summary>
        public IEnumerable<Block> ConstituentBlocks { get; private set; }

        /// <summary>
        /// Returns a block which contains only the constituent block hashes, ready to write to disk.
        /// </summary>
        public ConstituentBlockListBlock ConstituentBlockListBlock { get => new ConstituentBlockListBlock(sourceBlock: this.AsBlock); }

        /// <summary>
        /// Emits the serialization of the block minus data and any ignored attributes (including itself).
        /// </summary>
        public ReadOnlyMemory<byte> MetaData =>
            this.MetaDataBytes();

        public abstract Block NewBlock(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data, bool allowCommit);

        public Block AsBlock { get => this as Block; }

        public IEnumerable<BrightChainValidationException> ValidationExceptions { get; protected set; }

        public Block(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data)
        {
            if (!BlockSizeMap.Map.ContainsValue(data.Length))
                throw new BrightChainException("Invalid Block Size"); // TODO: make (more) special exception type

            this.BlockSize = BlockSizeMap.BlockSize(data.Length);
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
        public Block XOR(Block other)
        {
            if (other is SourceBlock)
                throw new BrightChainException("Unexpected SourceBlock");
            if (this.BlockSize != other.BlockSize)
                throw new BrightChainException("BlockSize mismatch");

            DateTime keepUntil = DateTime.Compare(this.StorageContract.KeepUntilAtLeast, other.StorageContract.KeepUntilAtLeast) > 0 ?
                this.StorageContract.KeepUntilAtLeast :
                other.StorageContract.KeepUntilAtLeast;
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
            DateTime keepUntil = this.StorageContract.KeepUntilAtLeast;
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

                keepUntil = (b.StorageContract.KeepUntilAtLeast > keepUntil) ? b.StorageContract.KeepUntilAtLeast : keepUntil;
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

        /// <summary>
        /// return true or throw an exception with the error
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            List<BrightChainValidationException> validationExceptions = new List<BrightChainValidationException>();

            if (this.BlockSize == BlockSize.Unknown)
                validationExceptions.Add(new BrightChainValidationException(
                    element: nameof(this.BlockSize),
                    message: String.Format("{0} is invalid: {1}", nameof(this.BlockSize), this.BlockSize.ToString())));

            if (this.BlockSize != BlockSizeMap.BlockSize(this.Data.Length))
                validationExceptions.Add(new BrightChainValidationException(
                    element: nameof(this.BlockSize),
                    message: String.Format("{0} is invalid: {1}, actual {2} bytes", nameof(this.BlockSize), this.BlockSize.ToString(), this.Data.Length)));

            try
            {
                var recomputedHash = new BlockHash(this);
                if (this.Id != recomputedHash)
                    validationExceptions.Add(new BrightChainValidationException(
                        element: nameof(this.Id),
                        message: String.Format("{0} is invalid: {1}, actual {2}", nameof(this.Id), this.Id.ToString(), recomputedHash.ToString())));
            }
            catch (Exception e)
            {
                validationExceptions.Add(new BrightChainValidationException(
                    element: nameof(this.Id),
                    message: String.Format("{0} is invalid: {1}, unable to recompute hash: {2}", nameof(this.Id), this.Id.ToString(), e.Message)));
            }

            if (this.Data.Length != BlockSizeMap.BlockSize(this.BlockSize))
                validationExceptions.Add(new BrightChainValidationException(
                    element: nameof(this.Data),
                    message: String.Format("{0} has no data: {1} bytes", nameof(this.Data), this.Data.Length.ToString())));

            this.ValidationExceptions = validationExceptions;

            return (validationExceptions.Count == 0);
        }

        internal bool reloadMetadata(string key, object value, out Exception exception)
        {
            var prop = this.GetType().GetProperty(key);
            try
            {
                foreach (object attr in prop.GetCustomAttributes(true))
                    if (attr is BrightChainMetadataAttribute)
                    {
                        prop.SetValue(this, value);
                        if (value is RedundancyContract redundancyContract)
                            this.StorageContract = redundancyContract.StorageContract;
                        exception = null;
                        return true;
                    }
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }

            // not settable attribute
            exception = new BrightChainException("Invalid Metadata attribute");
            return false;
        }

        public static bool operator ==(Block a, Block b) =>
            ReadOnlyMemoryComparer<byte>.Compare(a.Data, b.Data) == 0;

        public static bool operator !=(Block a, Block b) =>
            !(a == b);

        public override bool Equals(object obj) =>
            this == obj as Block;

        public override int GetHashCode() =>
            this.Data.GetHashCode();

        public abstract void Dispose();

        public int CompareTo(IBlock other) =>
            BinaryComparer.Compare(this.Data.ToArray(), other.Data.ToArray());

        public int CompareTo(Block other) =>
            BinaryComparer.Compare(this.Data.ToArray(), other.Data.ToArray());
    }
}