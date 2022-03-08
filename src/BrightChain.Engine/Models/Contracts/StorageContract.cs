using System;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Models.Units;
using ProtoBuf;

namespace BrightChain.Engine.Models.Contracts;

/// <summary>
///     Contract for the minimum amount of time required to store a given block.
/// </summary>
[ProtoContract]
public struct StorageContract
{
    /// <summary>
    ///     Gets the Date/Time the block was received by the network.
    /// </summary>
    [ProtoMember(tag: 1)]
    public DateTime RequestTime { get; internal set; }

    /// <summary>
    ///     Gets the Minimum date the block will be preserved until.
    /// </summary>
    [ProtoMember(tag: 2)]
    public DateTime KeepUntilAtLeast { get; internal set; }

    /// <summary>
    ///     Gets the Number of bytes stored in this block.
    /// </summary>
    [ProtoMember(tag: 3)]
    public int ByteCount { get; internal set; }

    /// <summary>
    ///     Gets a value indicating whether the data is being stored for public use.
    ///     Factors into cost and other matters later on.
    /// </summary>
    [ProtoMember(tag: 4)]
    public bool PrivateEncrypted { get; internal set; }

    /// <summary>
    ///     Gets the contracted durability requirements.
    /// </summary>
    [ProtoMember(tag: 5)]
    public RedundancyContractType RedundancyContractType { get; internal set; }

    public StorageContract(DateTime RequestTime, DateTime KeepUntilAtLeast, int ByteCount, bool PrivateEncrypted,
        RedundancyContractType redundancyContractType)
    {
        this.RequestTime = RequestTime;
        this.KeepUntilAtLeast = KeepUntilAtLeast;
        this.ByteCount = ByteCount;
        this.PrivateEncrypted = PrivateEncrypted;
        this.RedundancyContractType = redundancyContractType;
    }

    public static bool operator ==(StorageContract a, StorageContract b)
    {
        return a.RequestTime == b.RequestTime &&
               a.KeepUntilAtLeast == b.KeepUntilAtLeast &&
               a.ByteCount == b.ByteCount &&
               a.PrivateEncrypted == b.PrivateEncrypted &&
               a.RedundancyContractType == b.RedundancyContractType;
    }

    public static bool operator !=(StorageContract a, StorageContract b)
    {
        return !(a == b);
    }

    public double Duration => this.KeepUntilAtLeast.Subtract(value: this.RequestTime).TotalSeconds;

    public ByteStorageDuration ByteStorageDuration => new(
        byteCount: this.ByteCount,
        durationSeconds: (ulong)this.Duration);

    public readonly bool DoNotStore => this.KeepUntilAtLeast.Equals(value: DateTime.MinValue);

    public readonly bool NonExpiring => this.KeepUntilAtLeast.Equals(value: DateTime.MaxValue);

    public bool Equals(StorageContract other)
    {
        return this == other;
    }

    public override bool Equals(object other)
    {
        return other is StorageContract storageContract && storageContract == this;
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}
