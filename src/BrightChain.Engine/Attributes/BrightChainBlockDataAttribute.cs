namespace BrightChain.Engine.Attributes
{
    using System;

    /// <summary>
    /// Properties with this attribute will be serialized into the block's data section which affects the data hash.
    /// </summary>
    public class BrightChainBlockDataAttribute : Attribute
    {
        public BrightChainBlockDataAttribute()
        {
        }
    }
}
