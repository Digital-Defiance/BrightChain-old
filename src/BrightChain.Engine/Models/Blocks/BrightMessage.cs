namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using BrightChain.Engine.Models.Entities;
    using BrightChain.Engine.Models.Hashes;

    public record BrightMessage : IDisposable
    {
        protected readonly Guid Id;
        protected readonly Agent Sender;
        protected readonly Agent Recipient;
        protected readonly BlockHash BlockHash;
        protected readonly DateTime Sent;
        protected readonly DateTime? Read;
        protected readonly bool Deleted;
        protected readonly Guid Thread;
        protected bool _disposedValue;

        protected BrightMessage()
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~BrightMail()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
