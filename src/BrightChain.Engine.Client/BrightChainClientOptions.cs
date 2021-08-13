namespace BrightChain.Engine.Client
{
    using System.Net;

    //
    // Summary:
    //     Defines all the configurable options that the CosmosClient requires.
    public class BrightChainClientOptions
    {
        //
        // Summary:
        //     Creates a new BrightChainClientOptions
        public BrightChainClientOptions() { }

        //
        // Summary:
        //     Allows optimistic batching of requests to service. Setting this option might
        //     impact the latency of the operations. Hence this option is recommended for non-latency
        //     sensitive scenarios only.
        public bool AllowBulkExecution { get; set; }
        //
        // Summary:
        //     Limits the operations to the provided endpoint on the CosmosClient.
        //
        // Value:
        //     Default value is false.
        //
        // Remarks:
        //     When the value of this property is false, the SDK will automatically discover
        //     write and read regions, and use them when the configured application region is
        //     not available. When set to true, availability is limited to the endpoint specified
        //     on the CosmosClient constructor. Defining the Microsoft.Azure.Cosmos.CosmosClientOptions.ApplicationRegion
        //     or Microsoft.Azure.Cosmos.CosmosClientOptions.ApplicationPreferredRegions is
        //     not allowed when setting the value to true.
        public bool LimitToEndpoint { get; set; }
        //
        // Summary:
        //     Get to set an optional JSON serializer. The client will use it to serialize or
        //     de-serialize user's cosmos request/responses. SDK owned types such as DatabaseProperties
        //     and ContainerProperties will always use the SDK default serializer.
        //[JsonConverter(typeof(ClientOptionJsonConverter))]
        //public IBrightChainSerializer Serializer { get; set; }

        //
        // Summary:
        //     Get to set optional serializer options.
        public BrightChainSerializationOptions SerializerOptions { get; set; }
        //
        // Summary:
        //     (Gateway/Https) Get or set the proxy information used for web requests.
        public IWebProxy WebProxy { get; set; }
        public TimeSpan? OpenTcpConnectionTimeout { get; set; }
        //
        // Summary:
        //     (Direct/TCP) Controls the amount of idle time after which unused connections
        //     are closed.
        //
        // Value:
        //     By default, idle connections are kept open indefinitely. Value must be greater
        //     than or equal to 10 minutes. Recommended values are between 20 minutes and 24
        //     hours.
        //
        // Remarks:
        //     Mainly useful for sparse infrequent access to a large database account.
        public TimeSpan? IdleTcpConnectionTimeout { get; set; }
        //
        // Summary:
        //     Gets or sets the flag to enable address cache refresh on TCP connection reset
        //     notification.
        //
        // Value:
        //     The default value is false
        //
        // Remarks:
        //     Does not apply if Microsoft.Azure.Cosmos.ConnectionMode.Gateway is used.
        public bool EnableTcpConnectionEndpointRediscovery { get; set; }
        //
        // Summary:
        //     Gets or sets the boolean to only return the headers and status code in the Cosmos
        //     DB response for write item operation like Create, Upsert, Patch and Replace.
        //     Setting the option to false will cause the response to have a null resource.
        //     This reduces networking and CPU load by not sending the resource back over the
        //     network and serializing it on the client.
        //
        // Remarks:
        //     This is optimal for workloads where the returned resource is not used.
        //     This option can be overriden by similar property in ItemRequestOptions and TransactionalBatchItemRequestOptions
        public bool? EnableContentResponseOnWrite { get; set; }
        //
        // Summary:
        //     Gets or sets the maximum number of retries in the case where the request fails
        //     because the Azure Cosmos DB service has applied rate limiting on the client.
        //
        // Value:
        //     The default value is 9. This means in the case where the request is rate limited,
        //     the same request will be issued for a maximum of 10 times to the server before
        //     an error is returned to the application. If the value of this property is set
        //     to 0, there will be no automatic retry on rate limiting requests from the client
        //     and the exception needs to be handled at the application level.
        //
        // Remarks:
        //     When a client is sending requests faster than the allowed rate, the service will
        //     return HttpStatusCode 429 (Too Many Requests) to rate limit the client. The current
        //     implementation in the SDK will then wait for the amount of time the service tells
        //     it to wait and retry after the time has elapsed.
        //     For more information, see Handle rate limiting/request rate too large.
        public int? MaxRetryAttemptsOnRateLimitedRequests { get; set; }
        //
        // Summary:
        //     Gets the handlers run before the process
        //[JsonConverter(typeof(ClientOptionJsonConverter))]
        //public Collection<RequestHandler> CustomHandlers { get; }
        //
        // Summary:
        //     The SDK does a background refresh based on the time interval set to refresh the
        //     token credentials. This avoids latency issues because the old token is used until
        //     the new token is retrieved.
        //
        // Remarks:
        //     The recommended minimum value is 5 minutes. The default value is 50% of the token
        //     expire time.
        public TimeSpan? TokenCredentialBackgroundRefreshInterval { get; set; }
        //
        // Summary:
        //     Gets the request timeout in seconds when connecting to the Azure Cosmos DB service.
        //     The number specifies the time to wait for response to come back from network
        //     peer.
        //
        // Value:
        //     Default value is 1 minute.
        public TimeSpan RequestTimeout { get; set; }
        //
        // Summary:
        //     Get or set the maximum number of concurrent connections allowed for the target
        //     service endpoint in the Azure Cosmos DB service.
        //
        // Value:
        //     Default value is 50.
        //
        // Remarks:
        //     This setting is only applicable in Gateway mode.
        public int GatewayModeMaxConnectionLimit { get; set; }
        //
        // Summary:
        //     Gets and sets the preferred regions for geo-replicated database accounts in the
        //     Azure Cosmos DB service.
        //
        // Remarks:
        //     When this property is specified, the SDK will use the region list in the provided
        //     order to define the endpoint failover order. This configuration is an alternative
        //     to Microsoft.Azure.Cosmos.CosmosClientOptions.ApplicationRegion, either one can
        //     be set but not both.
        public IReadOnlyList<string> ApplicationPreferredRegions { get; set; }
        //
        // Summary:
        //     Get or set the preferred geo-replicated region to be used for Azure Cosmos DB
        //     service interaction.
        //
        // Remarks:
        //     When this property is specified, the SDK prefers the region to perform operations.
        //     Also SDK auto-selects fallback geo-replicated regions for high availability.
        //     When this property is not specified, the SDK uses the write region as the preferred
        //     region for all operations.
        public string ApplicationRegion { get; set; }
        //
        // Summary:
        //     Get or set user-agent suffix to include with every Azure Cosmos DB service interaction.
        //
        // Remarks:
        //     Setting this property after sending any request won't have any effect.
        public string ApplicationName { get; set; }
        //
        // Summary:
        //     Gets or sets the maximum retry time in seconds for the Azure Cosmos DB service.
        //
        // Value:
        //     The default value is 30 seconds.
        //
        // Remarks:
        //     The minimum interval is seconds. Any interval that is smaller will be ignored.
        //     When a request fails due to a rate limiting error, the service sends back a response
        //     that contains a value indicating the client should not retry before the Microsoft.Azure.Cosmos.CosmosException.RetryAfter
        //     time period has elapsed. This property allows the application to set a maximum
        //     wait time for all retry attempts. If the cumulative wait time exceeds the this
        //     value, the client will stop retrying and return the error to the application.
        //     For more information, see Handle rate limiting/request rate too large.
        public TimeSpan? MaxRetryWaitTimeOnRateLimitedRequests { get; set; }
        //
        // Summary:
        //     Gets or sets a delegate to use to obtain an HttpClient instance to be used for
        //     HTTPS communication.
        //
        // Remarks:
        //     HTTPS communication is used when Microsoft.Azure.Cosmos.CosmosClientOptions.ConnectionMode
        //     is set to Microsoft.Azure.Cosmos.ConnectionMode.Gateway for all operations and
        //     when Microsoft.Azure.Cosmos.CosmosClientOptions.ConnectionMode is Microsoft.Azure.Cosmos.ConnectionMode.Direct
        //     (default) for metadata operations.
        //     Useful in scenarios where the application is using a pool of HttpClient instances
        //     to be shared, like ASP.NET Core applications with IHttpClientFactory or Blazor
        //     WebAssembly applications.
        //     For .NET core applications the default GatewayConnectionLimit will be ignored.
        //     It must be set on the HttpClientHandler.MaxConnectionsPerServer to limit the
        //     number of connections
        public Func<HttpClient> HttpClientFactory { get; set; }
    }
}
