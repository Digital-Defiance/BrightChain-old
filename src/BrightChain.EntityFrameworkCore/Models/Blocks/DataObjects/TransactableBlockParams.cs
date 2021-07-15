using BrightChain.Interfaces;

namespace BrightChain.Models.Blocks.DataObjects
{
    public class TransactableBlockParams : BlockParams
    {
        public ICacheManager<BlockHash, TransactableBlock> CacheManager;

        public TransactableBlockParams(ICacheManager<BlockHash, TransactableBlock> cacheManager, BlockParams blockArguments)
            : base(
                  blockSize: blockArguments.BlockSize,
                  requestTime: blockArguments.RequestTime,
                  keepUntilAtLeast: blockArguments.KeepUntilAtLeast,
                  redundancy: blockArguments.Redundancy,
                  allowCommit: blockArguments.AllowCommit,
                  privateEncrypted: blockArguments.PrivateEncrypted)
        {
            CacheManager = cacheManager;
        }
    }
}
