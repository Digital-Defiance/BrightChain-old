using BrightChain.Engine.Attributes;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Interfaces;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Models.Hashes;
using BrightChain.Engine.Services;

namespace BrightChain.Engine.Models.Blocks.Chains
{
    /// <summary>
    /// A block which describes the hashes of all of the blocks needed to reconstitute a resultant block.
    /// TODO: Ensure that the resultant list doesn't exceed a block, split into two lists, make a new top block, etc.
    /// TODO: Ensure that the hash of the source file
    /// TODO: Validate constituent blocks can recompose into that data (break up by tuple size), validate all blocks are same length
    /// </summary>
    public class ConstituentBlockListBlock : TransactableBlock, IBlock, IDisposable, IValidatable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConstituentBlockListBlock"/> class.
        /// </summary>
        /// <param name="blockParams"></param>
        public ConstituentBlockListBlock(ConstituentBlockListBlockParams blockParams)
        : base(
              blockParams: blockParams,
              data: Helpers.RandomDataHelper.DataFiller(
                  inputData: blockParams.ConstituentBlocks
                            .SelectMany(b => b.HashBytes.ToArray())
                             .ToArray(),
                  blockSize: blockParams.BlockSize))
        {
            // TODO : if finalBlockHash is null, reconstitute and compute- or accept the validation result's hash essentially?
            this.SourceId = blockParams.SourceId;
            this.TotalLength = blockParams.TotalLength;
            this.PrivateEncrypted = blockParams.PrivateEncrypted;
            this.ConstituentBlocks = blockParams.ConstituentBlocks;
            this.Previous = blockParams.Previous;
            this.Next = blockParams.Next;
            this.TupleCount = BlockBrightenerService.TupleCount;
        }

        /// <summary>
        /// Gets a CBLBlockParams object with the parameters of this block.
        /// </summary>
        public override ConstituentBlockListBlockParams BlockParams => new ConstituentBlockListBlockParams(
                    blockParams: new TransactableBlockParams(
                        cacheManager: this.CacheManager,
                        allowCommit: this.AllowCommit,
                        blockParams: new BlockParams(
                            blockSize: this.BlockSize,
                            requestTime: this.StorageContract.RequestTime,
                            keepUntilAtLeast: this.StorageContract.KeepUntilAtLeast,
                            redundancy: this.StorageContract.RedundancyContractType,
                            privateEncrypted: this.StorageContract.PrivateEncrypted,
                            originalType: Type.GetType(this.OriginalType))),
                    sourceId: this.SourceId,
                    segmentId: this.SegmentId,
                    totalLength: this.TotalLength,
                    constituentBlocks: this.ConstituentBlocks,
                    previous: this.Previous,
                    next: this.Next);

        /// <summary>
        /// Gets or sets the hash of the sum bytes of the file when assembled in order.
        /// </summary>
        [BrightChainMetadata]
        public DataHash SourceId { get; set; }

        /// <summary>
        /// Gets or sets the total length of bytes in the user data section.
        /// </summary>
        [BrightChainMetadata]
        public long TotalLength { get; set; }

        /// <summary>
        /// Gets or sets an int with the TupleCount at the time of creation.
        /// </summary>
        [BrightChainMetadata]
        public int TupleCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this "file" is encrypted or for public use.
        /// </summary>
        [BrightChainMetadata]
        public bool PrivateEncrypted { get; set; }

        /// <summary>
        /// Gets or sets the hash of the sum bytes of the segment of the file contained in this CBL when assembled in order.
        /// If the segment does not fill the the final block, the hash does not include the remainder of the data.
        /// </summary>
        [BrightChainMetadata]
        public SegmentHash SegmentId { get; set; }

        /// <summary>
        /// Gets or sets the BlockHash of the previous CBL in this CBL Chain.
        /// </summary>
        [BrightChainMetadata]

        public BlockHash Previous { get; set; }

        /// <summary>
        /// Gets or sets the hash of the next CBL in this CBL Chain.
        /// </summary>
        [BrightChainMetadata]
        public BlockHash Next { get; set; }

        /// <summary>
        /// Gets or sets the BrightChainID of the block's creator.
        /// </summary>
        [BrightChainMetadata]
        public BrokeredAnonymityIdentifier CreatorId { get; set; }

        /// <summary>
        /// Gets an array of the bytes of the constituent block hashes for writing to disk.
        /// </summary>
        public ReadOnlyMemory<byte> ConstituentBlockHashesBytes => new ReadOnlyMemory<byte>(
                this.ConstituentBlocks
                    .SelectMany(b =>
                        b.HashBytes.ToArray())
                    .ToArray());

        /// <summary>
        /// Gets a value indicating the computed cost of storing this contract.
        /// </summary>
        public double TotalCost { get; set; }

        /// <summary>
        /// Gets an int representing the computed capacity of this block in terms of number of BlockHashes.
        /// </summary>
        public int MaximumHashesPerBlock =>
            (int)Math.Floor((double)(BlockHash.HashSize / 8) / BlockSizeMap.BlockSize(this.BlockSize));

        /// <summary>
        /// Generate a BlockMap from the list of constituent blocks.
        /// </summary>
        /// <returns>BlockChainFileMap with TupleStripes of the chain.</returns>
        public BlockChainFileMap GenerateBlockMap()
        {
            return new BlockChainFileMap(this);
        }

        /// <summary>
        /// Perform validation of this CBL and its underlying data.
        /// </summary>
        /// <returns>A boolean indicating whether validation succeeded.</returns>
        public new bool Validate()
        {
            if (!base.Validate())
            {
                return false;
            }

            var validationExceptions = new List<BrightChainValidationException>(this.ValidationExceptions);

            if (!(this.Previous is null))
            {
                if (!(this.Previous is ConstituentBlockListBlock))
                {
                    validationExceptions.Add(new BrightChainValidationException(
                        element: nameof(this.Previous),
                        message: "Previous object must be a CBL."));
                }
            }

            // TODO: perform additional validation as described above
            return true;
        }
    }
}
