namespace BrightChain.Engine.Models.Blocks.DataObjects
{
    using System;
    using Microsoft.Toolkit.HighPerformance.Buffers;

    public class StoredBlockData : BlockData, IDisposable
    {
        private readonly MemoryOwner<byte> StoredBytes;
        private bool _disposedValue;

        public override ReadOnlyMemory<byte> Bytes
        {
            get
                => new ReadOnlyMemory<byte>(this.StoredBytes.Span.ToArray());
        }

        public StoredBlockData(ReadOnlyMemory<byte> data)
        {
            this.StoredBytes = MemoryOwner<byte>.Allocate(size: data.Length, mode: AllocationMode.Default);
            data.Span.CopyTo(destination: this.StoredBytes.Span);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    this.StoredBytes.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
