// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Client;
using BrightChain.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace BrightChain.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" /> and multiple registrations
    ///         are allowed. This means a single instance of each service is used by many <see cref="DbContext" />
    ///         instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class SingletonMlictnRtaisClientWrapper : ISingletonBrightChainClientWrapper
    {
        private static readonly string _userAgent = " BrightChain.EntityFrameworkCore/" + ProductInfo.GetVersion();
        private readonly BrightChainClientOptions _options;
        private readonly string? _endpoint;
        private readonly string? _key;
        private readonly string? _connectionString;
        private BrightChainClient? _client;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SingletonMlictnRtaisClientWrapper(IBrightChainSingletonOptions options)
        {
            _endpoint = options.AccountEndpoint;
            _key = options.AccountKey;
            _connectionString = options.ConnectionString;
            var configuration = new BrightChainClientOptions { ApplicationName = _userAgent };

            if (options.Region != null)
            {
                configuration.ApplicationRegion = options.Region;
            }

            if (options.LimitToEndpoint != null)
            {
                configuration.LimitToEndpoint = options.LimitToEndpoint.Value;
            }

            if (options.WebProxy != null)
            {
                configuration.WebProxy = options.WebProxy;
            }

            if (options.RequestTimeout != null)
            {
                configuration.RequestTimeout = options.RequestTimeout.Value;
            }

            if (options.OpenTcpConnectionTimeout != null)
            {
                configuration.OpenTcpConnectionTimeout = options.OpenTcpConnectionTimeout.Value;
            }

            if (options.IdleTcpConnectionTimeout != null)
            {
                configuration.IdleTcpConnectionTimeout = options.IdleTcpConnectionTimeout.Value;
            }

            if (options.GatewayModeMaxConnectionLimit != null)
            {
                configuration.GatewayModeMaxConnectionLimit = options.GatewayModeMaxConnectionLimit.Value;
            }

            _options = configuration;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainClient Client
            => _client ??= string.IsNullOrEmpty(_connectionString)
                ? new BrightChainClient(_endpoint, _key, _options)
                : new BrightChainClient(_connectionString, _options);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void Dispose()
        {
            _client?.Dispose();
            _client = null;
        }
    }
}
