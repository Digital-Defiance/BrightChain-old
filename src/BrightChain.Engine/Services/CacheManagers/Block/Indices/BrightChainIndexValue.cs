using System;

namespace BrightChain.Engine.Faster.Indices;

public abstract class BrightChainIndexValue
{
    public readonly ReadOnlyMemory<byte> Data;

    public BrightChainIndexValue(ReadOnlyMemory<byte> data)
    {
        this.Data = data;
    }

    public BrightChainIndexValue AsIndex => this;
}
