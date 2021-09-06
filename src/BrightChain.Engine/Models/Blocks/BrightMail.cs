namespace BrightChain.Engine.Models.Blocks
{
    using BrightChain.Engine.Models.Entities;

    public class BrightMail : IDisposable
    {
        private readonly Guid Id;
        private readonly IEnumerable<string> Headers;
        private readonly Agent Sender;
        private readonly Agent Recipient;
        private readonly string Subject;
        private readonly string Body;
        private readonly IEnumerable<BrightenedBlock> Attachments;
        private bool recipientBcc;
        private readonly DateTime Sent;
        private readonly DateTime? Read;
        private readonly bool Deleted;

        private bool _disposedValue;

        private BrightMail()
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
