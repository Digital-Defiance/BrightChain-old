// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Diagnostics.Internal;
using BrightChain.EntityFrameworkCore.Infrastructure.Internal;
using BrightChain.EntityFrameworkCore.Metadata.Conventions.Internal;
using BrightChain.EntityFrameworkCore.Query.Internal;
using BrightChain.EntityFrameworkCore.Storage.Internal;
using BrightChain.EntityFrameworkCore.Utilities;
using BrightChain.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;
using IMemberTranslatorProvider = BrightChain.EntityFrameworkCore.Query.Internal.IMemberTranslatorProvider;
using IMethodCallTranslatorProvider = BrightChain.EntityFrameworkCore.Query.Internal.IMethodCallTranslatorProvider;
using IQuerySqlGeneratorFactory = BrightChain.EntityFrameworkCore.Query.Internal.IQuerySqlGeneratorFactory;
using ISqlExpressionFactory = BrightChain.EntityFrameworkCore.Query.Internal.ISqlExpressionFactory;
using SqlExpressionFactory = BrightChain.EntityFrameworkCore.Query.Internal.SqlExpressionFactory;

// ReSharper disable once CheckNamespace
namespace BrightChain.Extensions.DependencyInjection
{
    /// <summary>
    ///     BrightChain-specific extension methods for <see cref="IServiceCollection" />.
    /// </summary>
    public static class BrightChainServiceCollectionExtensions
    {
        /// <summary>
        ///     <para>
        ///         Adds the services required by the Azure BrightChain database provider for Entity Framework
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
        public static IServiceCollection AddEntityFrameworkBrightChain(this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            var builder = new EntityFrameworkServicesBuilder(serviceCollection)
                .TryAdd<LoggingDefinitions, BrightChainLoggingDefinitions>()
                .TryAdd<IDatabaseProvider, DatabaseProvider<BrightChainOptionsExtension>>()
                .TryAdd<IDatabase, BrightChainDatabaseWrapper>()
                .TryAdd<IExecutionStrategyFactory, BrightChainExecutionStrategyFactory>()
                .TryAdd<IDbContextTransactionManager, BrightChainTransactionManager>()
                .TryAdd<IModelValidator, BrightChainModelValidator>()
                .TryAdd<IProviderConventionSetBuilder, BrightChainConventionSetBuilder>()
                .TryAdd<IValueGeneratorSelector, BrightChainValueGeneratorSelector>()
                .TryAdd<IDatabaseCreator, BrightChainDatabaseCreator>()
                .TryAdd<IQueryContextFactory, BrightChainQueryContextFactory>()
                .TryAdd<ITypeMappingSource, BrightChainTypeMappingSource>()

                // New Query pipeline
                .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, BrightChainQueryableMethodTranslatingExpressionVisitorFactory>()
                .TryAdd<IShapedQueryCompilingExpressionVisitorFactory, BrightChainShapedQueryCompilingExpressionVisitorFactory>()
                .TryAdd<ISingletonOptions, IBrightChainSingletonOptions>(p => p.GetRequiredService<IBrightChainSingletonOptions>())
                .TryAdd<IQueryTranslationPreprocessorFactory, BrightChainQueryTranslationPreprocessorFactory>()
                .TryAdd<IQueryCompilationContextFactory, BrightChainQueryCompilationContextFactory>()
                .TryAdd<IQueryTranslationPostprocessorFactory, BrightChainQueryTranslationPostprocessorFactory>()
                .TryAddProviderSpecificServices(
                    b => b
                        .TryAddSingleton<IBrightChainSingletonOptions, BrightChainSingletonOptions>()
                        .TryAddSingleton<ISingletonBrightChainClientWrapper, SingletonMlictnRtaisClientWrapper>()
                        .TryAddSingleton<ISqlExpressionFactory, SqlExpressionFactory>()
                        .TryAddSingleton<IQuerySqlGeneratorFactory, QuerySqlGeneratorFactory>()
                        .TryAddSingleton<IMethodCallTranslatorProvider, BrightChainMethodCallTranslatorProvider>()
                        .TryAddSingleton<IMemberTranslatorProvider, BrightChainMemberTranslatorProvider>()
                        .TryAddScoped<IBrightChainClientWrapper, BrightChainClientWrapper>()
                );

            builder.TryAddCoreServices();

            return serviceCollection;
        }
    }
}
