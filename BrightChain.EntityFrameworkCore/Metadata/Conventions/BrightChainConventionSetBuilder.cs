// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Extensions;
using BrightChain.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BrightChain.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     <para>
    ///         A builder for building conventions for th in-memory provider.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" /> and multiple registrations
    ///         are allowed. This means that each <see cref="DbContext" /> instance will use its own
    ///         set of instances of this service.
    ///         The implementations may depend on other services registered with any lifetime.
    ///         The implementations do not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class BrightChainConventionSetBuilder : ProviderConventionSetBuilder
    {
        /// <summary>
        ///     Creates a new <see cref="BrightChainConventionSetBuilder" /> instance.
        /// </summary>
        /// <param name="dependencies"> The core dependencies for this service. </param>
        public BrightChainConventionSetBuilder(
            ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <inheritdoc />
        public override ConventionSet CreateConventionSet()
        {
            var conventionSet = base.CreateConventionSet();

            conventionSet.ModelFinalizingConventions.Add(new DefiningQueryRewritingConvention(this.Dependencies));

            return conventionSet;
        }

        /// <summary>
        ///     <para>
        ///         Call this method to build a <see cref="ConventionSet" /> for the in-memory provider when using
        ///         the <see cref="ModelBuilder" /> outside of <see cref="DbContext.OnModelCreating" />.
        ///     </para>
        ///     <para>
        ///         Note that it is unusual to use this method.
        ///         Consider using <see cref="DbContext" /> in the normal way instead.
        ///     </para>
        /// </summary>
        /// <returns> The convention set. </returns>
        public static ConventionSet Build()
        {
            using var serviceScope = CreateServiceScope();
            using var context = serviceScope.ServiceProvider.GetRequiredService<DbContext>();
            return ConventionSet.CreateConventionSet(context);
        }

        /// <summary>
        ///     <para>
        ///         Call this method to build a <see cref="ModelBuilder" /> for SQLite outside of <see cref="DbContext.OnModelCreating" />.
        ///     </para>
        ///     <para>
        ///         Note that it is unusual to use this method.
        ///         Consider using <see cref="DbContext" /> in the normal way instead.
        ///     </para>
        /// </summary>
        /// <returns> The convention set. </returns>
        public static ModelBuilder CreateModelBuilder()
        {
            using var serviceScope = CreateServiceScope();
            using var context = serviceScope.ServiceProvider.GetRequiredService<DbContext>();
            return new ModelBuilder(ConventionSet.CreateConventionSet(context), context.GetService<ModelDependencies>());
        }

        private static IServiceScope CreateServiceScope()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkBrightChainDatabase()
                .AddDbContext<DbContext>(
                    (p, o) =>
                        o.UseBrightChainDatabase(Guid.NewGuid().ToString())
                            .UseInternalServiceProvider(p))
                .BuildServiceProvider();

            return serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        }
    }
}