using System;
using System.Security.Cryptography;
using BrightChain.Engine.Models.Keys;

namespace BrightChain.Engine.Models.Agents;

public class BrightChainAgent
{
    public Guid Id { get; }

    private BrightChainKey AgentKey { get; }

    public ECDiffieHellmanCngPublicKey PublicKey
    {
        get
        {
            var keyInfo = this.AgentKey.ExportSubjectPublicKeyInfo();

            return ECDiffieHellmanCngPublicKey.FromByteArray(publicKeyBlob: keyInfo,
                format: CngKeyBlobFormat.EccPublicBlob) as ECDiffieHellmanCngPublicKey;
        }
    }
}
