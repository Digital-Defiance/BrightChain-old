using System;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightChain.Engine.Models.Blocks.DataObjects;

public class StoredBlockData : BlockData, IDisposable
{
    private readonly MemoryOwner<byte> StoredBytes;
    private bool _disposedValue;

    public StoredBlockData(ReadOnlyMemory<byte> data)
    {
        this.StoredBytes = MemoryOwner<byte>.Allocate(size: data.Length,
            mode: AllocationMode.Default);
        data.Span.CopyTo(destination: this.StoredBytes.Span);
    }

    public override ReadOnlyMemory<byte> Bytes => new(array: this.StoredBytes.Span.ToArray());

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
        GC.SuppressFinalize(obj: this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this._disposedValue)
        {
            if (disposing)
            {
                this.StoredBytes.Dispose();
            }

            this._disposedValue = true;
        }
    }
}
