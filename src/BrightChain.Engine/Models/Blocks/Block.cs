using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
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
using BrightChain.Engine.Services.CacheManagers.Block;
using Ent;
using ProtoBuf;

namespace BrightChain.Engine.Models.Blocks;

using static EntCalc;

/// <summary>
///     The block is the base unit persisted to disk.
/// </summary>
[DataContract]
[ProtoContract]
[ProtoInclude(tag: 1,
    knownType: typeof(BrightenedBlock))]
[ProtoInclude(tag: 2,
    knownType: typeof(RootBlock))]
[ProtoInclude(tag: 3,
    knownType: typeof(RandomizerBlock))]
[ProtoInclude(tag: 4,
    knownType: typeof(BrightenedBlock))]
[ProtoInclude(tag: 5,
    knownType: typeof(ConstituentBlockListBlock))]
[ProtoInclude(tag: 6,
    knownType: typeof(SuperConstituentBlockListBlock))]
[ProtoInclude(tag: 7,
    knownType: typeof(ChainLinq<>))]
public abstract class Block : IBlock, IComparable<IBlock>, IComparable<Block>, IEquatable<Block>, IEquatable<IBlock>
{
    public readonly Type OriginalType;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Block" /> class.
    ///     Construct a block from the given parameters and data.
    /// </summary>
    /// <param name="blockParams"></param>
    /// <param name="data"></param>
    public Block(BlockParams blockParams, ReadOnlyMemory<byte> data, IEnumerable<BlockHash> constituentBlockHashes = null)
    {
        if (this is RootBlock)
        {
            // it is much easier to validate that we're the only rootblock at the TransactableBlock level where we know the cache manager
            // TODO: there can only be one
            this.BlockSize = blockParams.BlockSize;
        }
        else
        {
            var detectedBlockSize = BlockSizeMap.BlockSize(blockSize: data.Length);

            if (blockParams.BlockSize != BlockSize.Unknown && detectedBlockSize != blockParams.BlockSize)
            {
                throw new BrightChainException(message: "Block size mismatch");
            }

            this.BlockSize = detectedBlockSize;
        }

        var assembly = Assembly.GetEntryAssembly();
        var versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

        this.StorageContract = new StorageContract(
            RequestTime: blockParams.RequestTime,
            KeepUntilAtLeast: blockParams.KeepUntilAtLeast,
            ByteCount: data.Length,
            PrivateEncrypted: blockParams.PrivateEncrypted,
            redundancyContractType: blockParams.Redundancy);
        this.StoredData = new StoredBlockData(data: data);
        this.Id = new BlockHash(block: this); // must happen after data is in place
        this.ConstituentBlocks = constituentBlockHashes is null ? new BlockHash[] { } : constituentBlockHashes;
        this.OriginatingNode = null;
        this.Signature = null;
        this.SignatureVerified = false;
        this.RevocationCertificates = new List<RevocationCertificate>();
        this.OriginalType = blockParams.OriginalType;
        this.OriginalAssemblyTypeString = this.OriginalType.AssemblyQualifiedName;
        this.AssemblyVersion = versionAttribute.InformationalVersion;
        this.HashVerified = this.Validate(); // also fills in any validation errors in the array
    }

    public ReadOnlyMemory<byte> Bytes => this.StoredData.Bytes;

    public string Base58Data => this.StoredData.Base58Data;

    public string Base58Id => this.Id.Base58;

    public bool HashVerified { get; }

    /// <summary>
    ///     For private encrypted files, a special token encrypted with the original user's key will allow revocation
    /// </summary>
    [ProtoMember(tag: 8)]
    public IEnumerable<RevocationCertificate> RevocationCertificates { get; internal set; }

    /// <summary>
    ///     Gets a boolean whether the revocation list contains possible revocation tokens.
    /// </summary>
    public bool Revokable => this.RevocationCertificates.Count() > 0;

    /// <summary>
    ///     Gets or sets a list of the blocks, in order, required to complete this block. Not persisted to disk.
    ///     Generally only used during construction of a chain
    /// </summary>
    public IEnumerable<BlockHash> ConstituentBlocks { get; protected set; }

