namespace BrightChain.Engine.Models.Blocks
{
    using System;

    /// <summary>
    /// Constitues really just the user's id- however the system will allow users to store data with registered identities or even anonymously
    /// but we will store error correction (FEC) data that allows us to recover the original Id if all the pieces of it (once sharded) are reconstructed.
    /// </summary>
    public class BrokeredAnonymityIdentifier : IComparable<BrokeredAnonymityIdentifier>, IDisposable, IFormattable, IEquatable<BrokeredAnonymityIdentifier>
    {
        /// <summary>
        /// Filled in with either the real Id, an alias of, or the the "anonymous" user (00000.. all zero).
        /// </summary>
        public ReadOnlyMemory<byte> Id;

        /// <summary>
        /// FEC data computed off the ID prior to any masking
        /// </summary>
        public ReadOnlyMemory<byte> ChecksumRecovery;

        public BrokeredAnonymityIdentifier(ReadOnlyMemory<byte> originalId, ReadOnlyMemory<byte> requestedId)
        {
        }

        public int CompareTo(BrokeredAnonymityIdentifier other)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool Equals(BrokeredAnonymityIdentifier other)
        {
            throw new NotImplementedException();
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            throw new NotImplementedException();
        }
    }
}
