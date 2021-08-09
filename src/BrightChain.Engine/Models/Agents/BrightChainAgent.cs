namespace BrightChain.Engine.Models.Agents
{
    using System;
    using System.Security.Cryptography;
    using BrightChain.Engine.Models.Keys;

    public class BrightChainAgent
    {
        public Guid Id { get; }

        private BrightChainKey AgentKey { get; }

        public ECDiffieHellmanCngPublicKey PublicKey
        {
            get
            {
                var keyInfo = this.AgentKey.ExportSubjectPublicKeyInfo();

                return ECDiffieHellmanCngPublicKey.FromByteArray(keyInfo, CngKeyBlobFormat.EccPublicBlob) as ECDiffieHellmanCngPublicKey;
            }
        }
    }
}