    public Block AsBlock => this;

    public IBlock AsIBlock => this;

    public ulong Reads { get; }

    public IEnumerable<BlockRating> Ratings { get; }

    public decimal Rating { get; }

    /// <summary>
    ///     If Guid is set with valid LegalOrder, any delete calls should be prevented. Respond as denied or invisible according to
    ///     LegalHoldInvisible.
    /// </summary>
    public Guid? LegalHoldPreventDelete { get; }

    /// <summary>
    ///     If Guid is set with valid LegalOrder, any read accesses should be prevented. Respond as denied or invisible according to
    ///     LegalHoldInvisible.
    /// </summary>
    public Guid? LegalHoldPreventRead { get; }

    /// <summary>
    ///     If Guid is set with valid LegalOrder, any read accesses should be logged to appropriate legal log.
    /// </summary>
    public Guid? LegalHoldLogRead { get; }

    /// <summary>
    ///     If Guid is set with valid LegalOrder, any accesses should respond as if block does not exist.
    /// </summary>
    public Guid? LegalHoldInvisible { get; }

    /// <summary>
    ///     Gets an EntCalcResult.
    ///     Uses ENT Chi Square monte-carlo calculator/estimator.
    /// </summary>
    public EntCalcResult EntropyEstimate
    {
        get
        {
            var entCalc = new EntCalc(binmode: false);
            new List<byte>(collection: this.Bytes.ToArray()).ForEach(action: b => entCalc.AddSample(buf: b,
                Fold: false));
            var calculationResult = entCalc.EndCalculation();
            return calculationResult;
        }
    }

    /// <summary>
    ///     Gets a uint with the CRC32 of the block's data.
    /// </summary>
    public uint Crc32 =>
        this.StoredData.Crc32;

    public ulong Crc64 =>
        this.StoredData.Crc64;

    /// <summary>
    ///     Gets a blockParams object from this block's attributes.
    /// </summary>
    public virtual BlockParams BlockParams => new(
        blockSize: this.BlockSize,
        requestTime: this.StorageContract.RequestTime,
        keepUntilAtLeast: this.StorageContract.KeepUntilAtLeast,
        redundancy: this.StorageContract.RedundancyContractType,
        privateEncrypted: this.StorageContract.PrivateEncrypted,
        originalType: this.OriginalType);

    [ProtoMember(tag: 1)] public BlockHash Id { get; }

    [ProtoMember(tag: 2)] public StorageContract StorageContract { get; set; }

    /// <summary>
    ///     Gets the bytes associated with this block.
    ///     Notably, the StoredData is NOT part of the proto contract.
    /// </summary>
    public BlockData StoredData { get; internal set; }

    public BlockSize BlockSize { get; }

    /// <summary>
    ///     Gets a BlockSignature containing a signature of the block's hash and all other contents of metadata except signature.
    /// </summary>
    [ProtoMember(tag: 4)]
    public BlockSignature Signature { get; internal set; }

    public bool Signed => this.Signature is not null;

    public bool SignatureVerified { get; internal set; }

    [ProtoMember(tag: 5)] public BrightChainNode OriginatingNode { get; internal set; }

    // TODO: Probably going to remove this from the stored attributes and only persist these on ChainLinqDataObjects?
    [ProtoMember(tag: 6)] public string OriginalAssemblyTypeString { get; internal set; }

    [ProtoMember(tag: 7)] public string AssemblyVersion { get; internal set; }

    public IEnumerable<BrightChainValidationException> ValidationExceptions { get; private set; }

    /// <summary>
    ///     XORs this block with another/randomizer block.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public ReadOnlyMemory<byte> XOR(Block other)
    {
        if (other is IdentifiableBlock)
        {
            throw new BrightChainException(message: "Unexpected Identifiable Block");
        }

        if (this.Bytes.Length != other.Bytes.Length)
        {
            throw new BrightChainException(message: "BlockSize mismatch");
        }

        return Utilities.ReadOnlyMemoryXOR(sourceA: this.Bytes,
            sourceB: other.Bytes);
    }

