namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Extensions;
    using BrightChain.Engine.Helpers;
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Blocks.Chains;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Contracts;
    using BrightChain.Engine.Models.Entities;
    using BrightChain.Engine.Models.Hashes;
    using BrightChain.Engine.Models.Nodes;
    using BrightChain.Engine.Services.CacheManagers;
    using Ent;
    using ProtoBuf;
    using static Ent.EntCalc;

    /// <summary>
    /// The block is the base unit persisted to disk.
    /// </summary>
    [ProtoContract]
    [ProtoInclude(1, typeof(BrightenedBlock))]
    [ProtoInclude(2, typeof(RootBlock))]
    [ProtoInclude(3, typeof(PrivateEncryptedSourceBlock))]
    [ProtoInclude(4, typeof(RandomizerBlock))]
    [ProtoInclude(5, typeof(TransactableBlock))]
    [ProtoInclude(6, typeof(ConstituentBlockListBlock))]
    [ProtoInclude(7, typeof(SuperConstituentBlockListBlock))]
    [ProtoInclude(8, typeof(ChainLinq<>))]
    public abstract class Block : IBlock, IComparable<IBlock>, IComparable<Block>, IEquatable<Block>, IEquatable<IBlock>
    {
        [ProtoMember(1)]
        public BlockHash Id { get; }

        [ProtoMember(2)]
        public StorageContract StorageContract { get; set; }

        /// <summary>
        /// Gets the bytes associated with this block.
        /// Notably, the StoredData is NOT part of the proto contract.
        /// </summary>
        public BlockData StoredData { get; internal set; }

        public ReadOnlyMemory<byte> Bytes => this.StoredData.Bytes;

        public byte ByteAt(int index)
        {
            return this.StoredData.Bytes.Slice(index).ToArray()[0];
        }

        public BlockSize BlockSize { get; }

        public bool HashVerified { get; private set; }

        /// <summary>
        /// Gets a BlockSignature containing a signature of the block's hash and all other contents of metadata except signature.
        /// </summary>
        [ProtoMember(4)]
        public BlockSignature Signature { get; internal set; }

        public bool Signed => (this.Signature is not null);

        public bool SignatureVerified { get; internal set; }

        [ProtoMember(5)]
        public BrightChainNode OriginatingNode { get; internal set; }

        [ProtoMember(6)]
        public string OriginalType { get; internal set; }

        protected readonly Type originalType;

        [ProtoMember(7)]
        public string AssemblyVersion { get; internal set; }

        /// <summary>
        /// For private encrypted files, a special token encrypted with the original user's key will allow revocation
        /// </summary>
        [ProtoMember(8)]
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

        public abstract Block NewBlock(BlockParams blockParams, ReadOnlyMemory<byte> data);

        public Block AsBlock => this;

        public IBlock AsIBlock => this;

        public IEnumerable<BrightChainValidationException> ValidationExceptions { get; private set; }

        /// <summary>
        /// Gets an EntCalcResult.
        /// Uses ENT Chi Square monte-carlo calculator/estimator.
        /// </summary>
        public EntCalcResult EntropyEstimate
        {
            get
            {
                MemoryStream memoryStream = new MemoryStream(this.Bytes.ToArray());
                memoryStream.Position = 0;
                EntCalc entCalc = new EntCalc(false);
                while (memoryStream.Position < memoryStream.Length)
                {
                    entCalc.AddSample((byte)memoryStream.ReadByte(), false);
                }

                EntCalc.EntCalcResult calculationResult = entCalc.EndCalculation();
                memoryStream.Close();
                return calculationResult;
            }
        }

        /// <summary>
        /// Gets a uint with the CRC32 of the block's data.
        /// </summary>
        public uint Crc32 =>
            this.StoredData.Crc32;

        public ulong Crc64 =>
            this.StoredData.Crc64;

        /// <summary>
        /// Compares the data hashes only.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Block a, Block b)
        {
            return a.StoredData == b.StoredData;
        }

        public static bool operator !=(Block a, Block b)
        {
            return a.StoredData != b.StoredData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Block"/> class.
        /// Construct a block from the given parameters and data.
        /// </summary>
        /// <param name="blockParams"></param>
        /// <param name="data"></param>
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

            Assembly assembly = Assembly.GetEntryAssembly();
            AssemblyInformationalVersionAttribute versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            this.StorageContract = new StorageContract(
                RequestTime: blockParams.RequestTime,
                KeepUntilAtLeast: blockParams.KeepUntilAtLeast,
                ByteCount: data.Length,
                PrivateEncrypted: blockParams.PrivateEncrypted,
                redundancyContractType: blockParams.Redundancy);
            this.StoredData = new BlockData(data);
            this.Id = new BlockHash(this); // must happen after data is in place
            this.ConstituentBlocks = new BlockHash[] { };
            this.OriginatingNode = null;
            this.Signature = null;
            this.SignatureVerified = false;
            this.RevocationCertificates = new List<RevocationCertificate>();
            this.originalType = blockParams.OriginalType;
            this.OriginalType = this.originalType.AssemblyQualifiedName;
            this.AssemblyVersion = versionAttribute.InformationalVersion;
            this.HashVerified = this.Validate(); // also fills in any validation errors in the array
        }

        /// <summary>
        /// XORs this block with another/randomizer block.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public ReadOnlyMemory<byte> XOR(Block other)
        {
            if (other is SourceBlock)
            {
                throw new BrightChainException("Unexpected SourceBlock");
            }

            if (this.Bytes.Length != other.Bytes.Length)
            {
                throw new BrightChainException("BlockSize mismatch");
            }

            return Utilities.ReadOnlyMemoryXOR(this.Bytes, other.Bytes);
        }

        /// <summary>
        /// XORs this block with a list of other/randomizer blocks.
        /// XOR will ignore the instance block in the block array.
        /// </summary>
        /// <param name="others"></param>
        /// <returns></returns>
        public ReadOnlyMemory<byte> XOR(Block[] others)
        {
            int blockSize = BlockSizeMap.Map[this.BlockSize];
            byte[] xorData = this.Bytes.ToArray();

            foreach (Block b in others)
            {
                if (b.Id == this.Id)
                {
                    continue;
                }

                if (b is SourceBlock)
                {
                    throw new BrightChainException("Unexpected SourceBlock");
                }

                if (b.BlockSize != this.BlockSize)
                {
                    throw new BrightChainException("BlockSize mismatch");
                }

                byte[] xorWith = b.Bytes.ToArray();
                for (int i = 0; i < blockSize; i++)
                {
                    xorData[i] = (byte)(xorData[i] ^ xorWith[i]);
                }
            }

            return new ReadOnlyMemory<byte>(xorData);
        }

        /// <summary>
        /// Returns a boolean indicating whether the assembly qualified type name was resolved. Optional type to compare against.
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="compareTo"></param>
        /// <returns></returns>
        public static bool ValidateType(string typeName, Type compareTo = null)
        {
            try
            {
                var type = Type.GetType(typeName);

                if (type is null)
                {
                    return false;
                }

                return (compareTo is null) || type.Equals(compareTo);
            }
            catch (Exception _)
            {
                return false;
            }
        }

        public bool ValidateType(Type compareTo = null)
        {
            return ValidateType(this.OriginalType, compareTo is null ? this.originalType : compareTo);
        }

        /// <summary>
        /// Sign the block/metadata.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public BlockSignature Sign(Agent user, string password)
        {
            throw new NotImplementedException();
            this.SignatureVerified = true;
        }

        /// <summary>
        /// Verifies the block's signature.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="signature">Signature hash. If signature is not provided, pull from attributes.</param>
        /// <returns></returns>
        public bool VerifySignature(Agent user, string password, ReadOnlyMemory<byte>? signature = null)
        {
            throw new NotImplementedException();
            return false;
        }

        public TransactableBlock MakeTransactable(BlockCacheManager cacheManager, bool allowCommit)
        {
            return new TransactableBlock(
                cacheManager: cacheManager,
                sourceBlock: this,
                allowCommit: allowCommit);
        }

        /// <summary>
        /// Gets a blockParams object from this block's attributes.
        /// </summary>
        public virtual BlockParams BlockParams => new BlockParams(
                blockSize: this.BlockSize,
                requestTime: this.StorageContract.RequestTime,
                keepUntilAtLeast: this.StorageContract.KeepUntilAtLeast,
                redundancy: this.StorageContract.RedundancyContractType,
                privateEncrypted: this.StorageContract.PrivateEncrypted,
                originalType: this.originalType);

        public bool Validate()
        {
            IEnumerable<BrightChainValidationException> validationExceptions;
            var result = this.PerformValidation(out validationExceptions);
            this.ValidationExceptions = validationExceptions;
            return result;
        }

        public abstract void Dispose();

        public override int GetHashCode()
        {
            return (int)this.StoredData.Crc32;
        }

        public int CompareTo(IBlock other)
        {
            return this.StoredData.CompareTo(other.StoredData);
        }

        public int CompareTo(Block other)
        {
            return this.StoredData.CompareTo(other.StoredData);
        }

        public override bool Equals(object obj)
        {
            return obj is Block blockObj ? this.Equals(blockObj) : false;
        }

        public bool Equals(IBlock other)
        {
            return this.CompareTo(other) == 0;
        }

        public bool Equals(Block other)
        {
            return this.CompareTo(other) == 0;
        }
    }
}
