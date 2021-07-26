// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using BrightChain.EntityFrameworkCore.Properties;
using BrightChain.EntityFrameworkCore.Utilities;
using BrightChain.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

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
            this._accountEndpoint = copyFrom._accountEndpoint;
            this._accountKey = copyFrom._accountKey;
            this._databaseName = copyFrom._databaseName;
            this._connectionString = copyFrom._connectionString;
            this._region = copyFrom._region;
            this._limitToEndpoint = copyFrom._limitToEndpoint;
            this._executionStrategyFactory = copyFrom._executionStrategyFactory;
            this._webProxy = copyFrom._webProxy;
            this._requestTimeout = copyFrom._requestTimeout;
            this._openTcpConnectionTimeout = copyFrom._openTcpConnectionTimeout;
            this._idleTcpConnectionTimeout = copyFrom._idleTcpConnectionTimeout;
            this._gatewayModeMaxConnectionLimit = copyFrom._gatewayModeMaxConnectionLimit;
            this._maxTcpConnectionsPerEndpoint = copyFrom._maxTcpConnectionsPerEndpoint;
            this._maxRequestsPerTcpConnection = copyFrom._maxRequestsPerTcpConnection;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual DbContextOptionsExtensionInfo Info
            => this._info ??= new ExtensionInfo(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string? AccountEndpoint
            => this._accountEndpoint;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithAccountEndpoint(string? accountEndpoint)
        {
            if (this._connectionString != null)
            {
                throw new InvalidOperationException(BrightChainStrings.ConnectionStringConflictingConfiguration);
            }

            var clone = this.Clone();

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
            => this._accountKey;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithAccountKey(string? accountKey)
        {
            if (accountKey is not null && this._connectionString is not null)
            {
                throw new InvalidOperationException(BrightChainStrings.ConnectionStringConflictingConfiguration);
            }

            var clone = this.Clone();

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
            => this._connectionString;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithConnectionString(string? connectionString)
        {
            if (connectionString is not null && (this._accountEndpoint != null || this._accountKey != null))
            {
                throw new InvalidOperationException(BrightChainStrings.ConnectionStringConflictingConfiguration);
            }

            var clone = this.Clone();

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
            => this._databaseName!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithDatabaseName(string database)
        {
            var clone = this.Clone();

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
            => this._region;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithRegion(string? region)
        {
            var clone = this.Clone();

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
            => this._limitToEndpoint;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithLimitToEndpoint(bool enable)
        {
            var clone = this.Clone();

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
            => this._webProxy;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithWebProxy(IWebProxy? proxy)
        {
            var clone = this.Clone();

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
            => this._requestTimeout;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithRequestTimeout(TimeSpan? timeout)
        {
            var clone = this.Clone();

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
            => this._openTcpConnectionTimeout;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithOpenTcpConnectionTimeout(TimeSpan? timeout)
        {
            var clone = this.Clone();

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
            => this._idleTcpConnectionTimeout;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithIdleTcpConnectionTimeout(TimeSpan? timeout)
        {
            var clone = this.Clone();

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
            => this._gatewayModeMaxConnectionLimit;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithGatewayModeMaxConnectionLimit(int? connectionLimit)
        {
            var clone = this.Clone();

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
            => this._maxTcpConnectionsPerEndpoint;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithMaxTcpConnectionsPerEndpoint(int? connectionLimit)
        {
            var clone = this.Clone();

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
            => this._maxRequestsPerTcpConnection;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension WithMaxRequestsPerTcpConnection(int? requestLimit)
        {
            var clone = this.Clone();

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
            => this._enableContentResponseOnWrite;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainOptionsExtension ContentResponseOnWriteEnabled(bool enabled)
        {
            var clone = this.Clone();

            clone._enableContentResponseOnWrite = enabled;

            return clone;
        }

        /// <summary>
        ///     A factory for creating the default <see cref="IExecutionStrategy" />, or <see langword="null" /> if none has been
        ///     configured.
        /// </summary>
        public virtual Func<ExecutionStrategyDependencies, IExecutionStrategy>? ExecutionStrategyFactory
            => this._executionStrategyFactory;

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="executionStrategyFactory"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual BrightChainOptionsExtension WithExecutionStrategyFactory(
            Func<ExecutionStrategyDependencies, IExecutionStrategy>? executionStrategyFactory)
        {
            var clone = this.Clone();

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
                if (this._serviceProviderHash == null)
                {
                    long hashCode;
                    if (!string.IsNullOrEmpty(this.Extension._connectionString))
                    {
                        hashCode = this.Extension._connectionString.GetHashCode();
                    }
                    else
                    {
                        hashCode = (this.Extension._accountEndpoint?.GetHashCode() ?? 0);
                        hashCode = (hashCode * 397) ^ (this.Extension._accountKey?.GetHashCode() ?? 0);
                    }

                    hashCode = (hashCode * 397) ^ (this.Extension._region?.GetHashCode() ?? 0);
                    //hashCode = (hashCode * 3) ^ (Extension._connectionMode?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 3) ^ (this.Extension._limitToEndpoint?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 397) ^ (this.Extension._webProxy?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 397) ^ (this.Extension._requestTimeout?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 397) ^ (this.Extension._openTcpConnectionTimeout?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 397) ^ (this.Extension._idleTcpConnectionTimeout?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 131) ^ (this.Extension._gatewayModeMaxConnectionLimit?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 397) ^ (this.Extension._maxTcpConnectionsPerEndpoint?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 131) ^ (this.Extension._maxRequestsPerTcpConnection?.GetHashCode() ?? 0);
                    this._serviceProviderHash = hashCode;
                }

                return this._serviceProviderHash.Value;
            }

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            {
                Check.NotNull(debugInfo, nameof(debugInfo));

                if (!string.IsNullOrEmpty(this.Extension._connectionString))
                {
                    debugInfo["BrightChain:" + nameof(ConnectionString)] =
                        this.Extension._connectionString.GetHashCode().ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    debugInfo["BrightChain:" + nameof(AccountEndpoint)] =
                        (this.Extension._accountEndpoint?.GetHashCode() ?? 0L).ToString(CultureInfo.InvariantCulture);
                    debugInfo["BrightChain:" + nameof(AccountKey)] = (this.Extension._accountKey?.GetHashCode() ?? 0L).ToString(CultureInfo.InvariantCulture);
                }

                debugInfo["BrightChain:" + nameof(BrightChainDbContextOptionsBuilder.Region)] =
                    (this.Extension._region?.GetHashCode() ?? 0).ToString(CultureInfo.InvariantCulture);
            }

            public override string LogFragment
            {
                get
                {
                    if (this._logFragment == null)
                    {
                        var builder = new StringBuilder();

                        builder.Append("ServiceEndPoint=").Append(this.Extension._accountEndpoint).Append(' ');

                        builder.Append("Database=").Append(this.Extension._databaseName).Append(' ');

                        this._logFragment = builder.ToString();
                    }

                    return this._logFragment;
                }
            }
        }
    }
}
