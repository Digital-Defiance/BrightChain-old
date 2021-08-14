namespace BrightChain.Engine.Models.Hashes
{
    using System.Security.Cryptography;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Helpers;
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Contracts;
    using ProtoBuf;

    /// <summary>
    /// Type box for the sha hashes of signatures.
    /// </summary>
    [ProtoContract]
    [ProtoInclude(1, typeof(BlockSignature))]
    [ProtoInclude(2, typeof(RevocationCertificate))]
    public class DataSignature : IDataSignature, IComparable<DataSignature>
    {
        /// <summary>
        /// Size in bits of the hash.
        /// </summary>
        public const int SignatureHashSize = 256;

        /// <summary>
        /// Size in bytes of the hash.
        /// </summary>
        public const int SignatureHashSizeBytes = SignatureHashSize / 8;

        [ProtoMember(1)]
        public ReadOnlyMemory<byte> SignatureHashBytes { get; protected set; }

        [ProtoMember(2)]
        public bool Computed { get; }

        public DataSignature(IBlock block)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                throw new NotImplementedException();
            }

            this.Computed = true;
        }

        public DataSignature(BlockSize originalBlockSize, ReadOnlyMemory<byte> providedHashBytes)
        {
            this.SignatureHashBytes = providedHashBytes;
            this.Computed = false;
        }

        internal DataSignature(BlockSize originalBlockSize, ReadOnlyMemory<byte> providedHashBytes, bool computed = false)
        {
            this.SignatureHashBytes = providedHashBytes;
            this.Computed = computed;
        }

        public DataSignature(ReadOnlyMemory<byte> dataBytes)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                throw new NotImplementedException();
            }

            this.Computed = true;
        }

        public string ToString(string format, IFormatProvider _)
        {
            return BitConverter.ToString(this.SignatureHashBytes.ToArray()).Replace("-", string.Empty).ToLower(culture: System.Globalization.CultureInfo.InvariantCulture);
        }

        public new string ToString()
        {
            return BitConverter.ToString(this.SignatureHashBytes.ToArray()).Replace("-", string.Empty).ToLower(culture: System.Globalization.CultureInfo.InvariantCulture);
        }

        public static bool operator ==(DataSignature a, DataSignature b)
        {
            return ReadOnlyMemoryComparer<byte>.Compare(a.SignatureHashBytes, b.SignatureHashBytes) == 0;
        }

        public static bool operator ==(ReadOnlyMemory<byte> b, DataSignature a)
        {
            return ReadOnlyMemoryComparer<byte>.Compare(a.SignatureHashBytes, b) == 0;
        }

        public static bool operator !=(ReadOnlyMemory<byte> b, DataSignature a)
        {
            return !(b == a);
        }

        public static bool operator !=(DataSignature a, DataSignature b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            return obj is DataSignature ? ReadOnlyMemoryComparer<byte>.Compare(this.SignatureHashBytes, (obj as DataSignature).SignatureHashBytes) == 0 : false;
        }

        public override int GetHashCode()
        {
            return this.SignatureHashBytes.GetHashCode();
        }

        public int CompareTo(DataSignature other)
        {
            return ReadOnlyMemoryComparer<byte>.Compare(this.SignatureHashBytes, other.SignatureHashBytes);
        }
    }
}
