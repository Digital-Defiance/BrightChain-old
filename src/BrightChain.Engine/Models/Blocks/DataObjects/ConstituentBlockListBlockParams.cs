using System.Collections.Generic;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Models.Hashes;

namespace BrightChain.Engine.Models.Blocks.DataObjects
{
    public class ConstituentBlockListBlockParams : TransactableBlockParams
    {
        public readonly DataHash SourceId;
        public readonly long TotalLength;
        public readonly IEnumerable<BlockHash> ConstituentBlocks;

        /// <summary>
        /// Hash of the sum bytes of the segment of the file contained in this CBL when assembled in order.
        /// </summary>
        public SegmentHash SegmentId;
        public readonly BlockHash Previous = null;
        public readonly BlockHash Next = null;

        public ConstituentBlockListBlockParams(TransactableBlockParams blockParams, DataHash sourceId, SegmentHash segmentId, long totalLength, IEnumerable<BlockHash> constituentBlocks, BlockHash previous = null, BlockHash next = null)
       : base(
             cacheManager: blockParams.CacheManager,
             allowCommit: blockParams.AllowCommit,
             blockParams: blockParams)
        {
            this.SourceId = sourceId;
            this.TotalLength = totalLength;
            this.ConstituentBlocks = constituentBlocks;
            this.SegmentId = segmentId;
            this.Previous = previous;
            this.Next = next;
        }

        public ConstituentBlockListBlockParams Merge(ConstituentBlockListBlockParams otherBlockParams)
        {
            if (otherBlockParams.BlockSize != this.BlockSize)
            {
                throw new BrightChainException("BlockSize mismatch");
            }

            var newConstituentBlocks = new List<BlockHash>(this.ConstituentBlocks);
            newConstituentBlocks.AddRange(otherBlockParams.ConstituentBlocks);

            return new ConstituentBlockListBlockParams(
                blockParams: this.Merge(otherBlockParams),
                sourceId: this.SourceId,
                segmentId: this.SegmentId,
                totalLength: this.TotalLength > otherBlockParams.TotalLength ? this.TotalLength : otherBlockParams.TotalLength,
                constituentBlocks: newConstituentBlocks,
                previous: this.Previous,
                next: this.Next);
        }
    }
}
