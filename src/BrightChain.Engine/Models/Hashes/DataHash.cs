
namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using System.Security.Cryptography;
    using BrightChain.Engine.Helpers;
    using BrightChain.Engine.Interfaces;

    /// <summary>
    /// Type box for the sha hashes.
    /// </summary>
    public class DataHash : IDataHash, IComparable<DataHash>, IEquatable<DataHash>
    {
        /// <summary>
        /// Size in bits of the hash.
        /// </summary>
        public const int HashSize = 256;

        /// <summary>
        /// Size in bytes of the hash.
        /// </summary>
        public const int HashSizeBytes = HashSize / 8;


        /// <summary>
        /// Initializes a new instance of the <see cref="DataHash"/> class.
        /// </summary>
        /// <param name="dataBytes">Data to compute hash from.</param>
        public DataHash(ReadOnlyMemory<byte> dataBytes)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                this.HashBytes = mySHA256.ComputeHash(dataBytes.ToArray());
            }

            this.Computed = true;
            this.SourceDataLength = dataBytes.Length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataHash"/> class.
        /// </summary>
        /// <param name="providedHashBytes">Hash bytes to accept as the hash.</param>
        /// <param name="computed">A boolean value indicating whether the source bytes were computed internally or externally (false).</param>
        /// <param name="sourceDataLength">A long indicating the length of the source data.</param>
        public DataHash(ReadOnlyMemory<byte> providedHashBytes, long sourceDataLength, bool computed)
        {
            this.HashBytes = providedHashBytes;
            this.Computed = computed;
            this.SourceDataLength = sourceDataLength;
        }

        /// <summary>
        /// Gets a ReadOnlyMemory<byte> containing the raw hash result bytes.
        /// </summary>
        public ReadOnlyMemory<byte> HashBytes { get; }

        /// <summary>
        /// Gets a long containing the length of the source data the hash was computed on.
        /// </summary>
        public long SourceDataLength { get; }

        /// <summary>
        /// Gets a value indicating whether trusted code calculated this hash.
        /// </summary>
        public bool Computed { get; }

        public static bool operator ==(DataHash a, DataHash b) =>
            a.SourceDataLength == b.SourceDataLength && ReadOnlyMemoryComparer<byte>.Compare(a.HashBytes, b.HashBytes) == 0;

        public static bool operator ==(ReadOnlyMemory<byte> b, DataHash a) =>
            a.SourceDataLength == b.Length && ReadOnlyMemoryComparer<byte>.Compare(a.HashBytes, b) == 0;

        public static bool operator !=(ReadOnlyMemory<byte> b, DataHash a) =>
            !(b == a);

        public static bool operator !=(DataHash a, DataHash b)
            => !(b == a);

        /// <summary>
        /// Returns a formatted hash string as a series of lowercase hexadecimal characters.
        /// </summary>
        /// <param name="_">Ignored.</param>
        /// <param name="__">Ignored also.</param>
        /// <returns>Returns a formatted hash string.</returns>
        public string ToString(string _, IFormatProvider __) =>
            Helpers.Utilities.HashToFormattedString(this.HashBytes.ToArray());

        /// <summary>
        /// Returns a formatted hash string as a series of lowercase hexadecimal characters.
        /// </summary>
        /// <returns>Returns a formatted hash string.</returns>
        public new string ToString() =>
            Helpers.Utilities.HashToFormattedString(this.HashBytes.ToArray());

        /// <summary>
        /// Compares the raw bytes of the hash with a DataHash classed as a plain object.
        /// </summary>
        /// <param name="obj">Should be of DataHash type.</param>
        /// <returns>Returns a boolean indicating whether the bytes are the same in both objects.</returns>
        public override bool Equals(object obj) =>
            obj is IDataHash iDataHash ? iDataHash.SourceDataLength == this.SourceDataLength && ReadOnlyMemoryComparer<byte>.Compare(this.HashBytes, iDataHash.HashBytes) == 0 : false;

        /// <summary>
        /// Computes and returns the hash code for the HashBytes in this object.
        /// </summary>
        /// <returns>Returns the hash code for the HashBytes in this object.</returns>
        public override int GetHashCode() =>
            this.HashBytes.GetHashCode();

        /// <summary>
        /// Compares the raw bytes of the hash.
        /// </summary>
        /// <param name="other">Other DataHash to compare bytes with.</param>
        /// <returns>Returns a standard comparison result, -1, 0, 1 for less than, equal, greater than.</returns>
        /// TODO: verify -1/1 correctness
        public int CompareTo(DataHash other) =>
            other.SourceDataLength == this.SourceDataLength ? ReadOnlyMemoryComparer<byte>.Compare(this.HashBytes, other.HashBytes) : (other.SourceDataLength > this.SourceDataLength ? -1 : 1);

        /// <summary>
        /// Returns a boolean whether the two objects contain the same series of bytes.
        /// </summary>
        /// <param name="other">Other DataHash to compare bytes with.</param>
        /// <returns>Returns the standard comparison result, -1, 0, 1 for less than, equal, greater than.</returns>
        public bool Equals(DataHash other) =>
            !(other is null) ? other.SourceDataLength == this.SourceDataLength && ReadOnlyMemoryComparer<byte>.Compare(this.HashBytes, other.HashBytes) == 0 : false;
    }
}
