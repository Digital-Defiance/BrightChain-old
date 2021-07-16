namespace BrightChain.Engine.Models.Contracts
{
    using System;

    /// <summary>
    /// Type box for the revocation certificates/tokens to delete private/encrypted blocks.
    /// </summary>
    public class RevocationCertificate : IComparable<RevocationCertificate>
    {
        /// <summary>
        /// Compares this RevocationCertificate to another for equality.
        /// </summary>
        /// <param name="other">Other RevocationCertificate instance to compare to.</param>
        /// <returns>integer representing -1, 0, or 1 for <, =, and >, respectively.</returns>
        public int CompareTo(RevocationCertificate other)
        {
            throw new NotImplementedException();
        }
    }
}
