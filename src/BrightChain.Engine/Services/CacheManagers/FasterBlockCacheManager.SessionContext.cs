namespace BrightChain.Engine.Services.CacheManagers
{
    using System;
    using BrightChain.Engine.Faster;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Hashes;
    using FASTER.core;

    public partial class FasterBlockCacheManager
    {
        private readonly BlockSessionContext SessionContext;

        private BlockSessionContext NewSharedSessionContext()
        {
            return new BlockSessionContext(
                logger: this.Logger,
metadataSession: this.NewMetadataSession(),
dataSession: this.NewDataSession(),
cblSourceHashSession: this.NewCblSourceHashSession(),
cblCorrelationIdsSession: this.NewCblCorrelationIdSession());
        }

        private ClientSession<BlockHash, BrightenedBlock, BrightenedBlock, BrightenedBlock, BrightChainFasterCacheContext, SimpleFunctions<BlockHash, BrightenedBlock, BrightChainFasterCacheContext>> NewMetadataSession()
        {
            return this.blockMetadataKV.For(functions: new SimpleFunctions<BlockHash, BrightenedBlock, BrightChainFasterCacheContext>())
.NewSession<SimpleFunctions<BlockHash, BrightenedBlock, BrightChainFasterCacheContext>>();
        }

        private ClientSession<BlockHash, BlockData, BlockData, BlockData, BrightChainFasterCacheContext, SimpleFunctions<BlockHash, BlockData, BrightChainFasterCacheContext>> NewDataSession()
        {
            return this.blockDataKV.For(functions: new SimpleFunctions<BlockHash, BlockData, BrightChainFasterCacheContext>())
.NewSession<SimpleFunctions<BlockHash, BlockData, BrightChainFasterCacheContext>>();
        }

        private ClientSession<DataHash, BrightHandle, BrightHandle, BrightHandle, BrightChainFasterCacheContext, SimpleFunctions<DataHash, BrightHandle, BrightChainFasterCacheContext>> NewCblSourceHashSession()
        {
            return this.cblSourceHashesKV.For(functions: new SimpleFunctions<DataHash, BrightHandle, BrightChainFasterCacheContext>())
.NewSession<SimpleFunctions<DataHash, BrightHandle, BrightChainFasterCacheContext>>();
        }

        private ClientSession<Guid, DataHash, DataHash, DataHash, BrightChainFasterCacheContext, SimpleFunctions<Guid, DataHash, BrightChainFasterCacheContext>> NewCblCorrelationIdSession()
        {
            return this.cblCorrelationIdsKV.For(functions: new SimpleFunctions<Guid, DataHash, BrightChainFasterCacheContext>())
.NewSession<SimpleFunctions<Guid, DataHash, BrightChainFasterCacheContext>>();
        }
    }
}
