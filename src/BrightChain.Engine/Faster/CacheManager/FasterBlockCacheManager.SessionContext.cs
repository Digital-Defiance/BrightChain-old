namespace BrightChain.Engine.Faster.CacheManager
{
    using System;
    using BrightChain.Engine.Faster;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Hashes;
    using FASTER.core;

    public partial class FasterBlockCacheManager
    {
        private readonly BlockSessionContext sessionContext;

        private BlockSessionContext NewSharedSessionContext => new BlockSessionContext(
                logger: this.Logger,
                metadataSession: this.NewMetadataSession,
                dataSession: this.NewDataSession,
                expirationSession: this.NewExpirationSession,
                cblSourceHashSession: this.NewCblSourceHashSession,
                cblCorrelationIdsSession: this.NewCblCorrelationIdSession);

        private ClientSession<BlockHash, BrightenedBlock, BrightenedBlock, BrightenedBlock, BrightChainFasterCacheContext, SimpleFunctions<BlockHash, BrightenedBlock, BrightChainFasterCacheContext>> NewMetadataSession
            => this.primaryMetadataKV
                .For(functions: new SimpleFunctions<BlockHash, BrightenedBlock, BrightChainFasterCacheContext>())
                .NewSession<SimpleFunctions<BlockHash, BrightenedBlock, BrightChainFasterCacheContext>>();

        private ClientSession<BlockHash, BlockData, BlockData, BlockData, BrightChainFasterCacheContext, SimpleFunctions<BlockHash, BlockData, BrightChainFasterCacheContext>> NewDataSession
            => this.primaryDataKV
                .For(functions: new SimpleFunctions<BlockHash, BlockData, BrightChainFasterCacheContext>())
                .NewSession<SimpleFunctions<BlockHash, BlockData, BrightChainFasterCacheContext>>();

        private ClientSession<long, List<BlockHash>, List<BlockHash>, List<BlockHash>, BrightChainFasterCacheContext, SimpleFunctions<long, List<BlockHash>, BrightChainFasterCacheContext>> NewExpirationSession
            => this.primaryExpirationKV
                .For(functions: new SimpleFunctions<long, List<BlockHash>, BrightChainFasterCacheContext>())
                .NewSession<SimpleFunctions<long, List<BlockHash>, BrightChainFasterCacheContext>>();

        private ClientSession<DataHash, BrightHandle, BrightHandle, BrightHandle, BrightChainFasterCacheContext, SimpleFunctions<DataHash, BrightHandle, BrightChainFasterCacheContext>> NewCblSourceHashSession
            => this.cblSourceHashesKV
                .For(functions: new SimpleFunctions<DataHash, BrightHandle, BrightChainFasterCacheContext>())
                .NewSession<SimpleFunctions<DataHash, BrightHandle, BrightChainFasterCacheContext>>();

        private ClientSession<Guid, DataHash, DataHash, DataHash, BrightChainFasterCacheContext, SimpleFunctions<Guid, DataHash, BrightChainFasterCacheContext>> NewCblCorrelationIdSession
            => this.cblCorrelationIdsKV
                .For(functions: new SimpleFunctions<Guid, DataHash, BrightChainFasterCacheContext>())
                .NewSession<SimpleFunctions<Guid, DataHash, BrightChainFasterCacheContext>>();
    }
}
