using BrightChain.Engine.Faster.Functions;
using BrightChain.Engine.Faster.Indices;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Models.Hashes;
using FASTER.core;

namespace BrightChain.Engine.Faster.CacheManager;

public partial class FasterBlockCacheManager
{
    private BlockSessionContext NewFasterSessionContext => new(
        logger: this.Logger,
        dataSession: this.NewDataSession,
        cblIndicesSession: this.NewCblIndicesSession);

    private ClientSession<BlockHash, BlockData, BlockData, BlockData, BrightChainFasterCacheContext, BrightChainBlockHashAdvancedFunctions>
        NewDataSession
        => this.KV
            .For(functions: new BrightChainBlockHashAdvancedFunctions())
            .NewSession<BrightChainBlockHashAdvancedFunctions>();

    private ClientSession<string, BrightChainIndexValue, BrightChainIndexValue, BrightChainIndexValue, BrightChainFasterCacheContext,
        BrightChainIndicesFunctions> NewCblIndicesSession
        => this.cblIndicesKV
            .For(functions: new BrightChainIndicesFunctions())
            .NewSession<BrightChainIndicesFunctions>();
}
