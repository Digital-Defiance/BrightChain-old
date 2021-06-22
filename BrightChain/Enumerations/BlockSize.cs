namespace BrightChain.Enumerations
{
    public enum BlockSize
    {
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
        Large
    }
}