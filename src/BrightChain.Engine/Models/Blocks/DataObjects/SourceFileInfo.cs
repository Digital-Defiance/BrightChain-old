using System.IO;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Exceptions;
using NeuralFabric.Models.Hashes;

namespace BrightChain.Engine.Models.Blocks.DataObjects;

public struct SourceFileInfo
{
    public readonly FileInfo FileInfo;
    public readonly DataHash SourceId;
    public int BytesPerBlock;
    public long TotalBlocksExpected;
    public readonly long TotalBlockedBytes;
    public readonly long HashesPerBlock;
    public readonly int CblsExpected;
    public readonly long BytesPerCbl;

    public SourceFileInfo(string fileName, BlockSize blockSize)
    {
        this.FileInfo = new FileInfo(fileName: fileName);
        this.SourceId = new DataHash(fileInfo: this.FileInfo);
        this.BytesPerBlock = BlockSizeMap.BlockSize(blockSize: blockSize);
        this.TotalBlocksExpected =
            (int)(this.FileInfo.Length / this.BytesPerBlock) + (this.FileInfo.Length % this.BytesPerBlock > 0 ? 1 : 0);
        this.TotalBlockedBytes = this.TotalBlocksExpected * this.BytesPerBlock;
        this.HashesPerBlock = BlockSizeMap.HashesPerBlock(blockSize: blockSize);
        this.CblsExpected = (int)(this.TotalBlocksExpected / this.HashesPerBlock) +
                            (this.TotalBlocksExpected % this.HashesPerBlock > 0 ? 1 : 0);
        this.BytesPerCbl = this.HashesPerBlock * this.BytesPerBlock;
        if (this.CblsExpected > this.HashesPerBlock)
        {
            throw new BrightChainException(message: nameof(this.CblsExpected));
        }
    }

    public SourceFileInfo(FileInfo fileInfo, BlockSize blockSize)
    {
        this.FileInfo = fileInfo;
        this.SourceId = new DataHash(fileInfo: this.FileInfo);
        this.BytesPerBlock = BlockSizeMap.BlockSize(blockSize: blockSize);
        this.TotalBlocksExpected =
            (int)(this.FileInfo.Length / this.BytesPerBlock) + (this.FileInfo.Length % this.BytesPerBlock > 0 ? 1 : 0);
        this.TotalBlockedBytes = this.TotalBlocksExpected * this.BytesPerBlock;
        this.HashesPerBlock = BlockSizeMap.HashesPerBlock(blockSize: blockSize);
        this.CblsExpected = (int)(this.TotalBlocksExpected / this.HashesPerBlock) +
                            (this.TotalBlocksExpected % this.HashesPerBlock > 0 ? 1 : 0);
        this.BytesPerCbl = this.HashesPerBlock * this.BytesPerBlock;
        if (this.CblsExpected > this.HashesPerBlock)
        {
            throw new BrightChainException(message: nameof(this.CblsExpected));
        }
    }
}
