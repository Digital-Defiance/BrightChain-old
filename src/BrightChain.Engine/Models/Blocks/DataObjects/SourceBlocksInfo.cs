using System.Collections.Generic;
using System.Linq;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Models.Hashes;

namespace BrightChain.Engine.Models.Blocks.DataObjects
{
    public struct SourceBlocksInfo
    {
        public readonly DataHash SourceId;
        public int BytesPerBlock;
        public long TotalBlocksExpected;
        public readonly long TotalBlockedBytes;
        public readonly long HashesPerBlock;
        public readonly int CblsExpected;
        public readonly long BytesPerCbl;

        public SourceBlocksInfo(IEnumerable<Block> blocks)
        {
            var first = blocks.First();
            this.SourceId = new DataHash(blocks.SelectMany(b => b.Data.ToArray()));
            this.BytesPerBlock = BlockSizeMap.BlockSize(first.BlockSize);
            var length = this.BytesPerBlock * blocks.Count();
            this.TotalBlocksExpected = length / this.BytesPerBlock + ((length % this.BytesPerBlock) > 0 ? 1 : 0);
            this.TotalBlockedBytes = this.TotalBlocksExpected * this.BytesPerBlock;
            this.HashesPerBlock = BlockSizeMap.HashesPerBlock(first.BlockSize);
            this.CblsExpected = (int)(this.TotalBlocksExpected / this.HashesPerBlock) + ((this.TotalBlocksExpected % this.HashesPerBlock) > 0 ? 1 : 0);
            this.BytesPerCbl = this.HashesPerBlock * this.BytesPerBlock;
            if (this.CblsExpected > this.HashesPerBlock)
            {
                throw new BrightChainException(nameof(this.CblsExpected));
            }
        }
    }
}
