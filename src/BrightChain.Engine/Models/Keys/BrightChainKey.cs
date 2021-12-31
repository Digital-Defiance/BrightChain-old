namespace BrightChain.Engine.Models.Keys
{
    using System.Security.Cryptography;

    public class BrightChainKey : ECDsa
    {
        public override byte[] SignHash(byte[] hash)
        {
            throw new System.NotImplementedException();
        }

        public override bool VerifyHash(byte[] hash, byte[] signature)
        {
            throw new System.NotImplementedException();
        }
    }
}
