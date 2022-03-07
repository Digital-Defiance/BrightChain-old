

namespace BrightChain.Engine.Models.Blocks.Tags
{
    using System;
    using System.Linq;
    using BrightChain.Engine.Enumerations;

    /// <summary>
    /// Tag string.
    /// </summary>
    public struct BrightTag : IFormattable
    {
        /// <summary>
        ///     Tag id.
        /// </summary>
        public readonly Guid Id;

        /// <summary>
        ///     Tag string.
        /// </summary>
        public readonly string Tag;

        /// <summary>
        ///     Tag type.
        /// </summary>
        public readonly BrightTagType Type;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BrightTag" /> struct.
        ///     Create a new tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="type"></param>
        public BrightTag(string tag, BrightTagType type = BrightTagType.UserAssigned)
        {
            this.Id = Guid.NewGuid();
            this.Tag = tag;
            this.Type = type;
        }

        /// <summary>
        ///     Gets tag bytes.
        /// </summary>
        public ReadOnlyMemory<byte> Bytes =>
            new(array: this.Tag.Select(selector: c => (byte)c).ToArray());

        /// <summary>
        ///     Gets create a unique identifier from the type and tag.
        /// </summary>
        public string UniqueIdentifier => $"{this.Type.ToString()}:{this.Tag}";

        /// <summary>
        ///     Gets the unique identifier as bytes.
        /// </summary>
        public ReadOnlyMemory<byte> IdentifierBytes => new(array: this.UniqueIdentifier.Select(selector: c => (byte)c).ToArray());

        /// <summary>
        ///     Gets the CRC32 of the tag bytes.
        /// </summary>
        public uint Crc32 =>
            NeuralFabric.Helpers.Crc32.ComputeChecksum(bytes: this.Bytes.ToArray());

        /// <summary>
        ///     Gets the CRC64 of the tag bytes.
        /// </summary>
        public ulong Crc64 =>
            NeuralFabric.Helpers.Crc64.ComputeChecksum(bytes: this.Bytes.ToArray());

        /// <summary>
        ///     Tag to string is just the tag.
        /// </summary>
        /// <param name="_"></param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        public string ToString(string _, IFormatProvider formatProvider)
        {
            return this.Tag.ToString(provider: formatProvider);
        }

        /// <summary>
        ///     Tag to string is just the tag.
        /// </summary>
        /// <returns>Tag string.</returns>
        public override string ToString()
        {
            return this.Tag;
        }
    }
}