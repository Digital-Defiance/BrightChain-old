using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrightChain.Engine.Client
{
    public class BrightChainClient
    {
        public BrightChainClient(string connectionString, BrightChainClientOptions options)
        {
            this.ConnectionString = connectionString;
            this.Options = options;
        }

        public BrightChainClient(string endpoint, string key, BrightChainClientOptions options)
        {
            this.Endpoint = endpoint;
            this.Key = key;
            this.Options = options;
        }

        public string Endpoint { get; }
        public string Key { get; }
        public BrightChainClientOptions Options { get; }
        public string ConnectionString { get; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public object GetDatabase(string databaseId)
        {
            throw new NotImplementedException();
        }

        public Task CreateDatabaseIfNotExistsAsync(string databaseId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
