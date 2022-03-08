using System;
using System.Security.Cryptography;

namespace BrightChain.Engine.Models.Keys;

public class BrightChainKey : ECDsa
{
    public override byte[] SignHash(byte[] hash)
    {
        throw new NotImplementedException();
    }

    public override bool VerifyHash(byte[] hash, byte[] signature)
    {
        throw new NotImplementedException();
    }
}
