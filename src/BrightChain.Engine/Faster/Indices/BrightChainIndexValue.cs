namespace BrightChain.Engine.Faster.Indices
{
    using System;
    using ProtoBuf;

    public abstract class BrightChainIndexValue
    {
        public readonly ReadOnlyMemory<byte> Data;

        public BrightChainIndexValue(ReadOnlyMemory<byte> data)
        {
            this.Data = data;
        }

        public BrightChainIndexValue AsIndex => this;
    }
}
