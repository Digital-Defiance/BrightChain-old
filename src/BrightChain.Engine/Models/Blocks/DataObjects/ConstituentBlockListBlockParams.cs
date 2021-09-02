namespace BrightChain.Engine.Models.Blocks.DataObjects
{
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Models.Hashes;

    public class ConstituentBlockListBlockParams : BlockParams
    {
        public readonly DataHash SourceId;

        public readonly long TotalLength;

        public readonly IEnumerable<BlockHash> ConstituentBlockHashes;

        /// <summary>
        /// Hash of the sum bytes of the segment of the file contained in this CBL when assembled in order.
        /// </summary>
        public SegmentHash SegmentId;

        public readonly BlockHash Previous = null;

        public readonly BlockHash Next = null;

        public readonly Guid CorrelationId;

        public readonly DataHash PreviousVersionHash = null;

        public ConstituentBlockListBlockParams(BlockParams blockParams, DataHash sourceId, SegmentHash segmentId, long totalLength, IEnumerable<BlockHash> constituentBlockHashes, BlockHash previous = null, BlockHash next = null, Guid? correlationId = null, DataHash previousVersionHash = null)
       : base(
             blockSize: blockParams.BlockSize,
             requestTime: blockParams.RequestTime,
             keepUntilAtLeast: blockParams.KeepUntilAtLeast,
             redundancy: blockParams.Redundancy,
             privateEncrypted: blockParams.PrivateEncrypted,
             originalType: blockParams.OriginalType)
        {
            if (correlationId.HasValue && previousVersionHash is null)
            {
                throw new BrightChainException("Should not have correlation Id specified with no previous hash");
            }

            this.SourceId = sourceId;
            this.TotalLength = totalLength;
            this.ConstituentBlockHashes = constituentBlockHashes;
            this.SegmentId = segmentId;
            this.Previous = previous;
            this.Next = next;
            this.CorrelationId = correlationId.HasValue ? correlationId.Value : Guid.NewGuid();
            this.PreviousVersionHash = previousVersionHash;
        }

        public ConstituentBlockListBlockParams Merge(ConstituentBlockListBlockParams otherBlockParams)
        {
            if (otherBlockParams.BlockSize != this.BlockSize)
            {
                throw new BrightChainException("BlockSize mismatch");
            }

            var newConstituentBlocks = new List<BlockHash>(this.ConstituentBlockHashes);
            newConstituentBlocks.AddRange(otherBlockParams.ConstituentBlockHashes);

            return new ConstituentBlockListBlockParams(
                blockParams: this.Merge(otherBlockParams),
                sourceId: this.SourceId,
                segmentId: this.SegmentId,
                totalLength: this.TotalLength > otherBlockParams.TotalLength ? this.TotalLength : otherBlockParams.TotalLength,
                constituentBlockHashes: newConstituentBlocks,
                previous: this.Previous,
                next: this.Next,
                correlationId: this.CorrelationId);
        }
    }
}
