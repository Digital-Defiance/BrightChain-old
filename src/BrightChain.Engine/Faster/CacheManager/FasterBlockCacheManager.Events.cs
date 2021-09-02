namespace BrightChain.Engine.Faster.CacheManager
{
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Hashes;

    public partial class FasterBlockCacheManager
    {
        /// <summary>
        ///     Fired whenever a block is added to the cache
        /// </summary>
        public override event ICacheManager<BlockHash, BrightenedBlock>.KeyAddedEventHandler KeyAdded;

        /// <summary>
        ///     Fired whenever a block is expired from the cache
        /// </summary>
        public override event ICacheManager<BlockHash, BrightenedBlock>.KeyExpiredEventHandler KeyExpired;

        /// <summary>
        ///     Fired whenever a block is removed from the collection
        /// </summary>
        public override event ICacheManager<BlockHash, BrightenedBlock>.KeyRemovedEventHandler KeyRemoved;

        /// <summary>
        ///     Fired whenever a block is requested from the cache but is not present.
        /// </summary>
        public override event ICacheManager<BlockHash, BrightenedBlock>.CacheMissEventHandler CacheMiss;
    }
}
