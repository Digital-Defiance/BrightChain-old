namespace BrightChain.Engine.Faster.CacheManager
{
    using BrightChain.Engine.Faster;
    using BrightChain.Engine.Faster.Indices;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Hashes;
    using FASTER.core;

    public partial class FasterBlockCacheManager
    {
        private readonly BlockSessionContext sessionContext;

        private BlockSessionContext NewSharedSessionContext => new BlockSessionContext(
                logger: this.Logger,
                dataSession: this.NewDataSession,
                cblIndicesSession: this.NewCblIndicesSession);

        private ClientSession<BlockHash, BlockData, BlockData, BlockData, BrightChainFasterCacheContext, SimpleFunctions<BlockHash, BlockData, BrightChainFasterCacheContext>> NewDataSession
            => this.primaryDataKV
                .For(functions: new SimpleFunctions<BlockHash, BlockData, BrightChainFasterCacheContext>())
                .NewSession<SimpleFunctions<BlockHash, BlockData, BrightChainFasterCacheContext>>();

        private ClientSession<string, BrightChainIndexValue, BrightChainIndexValue, BrightChainIndexValue, BrightChainFasterCacheContext, SimpleFunctions<string, BrightChainIndexValue, BrightChainFasterCacheContext>> NewCblIndicesSession
            => this.cblIndicesKV
                .For(functions: new SimpleFunctions<string, BrightChainIndexValue, BrightChainFasterCacheContext>())
                .NewSession<SimpleFunctions<string, BrightChainIndexValue, BrightChainFasterCacheContext>>();
    }
}
