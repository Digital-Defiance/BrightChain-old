using System;
using System.Collections.Generic;
using System.Linq;
using BrightChain.Engine.Attributes;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Interfaces;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Services;

namespace BrightChain.Engine.Models.Blocks.Chains
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
        public BlockHash SourceId { get; set; }

        /// <summary>
        /// Total length of bytes in the user data section
        /// </summary>
        [BrightChainMetadata]
        public ulong TotalLength { get; set; }

        /// <summary>
        /// TupleCount at the time
        /// </summary>
        [BrightChainMetadata]
        public int TupleCount { get; set; } = BlockWhitener.TupleCount;

        /// <summary>
        /// Whether this "file" is encrypted or for public use
        /// </summary>
        [BrightChainMetadata]
        public bool PrivateEncrypted { get; set; }

        [BrightChainMetadata]
        public BrokeredAnonymityIdentifier CreatorId { get; set; }

        public BlockChainFileMap BlockMap => new BlockChainFileMap(this);

        public ConstituentBlockListBlock(ConstituentBlockListBlockParams blockParams)
        : base(
              blockParams: new TransactableBlockParams(
                  cacheManager: blockParams.CacheManager,
                  blockParams: new BlockParams(
                      blockSize: blockParams.BlockSize,
                      requestTime: blockParams.RequestTime,
                      keepUntilAtLeast: blockParams.KeepUntilAtLeast,
                      redundancy: blockParams.Redundancy,
                      allowCommit: blockParams.AllowCommit,
                      privateEncrypted: blockParams.PrivateEncrypted)
            ),
              data: Helpers.RandomDataHelper.DataFiller(
                  inputData: new ReadOnlyMemory<byte>(
                    blockParams.ConstituentBlocks
                        .SelectMany(b =>
                            b.Id.HashBytes.ToArray())
                        .ToArray()),
                  blockSize: blockParams.BlockSize)
        )
        {
            // TODO : if finalDataHash is null, reconstitute and compute- or accept the validation result's hash essentially?
            SourceId = blockParams.FinalDataHash;
            TotalLength = blockParams.TotalLength;
            PrivateEncrypted = blockParams.PrivateEncrypted;
            ConstituentBlocks = blockParams.ConstituentBlocks;
        }

        public ReadOnlyMemory<byte> ConstituentBlockHashesBytes => new ReadOnlyMemory<byte>(
                ConstituentBlocks
                    .SelectMany(b =>
                        b.Id.HashBytes.ToArray())
                    .ToArray());

        public IEnumerable<BlockHash> ConstituentBlockHashes =>
            ConstituentBlocks
                .Select(b => b.Id)
                    .ToArray();

        public double TotalCost =>
            ConstituentBlocks.Sum(b => b.RedundancyContract.Cost);


        public static ConstituentBlockListBlock SplitHashList(IEnumerable<BlockHash> blockHashes, BlockSize blockSize)
        {
            throw new NotImplementedException();
            var iBlockSize = BlockSizeMap.BlockSize(blockSize);
            var hashesPerBlock = (int)Math.Floor((double)(iBlockSize / iBlockSize));
            while (blockHashes.Any())
            {
                var newCBLHashes = blockHashes.Take(hashesPerBlock);
                // do something
                blockHashes = blockHashes.Skip(hashesPerBlock);
            }
        }

        public new bool Validate()
        {
            // TODO: perform additional validation as described above
            return base.Validate();
        }
    }
}
