using BrightChain.Enumerations;
using BrightChain.Helpers;
using BrightChain.Interfaces;
using System;
using System.Security.Cryptography;

namespace BrightChain.Models.Blocks
{
    /// <summary>
    /// Type box for the sha hashes of signatures
    /// </summary>
    public class DataSignature : IDataSignature, IComparable<DataSignature>
    {
        public const int SignatureHashSize = 64;

        public ReadOnlyMemory<byte> SignatureHashBytes { get; protected set; }

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

        public string ToString(string format, IFormatProvider _) => this.ToString();

        public new string ToString() => BitConverter.ToString(this.SignatureHashBytes.ToArray());

        public static bool operator ==(DataSignature a, DataSignature b) => ReadOnlyMemoryComparer<byte>.Compare(a.SignatureHashBytes, b.SignatureHashBytes) == 0;

        public static bool operator ==(ReadOnlyMemory<byte> b, DataSignature a) => ReadOnlyMemoryComparer<byte>.Compare(a.SignatureHashBytes, b) == 0;

        public static bool operator !=(ReadOnlyMemory<byte> b, DataSignature a) => !(b == a);

        public static bool operator !=(DataSignature a, DataSignature b) => !a.Equals(b);

        public override bool Equals(object obj) => ReadOnlyMemoryComparer<byte>.Compare(this.SignatureHashBytes, (obj as DataSignature).SignatureHashBytes) == 0;

        public override int GetHashCode() => this.SignatureHashBytes.GetHashCode();

        public int CompareTo(DataSignature other) => ReadOnlyMemoryComparer<byte>.Compare(this.SignatureHashBytes, other.SignatureHashBytes);
    }
}