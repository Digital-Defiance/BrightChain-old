using BrightChain.Attributes;
using BrightChain.Enumerations;
using BrightChain.Interfaces;
using BrightChain.Models.Blocks.DataObjects;
using BrightChain.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightChain.Models.Blocks.Chains
{
    /// <summary>
    /// A block which describes the hashes of all of the blocks needed to reconstitute a resultant block.
    /// TODO: Ensure that the resultant list doesn't exceed a block, split into two lists, make a new top block, etc.
    /// TODO: Ensure that the hash of the source file
    /// TODO: Validate constituent blocks can recompose into that data (break up by tuple size), validate all blocks are same length
    /// </summary>
    public class ConstituentBlockListBlock : SourceBlock, IBlock, IDisposable, IValidatable
    {
        /// <summary>
        /// Hash of the sum bytes of the file when assembled in order
        /// </summary>
        [BrightChainMetadata]
        public BlockHash SourceId { get; }

        /// <summary>
        /// Total length of bytes in the user data section
        /// </summary>
        [BrightChainMetadata]
        public ulong TotalLength { get; }

        /// <summary>
        /// TupleCount at the time
        /// </summary>
        [BrightChainMetadata]
        public int TupleCount { get; } = BlockWhitener.TupleCount;

        /// <summary>
        /// Whether this "file" is encrypted or for public use
        /// </summary>
        [BrightChainMetadata]
        public bool PrivateEncrypted { get; }

        [BrightChainMetadata]
        public BrokeredAnonymityIdentifier CreatorId { get; }

        public BlockChainFileMap BlockMap => new BlockChainFileMap(this);

        public ConstituentBlockListBlock(ConstituentBlockListBlockParams blockArguments)
        : base(
              blockArguments: new TransactableBlockParams(
                  cacheManager: blockArguments.CacheManager,
                  blockArguments: new BlockParams(
                      blockSize: blockArguments.BlockSize,
                      requestTime: blockArguments.RequestTime,
                      keepUntilAtLeast: blockArguments.KeepUntilAtLeast,
                      redundancy: blockArguments.Redundancy,
                      allowCommit: blockArguments.AllowCommit,
                      privateEncrypted: blockArguments.PrivateEncrypted)
            ),
              data: Helpers.RandomDataHelper.DataFiller(
                  inputData: new ReadOnlyMemory<byte>(
                    blockArguments.ConstituentBlocks
                        .SelectMany(b =>
                            b.Id.HashBytes.ToArray())
                        .ToArray()),
                  blockSize: blockArguments.BlockSize)
        )
        {
            // TODO : if finalDataHash is null, reconstitute and compute- or accept the validation result's hash essentially?
        }

        public ReadOnlyMemory<byte> ConstituentBlockHashesBytes => new ReadOnlyMemory<byte>(
                this.ConstituentBlocks
                    .SelectMany(b =>
                        b.Id.HashBytes.ToArray())
                    .ToArray());

        public IEnumerable<BlockHash> ConstituentBlockHashes =>
            this.ConstituentBlocks
                .Select(b => b.Id)
                    .ToArray();

        public double TotalCost =>
            this.ConstituentBlocks.Sum(b => b.RedundancyContract.Cost);


        public static ConstituentBlockListBlock SplitHashList(IEnumerable<BlockHash> blockHashes, BlockSize blockSize)
        {
            var iBlockSize = BlockSizeMap.BlockSize(blockSize);
            var hashesPerBlock = (int)Math.Floor((double)(iBlockSize / iBlockSize));
            while (blockHashes.Any())
            {
                var newCBLHashes = blockHashes.Take(hashesPerBlock);
                // do something
                blockHashes = blockHashes.Skip(hashesPerBlock);
            }
            throw new NotImplementedException();
        }

        public new bool Validate() =>
            // TODO: perform additional validation as described above
            base.Validate();
    }
}
