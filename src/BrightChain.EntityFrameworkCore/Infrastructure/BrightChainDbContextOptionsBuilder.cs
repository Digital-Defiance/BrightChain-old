// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Net;
using BrightChain.EntityFrameworkCore.Infrastructure.Internal;
using BrightChain.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace BrightChain.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Allows BrightChain specific configuration to be performed on <see cref="DbContextOptions" />.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from a call to
    ///         <see cref="M:BrightChainDbContextOptionsExtensions.UseBrightChain{TContext}" />
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public class BrightChainDbContextOptionsBuilder : IBrightChainDbContextOptionsBuilderInfrastructure
    {
        private readonly DbContextOptionsBuilder _optionsBuilder;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BrightChainDbContextOptionsBuilder" /> class.
        /// </summary>
        /// <param name="optionsBuilder"> The options builder. </param>
        public BrightChainDbContextOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            _optionsBuilder = optionsBuilder;
        }

        /// <inheritdoc />
        public DbContextOptionsBuilder OptionsBuilder
            => _optionsBuilder;

        /// <summary>
        ///     Configures the context to use the provided <see cref="IExecutionStrategy" />.
        /// </summary>
        /// <param name="getExecutionStrategy"> A function that returns a new instance of an execution strategy. </param>
        public virtual BrightChainDbContextOptionsBuilder ExecutionStrategy(
            Func<ExecutionStrategyDependencies, IExecutionStrategy> getExecutionStrategy)
        {
            return WithOption(e => e.WithExecutionStrategyFactory(Check.NotNull(getExecutionStrategy, nameof(getExecutionStrategy))));
        }

        /// <summary>
        ///     Configures the context to use the provided geo-replicated region.
        /// </summary>
        /// <param name="region"> Azure BrightChain DB region name. </param>
        public virtual BrightChainDbContextOptionsBuilder Region(string region)
        {
            return WithOption(e => e.WithRegion(Check.NotNull(region, nameof(region))));
        }

        /// <summary>
        ///     Limits the operations to the provided endpoint.
        /// </summary>
        /// <param name="enable"> <see langword="true" /> to limit the operations to the provided endpoint. </param>
        public virtual BrightChainDbContextOptionsBuilder LimitToEndpoint(bool enable = true)
        {
            return WithOption(e => e.WithLimitToEndpoint(Check.NotNull(enable, nameof(enable))));
        }

        /// <summary>
        ///     Configures the proxy information used for web requests.
        /// </summary>
        /// <param name="proxy"> The proxy information used for web requests. </param>
        public virtual BrightChainDbContextOptionsBuilder WebProxy(IWebProxy proxy)
        {
            return WithOption(e => e.WithWebProxy(Check.NotNull(proxy, nameof(proxy))));
        }

        /// <summary>
        ///     Configures the timeout when connecting to the Azure BrightChain DB service.
        ///     The number specifies the time to wait for response to come back from network peer.
        /// </summary>
        /// <param name="timeout"> Request timeout. </param>
        public virtual BrightChainDbContextOptionsBuilder RequestTimeout(TimeSpan timeout)
        {
            return WithOption(e => e.WithRequestTimeout(Check.NotNull(timeout, nameof(timeout))));
        }

        /// <summary>
        ///     Configures the amount of time allowed for trying to establish a connection.
        /// </summary>
        /// <param name="timeout"> Open TCP connection timeout. </param>
        public virtual BrightChainDbContextOptionsBuilder OpenTcpConnectionTimeout(TimeSpan timeout)
        {
            return WithOption(e => e.WithOpenTcpConnectionTimeout(Check.NotNull(timeout, nameof(timeout))));
        }

        /// <summary>
        ///     Configures the amount of idle time after which unused connections are closed.
        /// </summary>
        /// <param name="timeout"> Idle connection timeout. </param>
        public virtual BrightChainDbContextOptionsBuilder IdleTcpConnectionTimeout(TimeSpan timeout)
        {
            return WithOption(e => e.WithIdleTcpConnectionTimeout(Check.NotNull(timeout, nameof(timeout))));
        }

        /// <summary>
        ///     Configures the maximum number of concurrent connections allowed for the target service endpoint
        ///     in the Azure BrightChain DB service.
        /// </summary>
        /// <param name="connectionLimit"> The maximum number of concurrent connections allowed. </param>
        public virtual BrightChainDbContextOptionsBuilder GatewayModeMaxConnectionLimit(int connectionLimit)
        {
            return WithOption(e => e.WithGatewayModeMaxConnectionLimit(Check.NotNull(connectionLimit, nameof(connectionLimit))));
        }

        /// <summary>
        ///     Configures the maximum number of TCP connections that may be opened to each BrightChain DB back-end.
        ///     Together with MaxRequestsPerTcpConnection, this setting limits the number of requests that are
        ///     simultaneously sent to a single BrightChain DB back-end (MaxRequestsPerTcpConnection x MaxTcpConnectionPerEndpoint).
        /// </summary>
        /// <param name="connectionLimit"> The maximum number of TCP connections that may be opened to each BrightChain DB back-end. </param>
        public virtual BrightChainDbContextOptionsBuilder MaxTcpConnectionsPerEndpoint(int connectionLimit)
        {
            return WithOption(e => e.WithMaxTcpConnectionsPerEndpoint(Check.NotNull(connectionLimit, nameof(connectionLimit))));
        }

        /// <summary>
        ///     Configures the number of requests allowed simultaneously over a single TCP connection.
        ///     When more requests are in flight simultaneously, the direct/TCP client will open additional connections.
        /// </summary>
        /// <param name="requestLimit"> The number of requests allowed simultaneously over a single TCP connection. </param>
        public virtual BrightChainDbContextOptionsBuilder MaxRequestsPerTcpConnection(int requestLimit)
        {
            return WithOption(e => e.WithMaxRequestsPerTcpConnection(Check.NotNull(requestLimit, nameof(requestLimit))));
        }

        /// <summary>
        /// Sets the boolean to only return the headers and status code in the BrightChain DB response for write item operation
        /// like Create, Upsert, Patch and Replace. Setting the option to false will cause the response to have a null resource.
        /// This reduces networking and CPU load by not sending the resource back over the network and serializing it on the client.
        /// </summary>
        /// <param name="enabled"><see langword="false" /> to have null resource</param>
        public virtual BrightChainDbContextOptionsBuilder ContentResponseOnWriteEnabled(bool enabled = true)
        {
            return WithOption(e => e.ContentResponseOnWriteEnabled(Check.NotNull(enabled, nameof(enabled))));
        }

        /// <summary>
        ///     Sets an option by cloning the extension used to store the settings. This ensures the builder
        ///     does not modify options that are already in use elsewhere.
        /// </summary>
        /// <param name="setAction"> An action to set the option. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        protected virtual BrightChainDbContextOptionsBuilder WithOption(Func<BrightChainOptionsExtension, BrightChainOptionsExtension> setAction)
        {
            ((IDbContextOptionsBuilderInfrastructure)_optionsBuilder).AddOrUpdateExtension(
                setAction(_optionsBuilder.Options.FindExtension<BrightChainOptionsExtension>() ?? new BrightChainOptionsExtension()));

            return this;
        }

        #region Hidden System.Object members

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string? ToString()
        {
            return base.ToString();
        }

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
