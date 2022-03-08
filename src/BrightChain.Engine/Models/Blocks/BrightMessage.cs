using System;
using BrightChain.Engine.Models.Entities;
using BrightChain.Engine.Models.Hashes;

namespace BrightChain.Engine.Models.Blocks;

public record BrightMessage : IDisposable
{
    protected readonly BlockHash BlockHash;
    protected readonly bool Deleted;
    protected readonly Guid Id;
    protected readonly DateTime? Read;
    protected readonly Agent Recipient;
    protected readonly Agent Sender;
    protected readonly DateTime Sent;
    protected readonly Guid Thread;
    protected bool _disposedValue;

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~BrightMail()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

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
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            this._disposedValue = true;
        }
    }
}
