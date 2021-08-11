namespace BrightChain.Engine.Enumerations
{
    /// <summary>
    /// List of the pre-specified block sizes this node supports
    /// The BlockSizeMap class contains the map to the actual sizes.
    /// </summary>
    public enum BlockSize
    {
        /// <summary>
        /// Invalid/indeterminate/unknown block size
        /// </summary>
        Unknown,

        /// <summary>
        /// Tiniest message size, for extremely small messages. 256b
        /// </summary>
        Micro,

        /// <summary>
        /// Message size, such as  a small data blob, currently 512b
        /// </summary>
        Message,

        /// <summary>
        /// Tiny size, such as smaller messages and configs, currently 1K
        /// </summary>
        Tiny,

        /// <summary>
        /// Small size, such as small data files up to a mb or so depending on desired block count, currently 4K 
        /// </summary>
        Small,

        /// <summary>
        /// Medium size, such as medium data files up to 5-100mb, currently 1M
        /// </summary>
        Medium,

        /// <summary>
        /// Large size, such as large data files over 4M up to many terabytes.
        /// </summary>
        Large,
    }
}
