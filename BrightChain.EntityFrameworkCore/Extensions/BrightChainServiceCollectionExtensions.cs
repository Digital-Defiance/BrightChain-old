// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Diagnostics.Internal;
using BrightChain.EntityFrameworkCore.Helpers;
using BrightChain.EntityFrameworkCore.Infrastructure.Internal;
using BrightChain.EntityFrameworkCore.Metadata.Conventions;
using BrightChain.EntityFrameworkCore.Query.Internal;
using BrightChain.EntityFrameworkCore.Storage.Internal;
using BrightChain.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace BrightChain.Extensions.DependencyInjection
{
    /// <summary>
    ///     In-memory specific extension methods for <see cref="IServiceCollection" />.
    /// </summary>
    public static class BrightChainServiceCollectionExtensions
    {
        /// <summary>
        ///     <para>
        ///         Adds the services required by the in-memory database provider for Entity Framework
        ///         to an <see cref="IServiceCollection" />.
        ///     </para>
        ///     <para>
        ///         Calling this method is no longer necessary when building most applications, including those that
        ///         use dependency injection in ASP.NET or elsewhere.
        ///         It is only needed when building the internal service provider for use with
        ///         the <see cref="DbContextOptionsBuilder.UseInternalServiceProvider" /> method.
        ///         This is not recommend other than for some advanced scenarios.
        ///     </para>
        /// </summary>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddEntityFrameworkBrightChainDatabase(this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            var builder = new EntityFrameworkServicesBuilder(serviceCollection)
                .TryAdd<LoggingDefinitions, BrightChainLoggingDefinitions>()
                .TryAdd<IDatabaseProvider, DatabaseProvider<BrightChainOptionsExtension>>()
                .TryAdd<IValueGeneratorSelector, BrightChainValueGeneratorSelector>()
                .TryAdd<IDatabase>(p => p.GetRequiredService<IBrightChainDatabase>())
                .TryAdd<IDbContextTransactionManager, BrightChainTransactionManager>()
                .TryAdd<IDatabaseCreator, BrightChainDatabaseCreator>()
                .TryAdd<IQueryContextFactory, BrightChainQueryContextFactory>()
                .TryAdd<IProviderConventionSetBuilder, BrightChainConventionSetBuilder>()
                .TryAdd<IModelValidator, BrightChainModelValidator>()
                .TryAdd<ITypeMappingSource, BrightChainTypeMappingSource>()
                .TryAdd<IShapedQueryCompilingExpressionVisitorFactory, BrightChainShapedQueryCompilingExpressionVisitorFactory>()
                .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, BrightChainQueryableMethodTranslatingExpressionVisitorFactory>()
                .TryAdd<ISingletonOptions, IBrightChainSingletonOptions>(p => p.GetRequiredService<IBrightChainSingletonOptions>())
                .TryAddProviderSpecificServices(
                    b => b
                        .TryAddSingleton<IBrightChainSingletonOptions, BrightChainSingletonOptions>()
                        .TryAddSingleton<IBrightChainStoreCache, BrightChainStoreCache>()
                        .TryAddSingleton<IBrightChainTableFactory, BrightChainTableFactory>()
                        .TryAddScoped<IBrightChainDatabase, BrightChainDatabase>());

            builder.TryAddCoreServices();

            return serviceCollection;
        }
    }
}