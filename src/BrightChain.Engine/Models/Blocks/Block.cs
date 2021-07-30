namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BrightChain.Engine.Attributes;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Extensions;
    using BrightChain.Engine.Helpers;
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Contracts;
    using BrightChain.Engine.Models.Entities;

    /// <summary>
    /// The block is the base unit persisted to disk.
    /// </summary>
    public abstract class Block : IBlock, IComparable<IBlock>, IComparable<Block>, IEquatable<Block>, IEquatable<IBlock>
    {
        public BlockHash Id { get; }

        [BrightChainMetadata]
        public StorageContract StorageContract { get; set; }

        public ReadOnlyMemory<byte> Data { get; protected set; }

        public BlockSize BlockSize { get; }

        public bool HashVerified { get; private set; }

        [BrightChainMetadata]
        public BlockSignature Signature { get; internal set; }

        public bool Signed => (this.Signature != null);

        public bool SignatureVerified { get; internal set; }

        /// <summary>
        /// For private encrypted files, a special token encrypted with the original user's key will allow revocation
        /// </summary>
        [BrightChainMetadata]
        public IEnumerable<RevocationCertificate> RevocationCertificates { get; internal set; }

        /// <summary>
        /// Gets a boolean whether the revocation list contains possible revocation tokens.
        /// </summary>
        public bool Revokable => this.RevocationCertificates.Count() > 0;

        /// <summary>
        /// Gets or sets a list of the blocks, in order, required to complete this block. Not persisted to disk.
        /// Generally only used during construction of a chain
        /// </summary>
        public IEnumerable<BlockHash> ConstituentBlocks { get; protected set; }

        /// <summary>
        /// Gets the serialization of the block minus data and any ignored attributes (including itself).
        /// </summary>
        public ReadOnlyMemory<byte> Metadata =>
            this.MetadataBytes();

        public abstract Block NewBlock(BlockParams blockParams, ReadOnlyMemory<byte> data);

        public Block AsBlock => this;

        public IEnumerable<BrightChainValidationException> ValidationExceptions { get; private set; }

        /// <summary>
        /// Gets a uint with the CRC32 of the block's data.
        /// </summary>
        public uint Crc32 =>
            Helpers.Crc32.ComputeNewChecksum(this.Data.ToArray());

        /// <summary>
        /// Gets a uint with the CRC32 of the block's data.
        /// </summary>
        public uint MetadataCrc32 =>
            Helpers.Crc32.ComputeNewChecksum(this.MetadataBytes().ToArray());

        public static bool operator ==(Block a, Block b)
        {
            return ReadOnlyMemoryComparer<byte>.Compare(a.Data, b.Data) == 0;
        }

        public static bool operator !=(Block a, Block b)
        {
            return !a.Equals(b);
        }

        public Block(BlockParams blockParams, ReadOnlyMemory<byte> data)
        {
            if (this is RootBlock)
            {
                // it is much easier to validate that we're the only rootblock at the TransactableBlock level where we know the cache manager
                // TODO: there can only be one
                this.BlockSize = blockParams.BlockSize;
            }
            else
            {
                var detectedBlockSize = BlockSizeMap.BlockSize(data.Length);

                if (blockParams.BlockSize != BlockSize.Unknown && detectedBlockSize != blockParams.BlockSize)
                {
                    throw new BrightChainException("Block size mismatch");
                }

                this.BlockSize = detectedBlockSize;
            }

            this.StorageContract = new StorageContract(
                RequestTime: blockParams.RequestTime,
                KeepUntilAtLeast: blockParams.KeepUntilAtLeast,
                ByteCount: data.Length,
                PrivateEncrypted: blockParams.PrivateEncrypted,
                redundancyContractType: blockParams.Redundancy);
            this.Data = data;
            this.Id = new BlockHash(this); // must happen after data is in place
            this.ConstituentBlocks = new BlockHash[] { };
            this.HashVerified = this.Validate(); // also fills in any validation errors in the array
            this.Signature = null;
            this.SignatureVerified = false;
            this.RevocationCertificates = new List<RevocationCertificate>();
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
            RedundancyContractType redundancy = this.StorageContract.RedundancyContractType > other.StorageContract.RedundancyContractType ?
                this.StorageContract.RedundancyContractType :
                other.StorageContract.RedundancyContractType;

            var result = this.NewBlock(
                blockParams: new BlockParams(
                    blockSize: this.BlockSize,
                    requestTime: DateTime.Now,
                    keepUntilAtLeast: keepUntil,
                    redundancy: redundancy,
                    privateEncrypted: this.StorageContract.PrivateEncrypted),
                data: Utilities.ParallelReadOnlyMemoryXOR(this.Data, other.Data));

            var newList = new List<BlockHash>(this.ConstituentBlocks);
            if (this is not SourceBlock)
            {
                newList.Add(this.Id);
            }

            if (other is not SourceBlock)
            {
                newList.Add(other.Id);
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
            RedundancyContractType redundancy = this.StorageContract.RedundancyContractType;
            int blockSize = BlockSizeMap.Map[this.BlockSize];
            var newList = new List<BlockHash>(this.ConstituentBlocks);
            if (this is not SourceBlock)
            {
                newList.Add(this.Id);
            }

            byte[] xorData = this.Data.ToArray();

            foreach (Block b in others)
            {
                if (b is SourceBlock)
                {
                    throw new BrightChainException("Unexpected SourceBlock");
                }
                else if (b is not RandomizerBlock)
                {
                    // TODO: this may change
                    throw new BrightChainException("Unexpected block type. Can only work with RandomizerBlocks");
                }

                if (b.BlockSize != this.BlockSize)
                {
                    throw new BrightChainException("BlockSize mismatch");
                }

                keepUntil = (b.StorageContract.KeepUntilAtLeast > keepUntil) ? b.StorageContract.KeepUntilAtLeast : keepUntil;
                redundancy = (b.StorageContract.RedundancyContractType > redundancy) ? b.StorageContract.RedundancyContractType : redundancy;
                byte[] xorWith = b.Data.ToArray();
                for (int i = 0; i < blockSize; i++)
                {
                    xorData[i] = (byte)(xorData[i] ^ xorWith[i]);
                }

                if (b is not SourceBlock)
                {
                    newList.Add(b.Id);
                }
            }

            var newBlockParams = this.BlockParams.Merge(new BlockParams(
                    blockSize: this.BlockSize,
                    requestTime: System.DateTime.Now,
                    keepUntilAtLeast: keepUntil,
                    redundancy: redundancy,
                    privateEncrypted: this.StorageContract.PrivateEncrypted));

            var result = this.NewBlock(
                blockParams: newBlockParams,
                data: new ReadOnlyMemory<byte>(xorData));
            result.ConstituentBlocks = newList.ToArray();
            return result;
        }

        public BlockSignature Sign(Agent user, string password)
        {
            throw new NotImplementedException();
            this.SignatureVerified = true;
        }

        public override bool Equals(object obj)
        {
            return obj is Block ? ReadOnlyMemoryComparer<byte>.Compare(this.Data, (obj as Block).Data) == 0 : false;
        }

        public override int GetHashCode()
        {
            return this.Data.GetHashCode();
        }

        public bool Validate()
        {
            IEnumerable<BrightChainValidationException> validationExceptions;
            var result = this.PerformValidation(out validationExceptions);
            this.ValidationExceptions = validationExceptions;
            return result;
        }

        public abstract void Dispose();

        public int CompareTo(IBlock other)
        {
            return other is null ? -1 : ReadOnlyMemoryComparer<byte>.Compare(this.Data, other.Data);
        }

        public int CompareTo(Block other)
        {
            return other is null ? -1 : ReadOnlyMemoryComparer<byte>.Compare(this.Data, other.Data);
        }

        public virtual BlockParams BlockParams => new BlockParams(
                blockSize: this.BlockSize,
                requestTime: this.StorageContract.RequestTime,
                keepUntilAtLeast: this.StorageContract.KeepUntilAtLeast,
                redundancy: this.StorageContract.RedundancyContractType,
                privateEncrypted: false);

        public bool Equals(IBlock other) =>
            this.CompareTo(other) == 0;

        public bool Equals(Block other) =>
            this.CompareTo(other) == 0;

    }
}
