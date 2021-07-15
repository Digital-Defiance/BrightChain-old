// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Infrastructure;
using BrightChain.EntityFrameworkCore.Infrastructure.Internal;
using BrightChain.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;

// ReSharper disable once CheckNamespace
namespace BrightChain.EntityFrameworkCore
{
    /// <summary>
    ///     BrightChain-specific extension methods for <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    public static class BrightChainDbContextOptionsExtensions
    {
        /// <summary>
        ///     Configures the context to connect to an Azure BrightChain database.
        /// </summary>
        /// <typeparam name="TContext"> The type of context to be configured. </typeparam>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="accountEndpoint"> The account end-point to connect to. </param>
        /// <param name="accountKey"> The account key. </param>
        /// <param name="databaseName"> The database name. </param>
        /// <param name="brightChainOptionsAction"> An optional action to allow additional BrightChain-specific configuration. </param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder<TContext> UseBrightChain<TContext>(
            this DbContextOptionsBuilder<TContext> optionsBuilder,
            string accountEndpoint,
            string accountKey,
            string databaseName,
            Action<BrightChainDbContextOptionsBuilder>? brightChainOptionsAction = null)
            where TContext : DbContext
        {
            return (DbContextOptionsBuilder<TContext>)UseBrightChain(
                           (DbContextOptionsBuilder)optionsBuilder,
                           accountEndpoint,
                           accountKey,
                           databaseName,
                           brightChainOptionsAction);
        }

        /// <summary>
        ///     Configures the context to connect to a Azure BrightChain database.
        /// </summary>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="accountEndpoint"> The account end-point to connect to. </param>
        /// <param name="accountKey"> The account key. </param>
        /// <param name="databaseName"> The database name. </param>
        /// <param name="brightChainOptionsAction"> An optional action to allow additional BrightChain-specific configuration. </param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder UseBrightChain(
            this DbContextOptionsBuilder optionsBuilder,
            string accountEndpoint,
            string accountKey,
            string databaseName,
            Action<BrightChainDbContextOptionsBuilder>? brightChainOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotNull(accountEndpoint, nameof(accountEndpoint));
            Check.NotEmpty(accountKey, nameof(accountKey));
            Check.NotEmpty(databaseName, nameof(databaseName));

            var extension = optionsBuilder.Options.FindExtension<BrightChainOptionsExtension>()
                ?? new BrightChainOptionsExtension();

            extension = extension
                .WithAccountEndpoint(accountEndpoint)
                .WithAccountKey(accountKey)
                .WithDatabaseName(databaseName);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            brightChainOptionsAction?.Invoke(new BrightChainDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        /// <summary>
        ///     Configures the context to connect to an Azure BrightChain database.
        /// </summary>
        /// <typeparam name="TContext"> The type of context to be configured. </typeparam>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="connectionString"> The connection string of the database to connect to. </param>
        /// <param name="databaseName"> The database name. </param>
        /// <param name="brightChainOptionsAction"> An optional action to allow additional BrightChain-specific configuration. </param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder<TContext> UseBrightChain<TContext>(
            this DbContextOptionsBuilder<TContext> optionsBuilder,
            string connectionString,
            string databaseName,
            Action<BrightChainDbContextOptionsBuilder>? brightChainOptionsAction = null)
            where TContext : DbContext
        {
            return (DbContextOptionsBuilder<TContext>)UseBrightChain(
                           (DbContextOptionsBuilder)optionsBuilder,
                           connectionString,
                           databaseName,
                           brightChainOptionsAction);
        }

        /// <summary>
        ///     Configures the context to connect to a Azure BrightChain database.
        /// </summary>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="connectionString"> The connection string of the database to connect to. </param>
        /// <param name="databaseName"> The database name. </param>
        /// <param name="brightChainOptionsAction"> An optional action to allow additional BrightChain-specific configuration. </param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder UseBrightChain(
            this DbContextOptionsBuilder optionsBuilder,
            string connectionString,
            string databaseName,
            Action<BrightChainDbContextOptionsBuilder>? brightChainOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotNull(connectionString, nameof(connectionString));
            Check.NotNull(databaseName, nameof(databaseName));

            var extension = optionsBuilder.Options.FindExtension<BrightChainOptionsExtension>()
                ?? new BrightChainOptionsExtension();

            extension = extension
                .WithConnectionString(connectionString)
                .WithDatabaseName(databaseName);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            brightChainOptionsAction?.Invoke(new BrightChainDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }
    }
}
