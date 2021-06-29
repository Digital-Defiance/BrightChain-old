using BrightChain.Models.Blocks;
using BrightChain.Models.Contracts;
using System;

namespace BrightChain.Interfaces
{
    /// <summary>
    /// Basic description for a block
    /// </summary>
    public interface IBlock : IDisposable, IComparable<IBlock>, IValidatable
    {
        /// <summary>
        /// Block's SHA-256 hash
        /// </summary>
        BlockHash Id { get; }
        /// <summary>
        /// Function to XOR this block's data with another
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        Block XOR(IBlock other);
        /// <summary>
        /// Function to XOR this block's data with an array of others
        /// </summary>
        /// <param name="others"></param>
        /// <returns></returns>
        Block XOR(IBlock[] others);
        /// <summary>
        /// Parameters of the duration contract for this block
        /// </summary>
        StorageDurationContract StorageContract { get; set; }
        /// <summary>
        /// Parameters of the redundancy contract for this block
        /// </summary>
        RedundancyContract RedundancyContract { get; set; }
        /// <summary>
        /// Returns the serialized MetaData pulled from attributes
        /// </summary>
        ReadOnlyMemory<byte> Metadata { get; }
        /// <summary>
        /// Returns only the raw data for the block and none of the metadata. The hash is based only on this.
        /// </summary>
        ReadOnlyMemory<byte> Data { get; }
        /// <summary>
    }
}