// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Properties;
using BrightChain.EntityFrameworkCore.Utilities;
using BrightChain.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;

namespace BrightChain.EntityFrameworkCore.Infrastructure.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class BrightChainOptionsExtension : IDbContextOptionsExtension
    {
        private string? _accountEndpoint;
        private string? _accountKey;
        private string? _connectionString;
        private string? _databaseName;
        private string? _region;
        private bool? _limitToEndpoint;
        private Func<ExecutionStrategyDependencies, IExecutionStrategy>? _executionStrategyFactory;
        private IWebProxy? _webProxy;
        private TimeSpan? _requestTimeout;
        private TimeSpan? _openTcpConnectionTimeout;
        private TimeSpan? _idleTcpConnectionTimeout;
        private int? _gatewayModeMaxConnectionLimit;
        private int? _maxTcpConnectionsPerEndpoint;
        private int? _maxRequestsPerTcpConnection;
        private bool? _enableContentResponseOnWrite;
        private DbContextOptionsExtensionInfo? _info;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public BrightChainOptionsExtension()
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected BrightChainOptionsExtension(BrightChainOptionsExtension copyFrom)
        {
            _accountEndpoint = copyFrom._accountEndpoint;
            _accountKey = copyFrom._accountKey;
            _databaseName = copyFrom._databaseName;
            _connectionString = copyFrom._connectionString;
            _region = copyFrom._region;
            _limitToEndpoint = copyFrom._limitToEndpoint;
            _executionStrategyFactory = copyFrom._executionStrategyFactory;
            _webProxy = copyFrom._webProxy;
            _requestTimeout = copyFrom._requestTimeout;
            _openTcpConnectionTimeout = copyFrom._openTcpConnectionTimeout;
            _idleTcpConnectionTimeout = copyFrom._idleTcpConnectionTimeout;
            _gatewayModeMaxConnectionLimit = copyFrom._gatewayModeMaxConnectionLimit;
            _maxTcpConnectionsPerEndpoint = copyFrom._maxTcpConnectionsPerEndpoint;
            _maxRequestsPerTcpConnection = copyFrom._maxRequestsPerTcpConnection;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual DbContextOptionsExtensionInfo Info
            => _info ??= new ExtensionInfo(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string? AccountEndpoint
            => _accountEndpoint;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithAccountEndpoint(string? accountEndpoint)
        {
            if (_connectionString != null)
            {
                throw new InvalidOperationException(BrightChainStrings.ConnectionStringConflictingConfiguration);
            }

            var clone = Clone();

            clone._accountEndpoint = accountEndpoint;

            return clone;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string? AccountKey
            => _accountKey;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithAccountKey(string? accountKey)
        {
            if (accountKey is not null && _connectionString is not null)
            {
                throw new InvalidOperationException(BrightChainStrings.ConnectionStringConflictingConfiguration);
            }

            var clone = Clone();

            clone._accountKey = accountKey;

            return clone;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string? ConnectionString
            => _connectionString;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithConnectionString(string? connectionString)
        {
            if (connectionString is not null && (_accountEndpoint != null || _accountKey != null))
            {
                throw new InvalidOperationException(BrightChainStrings.ConnectionStringConflictingConfiguration);
            }

            var clone = Clone();

            clone._connectionString = connectionString;

            return clone;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string DatabaseName
            => _databaseName!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithDatabaseName(string database)
        {
            var clone = Clone();

            clone._databaseName = database;

            return clone;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string? Region
            => _region;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithRegion(string? region)
        {
            var clone = Clone();

            clone._region = region;

            return clone;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool? LimitToEndpoint
            => _limitToEndpoint;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithLimitToEndpoint(bool enable)
        {
            var clone = Clone();

            clone._limitToEndpoint = enable;

            return clone;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IWebProxy? WebProxy
            => _webProxy;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithWebProxy(IWebProxy? proxy)
        {
            var clone = Clone();

            clone._webProxy = proxy;

            return clone;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual TimeSpan? RequestTimeout
            => _requestTimeout;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithRequestTimeout(TimeSpan? timeout)
        {
            var clone = Clone();

            clone._requestTimeout = timeout;

            return clone;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual TimeSpan? OpenTcpConnectionTimeout
            => _openTcpConnectionTimeout;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithOpenTcpConnectionTimeout(TimeSpan? timeout)
        {
            var clone = Clone();

            clone._openTcpConnectionTimeout = timeout;

            return clone;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual TimeSpan? IdleTcpConnectionTimeout
            => _idleTcpConnectionTimeout;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithIdleTcpConnectionTimeout(TimeSpan? timeout)
        {
            var clone = Clone();

            clone._idleTcpConnectionTimeout = timeout;

            return clone;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual int? GatewayModeMaxConnectionLimit
            => _gatewayModeMaxConnectionLimit;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithGatewayModeMaxConnectionLimit(int? connectionLimit)
        {
            var clone = Clone();

            clone._gatewayModeMaxConnectionLimit = connectionLimit;

            return clone;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual int? MaxTcpConnectionsPerEndpoint
            => _maxTcpConnectionsPerEndpoint;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithMaxTcpConnectionsPerEndpoint(int? connectionLimit)
        {
            var clone = Clone();

            clone._maxTcpConnectionsPerEndpoint = connectionLimit;

            return clone;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual int? MaxRequestsPerTcpConnection
            => _maxRequestsPerTcpConnection;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithMaxRequestsPerTcpConnection(int? requestLimit)
        {
            var clone = Clone();

            clone._maxRequestsPerTcpConnection = requestLimit;

            return clone;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool? EnableContentResponseOnWrite
            => _enableContentResponseOnWrite;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension ContentResponseOnWriteEnabled(bool enabled)
        {
            var clone = Clone();

            clone._enableContentResponseOnWrite = enabled;

            return clone;
        }

        /// <summary>
        ///     A factory for creating the default <see cref="IExecutionStrategy" />, or <see langword="null" /> if none has been
        ///     configured.
        /// </summary>
        public virtual Func<ExecutionStrategyDependencies, IExecutionStrategy>? ExecutionStrategyFactory
            => _executionStrategyFactory;

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="executionStrategyFactory"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual BrightChainOptionsExtension WithExecutionStrategyFactory(
            Func<ExecutionStrategyDependencies, IExecutionStrategy>? executionStrategyFactory)
        {
            var clone = Clone();

            clone._executionStrategyFactory = executionStrategyFactory;

            return clone;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual BrightChainOptionsExtension Clone()
        {
            return new(this);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ApplyServices(IServiceCollection services)
        {
            services.AddEntityFrameworkBrightChain();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void Validate(IDbContextOptions options)
        {
        }

        private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
        {
            private string? _logFragment;
            private long? _serviceProviderHash;

            public ExtensionInfo(IDbContextOptionsExtension extension)
                : base(extension)
            {
            }

            private new BrightChainOptionsExtension Extension
                => (BrightChainOptionsExtension)base.Extension;

            public override bool IsDatabaseProvider
                => true;

            public override long GetServiceProviderHashCode()
            {
                if (_serviceProviderHash == null)
                {
                    long hashCode;
                    if (!string.IsNullOrEmpty(Extension._connectionString))
                    {
                        hashCode = Extension._connectionString.GetHashCode();
                    }
                    else
                    {
                        hashCode = (Extension._accountEndpoint?.GetHashCode() ?? 0);
                        hashCode = (hashCode * 397) ^ (Extension._accountKey?.GetHashCode() ?? 0);
                    }

                    hashCode = (hashCode * 397) ^ (Extension._region?.GetHashCode() ?? 0);
                    //hashCode = (hashCode * 3) ^ (Extension._connectionMode?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 3) ^ (Extension._limitToEndpoint?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 397) ^ (Extension._webProxy?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 397) ^ (Extension._requestTimeout?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 397) ^ (Extension._openTcpConnectionTimeout?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 397) ^ (Extension._idleTcpConnectionTimeout?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 131) ^ (Extension._gatewayModeMaxConnectionLimit?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 397) ^ (Extension._maxTcpConnectionsPerEndpoint?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 131) ^ (Extension._maxRequestsPerTcpConnection?.GetHashCode() ?? 0);
                    _serviceProviderHash = hashCode;
                }

                return _serviceProviderHash.Value;
            }

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            {
                Check.NotNull(debugInfo, nameof(debugInfo));

                if (!string.IsNullOrEmpty(Extension._connectionString))
                {
                    debugInfo["BrightChain:" + nameof(ConnectionString)] =
                        Extension._connectionString.GetHashCode().ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    debugInfo["BrightChain:" + nameof(AccountEndpoint)] =
                        (Extension._accountEndpoint?.GetHashCode() ?? 0L).ToString(CultureInfo.InvariantCulture);
                    debugInfo["BrightChain:" + nameof(AccountKey)] = (Extension._accountKey?.GetHashCode() ?? 0L).ToString(CultureInfo.InvariantCulture);
                }

                debugInfo["BrightChain:" + nameof(BrightChainDbContextOptionsBuilder.Region)] =
                    (Extension._region?.GetHashCode() ?? 0).ToString(CultureInfo.InvariantCulture);
            }

            public override string LogFragment
            {
                get
                {
                    if (_logFragment == null)
                    {
                        var builder = new StringBuilder();

                        builder.Append("ServiceEndPoint=").Append(Extension._accountEndpoint).Append(' ');

                        builder.Append("Database=").Append(Extension._databaseName).Append(' ');

                        _logFragment = builder.ToString();
                    }

                    return _logFragment;
                }
            }
        }
    }
}
