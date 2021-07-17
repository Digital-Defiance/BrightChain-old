using System.Collections.Generic;

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

        public ConstituentBlockListBlockParams(TransactableBlockParams blockParams, DataHash sourceId, SegmentHash segmentHash, long totalLength, IEnumerable<BlockHash> constituentBlocks, BlockHash previous = null, BlockHash next = null)
       : base(
             cacheManager: blockParams.CacheManager,
             allowCommit: blockParams.AllowCommit,
             blockParams: blockParams)
        {
            this.SourceId = sourceId;
            this.TotalLength = totalLength;
            this.ConstituentBlocks = constituentBlocks;
            this.SegmentId = segmentHash;
            this.Previous = previous;
            this.Next = next;
        }
    }
}
