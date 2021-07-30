namespace BrightChain.Engine.Attributes
{
    using System;

    /// <summary>
    /// Properties with this attribute will be serialized into the block's metadata section which does not affect the data hash
    /// </summary>
    public class BrightChainMetadataAttribute : Attribute
    {
        public BrightChainMetadataAttribute()
        {
        }
    }
}
