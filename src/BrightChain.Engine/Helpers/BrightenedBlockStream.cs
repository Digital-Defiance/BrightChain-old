namespace BrightChain.Engine.Helpers
{
    using System;
    using System.IO;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Models.Blocks;

    public class BrightenedBlockStream : Stream
    {
        private readonly Stream sourceStream;
        private readonly Stream destinationStream;
        private readonly BlockSize blockSize;


        public BrightenedBlockStream(Stream sourceStream, BlockSize blockSize)
        {
            this.sourceStream = sourceStream;
            this.blockSize = blockSize;
        }

        public long SourcePosition =>
            this.sourceStream.Position;

        public override bool CanRead =>
            this.sourceStream.CanRead;

        public override bool CanSeek =>
            this.sourceStream.CanSeek;

        public override bool CanWrite =>
            this.sourceStream.CanWrite;

        public long BlockLength =>
            (long)Math.Ceiling((double)this.sourceStream.Length / BlockSizeMap.BlockSize(this.blockSize));

        public override long Length =>
            this.BlockLength * BlockSizeMap.BlockSize(this.blockSize) * Services.BlockBrightenerService.TupleCount;

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
