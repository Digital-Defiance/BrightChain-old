using System;
using System.Collections.Generic;

namespace BrightChain.Models.Blocks.Chains
{
    /// <summary>
    /// Represents a virtual map of all the contituent tuple-sets/blocks in a given source/reconstructed file. These cannot themselves be committed to disk
    /// The block datas may not actually be loaded in memory, but the appropriate blocks will be loaded (all non-local will be pulled into the cache first) relative to their access offsets.
    /// source file -> blockchainfilemap -> commit
    /// pull blocks from pool -> blockchainfilemap -> read
    /// </summary>
    public class BlockChainFileMap : ConstituentBlockListBlock
    {
        public IEnumerable<TupleStripe> Tuples { get; }
        public BlockChainFileMap(ConstituentBlockListBlockArguments blockArguments) : base(blockArguments: blockArguments)
        {
        }

        public static IEnumerable<TupleStripe> ReconstructTupleStripes(IEnumerable<Block> constituentBlocks) =>
            // var stripe = constituentBlocks.Take(BlockWhitener.TupleCount);
            throw new NotImplementedException();

        public BlockChainFileMap(ConstituentBlockListBlock cblBlock, IEnumerable<TupleStripe> tuples = null)
        : base(
              blockArguments: new ConstituentBlockListBlockArguments(
                  blockArguments: new TransactableBlockArguments(
                  cacheManager: cblBlock.CacheManager,
                  blockArguments: new BlockArguments(
                      blockSize: cblBlock.BlockSize,
                      requestTime: cblBlock.StorageContract.RequestTime,
                      keepUntilAtLeast: cblBlock.StorageContract.KeepUntilAtLeast,
                      redundancy: cblBlock.RedundancyContract.RedundancyContractType,
                      allowCommit: cblBlock.AllowCommit,
                      privateEncrypted: cblBlock.PrivateEncrypted)),
              finalDataHash: cblBlock.SourceId,
              totalLength: cblBlock.TotalLength,
              constituentBlocks: cblBlock.ConstituentBlocks)) =>
                    this.Tuples = (tuples is null) ? tuples : ReconstructTupleStripes(cblBlock.ConstituentBlocks);

        /// <summary>
        /// TODO: extend
        /// </summary>
        /// <returns></returns>
        public new bool Validate() => base.Validate();
    }
}
