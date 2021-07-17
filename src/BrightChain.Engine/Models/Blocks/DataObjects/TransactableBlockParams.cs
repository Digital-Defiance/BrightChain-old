using BrightChain.Engine.Interfaces;

namespace BrightChain.Engine.Models.Blocks.DataObjects
{
    public class TransactableBlockParams : BlockParams
    {
        public ICacheManager<BlockHash, TransactableBlock> CacheManager;

        public bool AllowCommit { get; }

        public TransactableBlockParams(ICacheManager<BlockHash, TransactableBlock> cacheManager, bool allowCommit, BlockParams blockParams)
            : base(
                  blockSize: blockParams.BlockSize,
                  requestTime: blockParams.RequestTime,
                  keepUntilAtLeast: blockParams.KeepUntilAtLeast,
                  redundancy: blockParams.Redundancy,
                  privateEncrypted: blockParams.PrivateEncrypted)
        {
            this.CacheManager = cacheManager;
            this.AllowCommit = allowCommit;
        }
    }
}
