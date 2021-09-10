namespace BrightChain.Engine.Faster.CacheManager
{
    using BrightChain.Engine.Faster;
    using BrightChain.Engine.Faster.Indices;
    using BrightChain.Engine.Faster.Functions;
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

        private ClientSession<BlockHash, BlockData, BlockData, BlockData, BrightChainFasterCacheContext, BrightChainAdvancedFunctions<BlockHash, BlockData, BlockData, BlockData, BrightChainFasterCacheContext>> NewDataSession
            => this.primaryDataKV
                .For(functions: new BrightChainAdvancedFunctions<BlockHash, BlockData, BlockData, BlockData, BrightChainFasterCacheContext>())
                .NewSession<BrightChainAdvancedFunctions<BlockHash, BlockData, BlockData, BlockData, BrightChainFasterCacheContext>>();

        private ClientSession<string, BrightChainIndexValue, BrightChainIndexValue, BrightChainIndexValue, BrightChainFasterCacheContext, BrightChainAdvancedFunctions<string, BrightChainIndexValue, BrightChainIndexValue, BrightChainIndexValue, BrightChainFasterCacheContext>> NewCblIndicesSession
            => this.cblIndicesKV
                .For(functions: new BrightChainAdvancedFunctions<string, BrightChainIndexValue, BrightChainIndexValue, BrightChainIndexValue, BrightChainFasterCacheContext>())
                .NewSession<BrightChainAdvancedFunctions<string, BrightChainIndexValue, BrightChainIndexValue, BrightChainIndexValue, BrightChainFasterCacheContext>>();
    }
}
