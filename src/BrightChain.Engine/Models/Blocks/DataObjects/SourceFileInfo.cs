using System;
using System.IO;
using BrightChain.Engine.Enumerations;

namespace BrightChain.Engine.Models.Blocks.DataObjects
{
    public struct SourceFileInfo
    {
        public readonly FileInfo FileInfo;
        public readonly DataHash SourceId;
        public int BytesPerBlock;
        public long TotalBlocksExpected;
        public readonly long TotalBlockedBytes;
        public readonly long HashesPerCbl;
        public readonly int CblsExpected;
        public readonly long BytesPerCbl;

        public SourceFileInfo(string fileName, BlockSize blockSize)
        {
            this.FileInfo = new FileInfo(fileName);
            this.SourceId = new DataHash(fileInfo: this.FileInfo);
            this.BytesPerBlock = BlockSizeMap.BlockSize(blockSize);
            this.TotalBlocksExpected = (int)(this.FileInfo.Length / this.BytesPerBlock) + ((this.FileInfo.Length % this.BytesPerBlock) > 0 ? 1 : 0);
            this.TotalBlockedBytes = this.TotalBlocksExpected * this.BytesPerBlock;
            this.HashesPerCbl = BlockSizeMap.HashesPerBlock(blockSize);
            this.CblsExpected = (int)Math.Ceiling((decimal)(this.TotalBlocksExpected / this.HashesPerCbl));
            this.BytesPerCbl = this.HashesPerCbl * this.BytesPerBlock;
        }

        public SourceFileInfo(FileInfo fileInfo, BlockSize blockSize)
        {
            this.FileInfo = fileInfo;
            this.SourceId = new DataHash(fileInfo: this.FileInfo);
            this.BytesPerBlock = BlockSizeMap.BlockSize(blockSize);
            this.TotalBlocksExpected = (int)(this.FileInfo.Length / this.BytesPerBlock) + ((this.FileInfo.Length % this.BytesPerBlock) > 0 ? 1 : 0);
            this.TotalBlockedBytes = this.TotalBlocksExpected * this.BytesPerBlock;
            this.HashesPerCbl = BlockSizeMap.HashesPerBlock(blockSize);
            this.CblsExpected = (int)Math.Ceiling((decimal)(this.TotalBlocksExpected / this.HashesPerCbl));
            this.BytesPerCbl = this.HashesPerCbl * this.BytesPerBlock;
        }
    }
}
