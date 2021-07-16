using BrightChain.Engine.Interfaces;

namespace BrightChain.Engine.Models.Blocks.DataObjects
{
    public class TransactableBlockParams : BlockParams
    {
        public ICacheManager<BlockHash, TransactableBlock> CacheManager;

        public TransactableBlockParams(ICacheManager<BlockHash, TransactableBlock> cacheManager, BlockParams blockParams)
            : base(
                  blockSize: blockParams.BlockSize,
                  requestTime: blockParams.RequestTime,
                  keepUntilAtLeast: blockParams.KeepUntilAtLeast,
                  redundancy: blockParams.Redundancy,
                  allowCommit: blockParams.AllowCommit,
                  privateEncrypted: blockParams.PrivateEncrypted)
        {
            CacheManager = cacheManager;
        }
    }
}
