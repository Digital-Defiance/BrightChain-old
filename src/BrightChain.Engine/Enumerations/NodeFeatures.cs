namespace BrightChain.Engine.Enumerations
{
    using System;

    /// <summary>
    /// Exhaustive list of supported node protocol sources and destinations.
    /// </summary>
    public enum NodeFeatures
    {
        /// <summary>
        /// This node consumes or offers RandomizerBlocks.
        /// </summary>
        RandomizerCache,

        /// <summary>
        /// This node consumes or offers public MemoryCache.
        /// </summary>
        MemoryCache,

        /// <summary>
        /// This node consumes or offers HeapLowPriority storage blocks.
        /// </summary>
        HeapLowStorage,

        /// <summary>
        /// This node consumes or offers HeapHighPriority storage blocks.
        /// </summary>
        HeapHighStorage,

        /// <summary>
        /// This node consumes or offers non-replicated magnetic storage.
        /// </summary>
        BasicLocal,

        /// <summary>
        /// This node consumes or offers non-replicated SSD storage.
        /// </summary>
        FastLocal,

        /// <summary>
        /// This node consumes or offers replicated magnetic storage. Mirror or better.
        /// </summary>
        RedundantBasicLocal,

        /// <summary>
        /// This node consumes or offers replicated SSD storage. Mirror or better.
        /// </summary>
        RedundantFastLocal,

        /// <summary>
        /// This node consumes or offers quarum block validation.
        /// </summary>
        BlockValidation,

        /// <summary>
        /// This node consumes or offers signed block JavaScript (NodeJS) execution/validation.
        /// </summary>
        JavaScriptCodeExecution,

        /// <summary>
        /// This node consumes or offers signed block CLR/CIL (C#, VB.NET, etc, even PHP via PeachPie).
        /// </summary>
        CilClrCodeExecution,
    }
}
