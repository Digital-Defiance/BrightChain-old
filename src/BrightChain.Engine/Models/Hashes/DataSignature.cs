using System;
using System.Security.Cryptography;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Helpers;
using BrightChain.Engine.Interfaces;

namespace BrightChain.Engine.Models.Hashes
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
            Computed = true;
        }

        public DataSignature(BlockSize originalBlockSize, ReadOnlyMemory<byte> providedHashBytes)
        {
            SignatureHashBytes = providedHashBytes;
            Computed = false;
        }

        internal DataSignature(BlockSize originalBlockSize, ReadOnlyMemory<byte> providedHashBytes, bool computed = false)
        {
            SignatureHashBytes = providedHashBytes;
            Computed = computed;
        }

        public DataSignature(ReadOnlyMemory<byte> dataBytes)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                throw new NotImplementedException();
            }
            Computed = true;
        }

        public string ToString(string format, IFormatProvider _)
        {
            return BitConverter.ToString(SignatureHashBytes.ToArray()).Replace("-", string.Empty).ToLower(culture: System.Globalization.CultureInfo.InvariantCulture);
        }

        public new string ToString()
        {
            return BitConverter.ToString(SignatureHashBytes.ToArray()).Replace("-", string.Empty).ToLower(culture: System.Globalization.CultureInfo.InvariantCulture);
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
            return obj is DataSignature ? ReadOnlyMemoryComparer<byte>.Compare(SignatureHashBytes, (obj as DataSignature).SignatureHashBytes) == 0 : false;
        }

        public override int GetHashCode()
        {
            return SignatureHashBytes.GetHashCode();
        }

        public int CompareTo(DataSignature other)
        {
            return ReadOnlyMemoryComparer<byte>.Compare(SignatureHashBytes, other.SignatureHashBytes);
        }
    }
}
