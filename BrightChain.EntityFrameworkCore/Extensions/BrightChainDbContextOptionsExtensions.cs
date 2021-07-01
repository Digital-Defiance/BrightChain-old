// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Diagnostics;
using BrightChain.EntityFrameworkCore.Helpers;
using BrightChain.EntityFrameworkCore.Infrastructure;
using BrightChain.EntityFrameworkCore.Infrastructure.Internal;
using BrightChain.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;

// ReSharper disable once CheckNamespace
namespace BrightChain.EntityFrameworkCore.Extensions
{
    /// <summary>
    ///     In-memory specific extension methods for <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    public static class BrightChainDbContextOptionsExtensions
    {
        /// <summary>
        ///     Configures the context to connect to an in-memory database.
        ///     The in-memory database is shared anywhere the same name is used, but only for a given
        ///     service provider. To use the same in-memory database across service providers, call
        ///     <see
        ///         cref="UseBrightChainDatabase{TContext}(DbContextOptionsBuilder{TContext},string,BrightChainDatabaseRoot,Action{BrightChainDbContextOptionsBuilder})" />
        ///     passing a shared <see cref="BrightChainDatabaseRoot" /> on which to root the database.
        /// </summary>
        /// <typeparam name="TContext"> The type of context being configured. </typeparam>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="databaseName">
        ///     The name of the in-memory database. This allows the scope of the in-memory database to be controlled
        ///     independently of the context. The in-memory database is shared anywhere the same name is used.
        /// </param>
        /// <param name="BrightChainOptionsAction">An optional action to allow additional in-memory specific configuration.</param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder<TContext> UseBrightChainDatabase<TContext>(
            this DbContextOptionsBuilder<TContext> optionsBuilder,
            string databaseName,
            Action<BrightChainDbContextOptionsBuilder>? BrightChainOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseBrightChainDatabase(
                (DbContextOptionsBuilder)optionsBuilder, databaseName, BrightChainOptionsAction);

        /// <summary>
        ///     Configures the context to connect to a named in-memory database.
        ///     The in-memory database is shared anywhere the same name is used, but only for a given
        ///     service provider. To use the same in-memory database across service providers, call
        ///     <see cref="UseBrightChainDatabase(DbContextOptionsBuilder,string,BrightChainDatabaseRoot,Action{BrightChainDbContextOptionsBuilder})" />
        ///     passing a shared <see cref="BrightChainDatabaseRoot" /> on which to root the database.
        /// </summary>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="databaseName">
        ///     The name of the in-memory database. This allows the scope of the in-memory database to be controlled
        ///     independently of the context. The in-memory database is shared anywhere the same name is used.
        /// </param>
        /// <param name="BrightChainOptionsAction">An optional action to allow additional in-memory specific configuration.</param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder UseBrightChainDatabase(
            this DbContextOptionsBuilder optionsBuilder,
            string databaseName,
            Action<BrightChainDbContextOptionsBuilder>? BrightChainOptionsAction = null)
            => UseBrightChainDatabase(optionsBuilder, databaseName, null, BrightChainOptionsAction);

        /// <summary>
        ///     Configures the context to connect to an in-memory database.
        ///     The in-memory database is shared anywhere the same name is used, but only for a given
        ///     service provider.
        /// </summary>
        /// <typeparam name="TContext"> The type of context being configured. </typeparam>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="databaseName">
        ///     The name of the in-memory database. This allows the scope of the in-memory database to be controlled
        ///     independently of the context. The in-memory database is shared anywhere the same name is used.
        /// </param>
        /// <param name="databaseRoot">
        ///     All in-memory databases will be rooted in this object, allowing the application
        ///     to control their lifetime. This is useful when sometimes the context instance
        ///     is created explicitly with <see langword="new" /> while at other times it is resolved using dependency injection.
        /// </param>
        /// <param name="BrightChainOptionsAction">An optional action to allow additional in-memory specific configuration.</param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder<TContext> UseBrightChainDatabase<TContext>(
            this DbContextOptionsBuilder<TContext> optionsBuilder,
            string databaseName,
            BrightChainDatabaseRoot? databaseRoot,
            Action<BrightChainDbContextOptionsBuilder>? BrightChainOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseBrightChainDatabase(
                (DbContextOptionsBuilder)optionsBuilder, databaseName, databaseRoot, BrightChainOptionsAction);

        /// <summary>
        ///     Configures the context to connect to a named in-memory database.
        ///     The in-memory database is shared anywhere the same name is used, but only for a given
        ///     service provider.
        /// </summary>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="databaseName">
        ///     The name of the in-memory database. This allows the scope of the in-memory database to be controlled
        ///     independently of the context. The in-memory database is shared anywhere the same name is used.
        /// </param>
        /// <param name="databaseRoot">
        ///     All in-memory databases will be rooted in this object, allowing the application
        ///     to control their lifetime. This is useful when sometimes the context instance
        ///     is created explicitly with <see langword="new" /> while at other times it is resolved using dependency injection.
        /// </param>
        /// <param name="BrightChainOptionsAction">An optional action to allow additional in-memory specific configuration.</param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder UseBrightChainDatabase(
            this DbContextOptionsBuilder optionsBuilder,
            string databaseName,
            BrightChainDatabaseRoot? databaseRoot,
            Action<BrightChainDbContextOptionsBuilder>? BrightChainOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotEmpty(databaseName, nameof(databaseName));

            var extension = optionsBuilder.Options.FindExtension<BrightChainOptionsExtension>()
                ?? new BrightChainOptionsExtension();

            extension = extension.WithStoreName(databaseName);

            if (databaseRoot != null)
            {
                extension = extension.WithDatabaseRoot(databaseRoot);
            }

            extension = extension.WithNullabilityCheckEnabled(true);

            ConfigureWarnings(optionsBuilder);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            BrightChainOptionsAction?.Invoke(new BrightChainDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        private static void ConfigureWarnings(DbContextOptionsBuilder optionsBuilder)
        {
            // Set warnings defaults
            var coreOptionsExtension
                = optionsBuilder.Options.FindExtension<CoreOptionsExtension>()
                ?? new CoreOptionsExtension();

            coreOptionsExtension = coreOptionsExtension.WithWarningsConfiguration(
                coreOptionsExtension.WarningsConfiguration.TryWithExplicit(
                    BrightChainEventId.TransactionIgnoredWarning, WarningBehavior.Throw));

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(coreOptionsExtension);
        }
    }
}