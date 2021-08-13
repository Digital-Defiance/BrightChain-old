
namespace BrightChain.Engine.Models.Hashes
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Helpers;
    using BrightChain.Engine.Interfaces;
    using FASTER.core;
    using ProtoBuf;

    /// <summary>
    /// Type box for the sha hashes.
    /// </summary>
    [ProtoContract]
    [ProtoInclude(0, typeof(BlockHash))]
    public class DataHash : IDataHash, IComparable<DataHash>, IEquatable<DataHash>, IFasterEqualityComparer<DataHash>
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
        /// <param name="dataBytes">Data to compute hash from.</param>
        public DataHash(IEnumerable<byte> dataBytes)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                this.HashBytes = mySHA256.ComputeHash((byte[])dataBytes);
            }

            this.Computed = true;
            this.SourceDataLength = dataBytes.Count();
        }

        public DataHash(Stream stream)
        {
            using (var sha = SHA256.Create())
            {
                var streamStart = stream.Position;
                sha.ComputeHash(stream);
                var streamLength = stream.Position - streamStart;
                this.HashBytes = sha.Hash;
                this.SourceDataLength = streamLength;
                this.Computed = true;
            }
        }

        public DataHash(FileInfo fileInfo)
        {
            using (Stream stream = File.OpenRead(fileInfo.FullName))
            {
                using (var sha = SHA256.Create())
                {
                    var streamStart = stream.Position;
                    sha.ComputeHash(stream);
                    var streamLength = stream.Position - streamStart;
                    this.HashBytes = sha.Hash;
                    this.SourceDataLength = streamLength;
                    this.Computed = true;
                }

                if (this.SourceDataLength != fileInfo.Length)
                {
                    throw new BrightChainException(nameof(this.SourceDataLength));
                }
            }
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
        [ProtoMember(1)]
        public ReadOnlyMemory<byte> HashBytes { get; }

        /// <summary>
        /// Gets a long containing the length of the source data the hash was computed on.
        /// </summary>
        [ProtoMember(2)]
        public long SourceDataLength { get; }

        /// <summary>
        /// Gets a value indicating whether trusted code calculated this hash.
        /// </summary>
        [ProtoMember(3)]
        public bool Computed { get; }

        public static bool operator ==(DataHash a, DataHash b)
        {
            return a.SourceDataLength == b.SourceDataLength && Helpers.ReadOnlyMemoryComparer<byte>.Compare(a.HashBytes, b.HashBytes) == 0;
        }

        public static bool operator ==(ReadOnlyMemory<byte> b, DataHash a)
        {
            return a.SourceDataLength == b.Length && Helpers.ReadOnlyMemoryComparer<byte>.Compare(a.HashBytes, b) == 0;
        }

        public static bool operator !=(ReadOnlyMemory<byte> b, DataHash a)
        {
            return !(b == a);
        }

        public static bool operator !=(DataHash a, DataHash b)
        {
            return !(b == a);
        }

        /// <summary>
        /// Returns a formatted hash string as a series of lowercase hexadecimal characters.
        /// </summary>
        /// <param name="_">Ignored.</param>
        /// <param name="__">Ignored also.</param>
        /// <returns>Returns a formatted hash string.</returns>
        public string ToString(string _, IFormatProvider __)
        {
            return Helpers.Utilities.HashToFormattedString(this.HashBytes.ToArray());
        }

        /// <summary>
        /// Returns a formatted hash string as a series of lowercase hexadecimal characters.
        /// </summary>
        /// <returns>Returns a formatted hash string.</returns>
        public new string ToString()
        {
            return Helpers.Utilities.HashToFormattedString(this.HashBytes.ToArray());
        }

        /// <summary>
        /// Compares the raw bytes of the hash with a DataHash classed as a plain object.
        /// </summary>
        /// <param name="obj">Should be of DataHash type.</param>
        /// <returns>Returns a boolean indicating whether the bytes are the same in both objects.</returns>
        public override bool Equals(object obj)
        {
            return obj is IDataHash iDataHash ? iDataHash.SourceDataLength == this.SourceDataLength && Helpers.ReadOnlyMemoryComparer<byte>.Compare(this.HashBytes, iDataHash.HashBytes) == 0 : false;
        }

        /// <summary>
        /// Computes and returns the hash code for the HashBytes in this object.
        /// </summary>
        /// <returns>Returns the hash code for the HashBytes in this object.</returns>
        public override int GetHashCode()
        {
            return (int)Crc32.ComputeNewChecksum(this.HashBytes.ToArray());
        }

        /// <summary>
        /// Compares the raw bytes of the hash.
        /// </summary>
        /// <param name="other">Other DataHash to compare bytes with.</param>
        /// <returns>Returns a standard comparison result, -1, 0, 1 for less than, equal, greater than.</returns>
        /// TODO: verify -1/1 correctness
        public int CompareTo(DataHash other)
        {
            return other.SourceDataLength == this.SourceDataLength ? Helpers.ReadOnlyMemoryComparer<byte>.Compare(this.HashBytes, other.HashBytes) : (other.SourceDataLength > this.SourceDataLength ? -1 : 1);
        }

        /// <summary>
        /// Returns a boolean whether the two objects contain the same series of bytes.
        /// </summary>
        /// <param name="other">Other DataHash to compare bytes with.</param>
        /// <returns>Returns the standard comparison result, -1, 0, 1 for less than, equal, greater than.</returns>
        public bool Equals(DataHash other)
        {
            return !(other is null) ? other.SourceDataLength == this.SourceDataLength && Helpers.ReadOnlyMemoryComparer<byte>.Compare(this.HashBytes, other.HashBytes) == 0 : false;
        }

        public long GetHashCode64(ref DataHash k)
        {
            return Crc32.ComputeNewChecksum(k.HashBytes.ToArray());
        }

        public bool Equals(ref DataHash k1, ref DataHash k2)
        {
            return !(k2 is null) ? k2.SourceDataLength == k1.SourceDataLength && Helpers.ReadOnlyMemoryComparer<byte>.Compare(k1.HashBytes, k2.HashBytes) == 0 : false;
        }
    }
}