    /// <summary>
    ///     XORs this block with a list of other/randomizer blocks.
    ///     XOR will ignore the instance block in the block array.
    /// </summary>
    /// <param name="others"></param>
    /// <returns></returns>
    public ReadOnlyMemory<byte> XOR(IEnumerable<Block> others)
    {
        var blockSize = BlockSizeMap.Map[key: this.BlockSize];
        var xorData = this.Bytes.ToArray();

        foreach (var b in others)
        {
            if (b.Id == this.Id)
            {
                continue;
            }

            if (b is IdentifiableBlock)
            {
                throw new BrightChainException(message: "Unexpected Identifiable Block");
            }

            if (b.BlockSize != this.BlockSize)
            {
                throw new BrightChainException(message: "BlockSize mismatch");
            }

            var xorWith = b.Bytes.ToArray();
            for (var i = 0; i < blockSize; i++)
            {
                xorData[i] = (byte)(xorData[i] ^ xorWith[i]);
            }
        }

        return new ReadOnlyMemory<byte>(array: xorData);
    }

    /// <summary>
    ///     Sign the block/metadata.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public BlockSignature Sign(Agent user, string password)
    {
        throw new NotImplementedException();
        this.SignatureVerified = true;
    }

    public bool Validate()
    {
        IEnumerable<BrightChainValidationException> validationExceptions;
        var result = this.PerformValidation(validationExceptions: out validationExceptions);
        this.ValidationExceptions = validationExceptions;
        return result;
    }

    public abstract void Dispose();

    public int CompareTo(IBlock other)
    {
        return this.StoredData.CompareTo(other: other.StoredData);
    }

    public int CompareTo(Block other)
    {
        return this.StoredData.CompareTo(other: other.StoredData);
    }

    public bool Equals(Block other)
    {
        return this.CompareTo(other: other) == 0;
    }

    public bool Equals(IBlock other)
    {
        return this.CompareTo(other: other) == 0;
    }

    public byte ByteAt(int index)
    {
        return this.StoredData.Bytes.Slice(start: index).ToArray()[0];
    }

    /// <summary>
    ///     Compares the data hashes only.
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
    ///     Returns a boolean indicating whether the assembly qualified type name was resolved. Optional type to compare against.
    /// </summary>
    /// <param name="typeName"></param>
    /// <param name="restoredType"></param>
    /// <param name="compareTo"></param>
    /// <returns></returns>
    public static bool ValidateType(out Type restoredType, string typeName, Type compareTo)
    {
        try
        {
            restoredType = Type.GetType(typeName: typeName);

            if (restoredType is null)
            {
                return false;
            }

            return restoredType.Equals(o: compareTo);
        }
        catch (Exception _)
        {
            restoredType = null;
            return false;
        }
    }

    public bool ValidateOriginalType()
    {
        return ValidateType(
            restoredType: out _,
            typeName: this.OriginalAssemblyTypeString,
            compareTo: this.OriginalType);
    }

    public bool CompareOriginalType(Type compareTo)
    {
        return this.OriginalType.Equals(o: compareTo);
    }

    public bool CompareOriginalType(Block other)
    {
        return this.CompareOriginalType(compareTo: other.OriginalType);
    }

    public bool ValidateCurrentTypeVsOriginal()
    {
        return this.GetType().Equals(o: this.OriginalType);
    }

    /// <summary>
    ///     Verifies the block's signature.
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

    public BrightenedBlock MakeTransactable(BrightenedBlockCacheManagerBase cacheManager, bool allowCommit)
    {
        var blockParams = new BrightenedBlockParams(
            cacheManager: cacheManager,
            allowCommit: allowCommit,
            blockParams: this.BlockParams);

        return new BrightenedBlock(
            blockParams: blockParams,
            data: this.Bytes,
            constituentBlockHashes: this.ConstituentBlocks);
    }

    public override int GetHashCode()
    {
        return (int)this.StoredData.Crc32;
    }

    public override bool Equals(object obj)
    {
        return obj is Block blockObj ? this.Equals(other: blockObj) : false;
    }
}
