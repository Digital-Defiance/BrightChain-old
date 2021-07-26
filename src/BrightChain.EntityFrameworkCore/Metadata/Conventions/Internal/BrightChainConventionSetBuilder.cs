// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace BrightChain.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class BrightChainConventionSetBuilder : ProviderConventionSetBuilder
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public BrightChainConventionSetBuilder(
            ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override ConventionSet CreateConventionSet()
        {
            var conventionSet = base.CreateConventionSet();

            conventionSet.ModelInitializedConventions.Add(new ContextContainerConvention(this.Dependencies));

            conventionSet.ModelFinalizingConventions.Add(new ETagPropertyConvention());

            var storeKeyConvention = new StoreKeyConvention(this.Dependencies);
            var discriminatorConvention = new BrightChainDiscriminatorConvention(this.Dependencies);
            var keyDiscoveryConvention = new BrightChainKeyDiscoveryConvention(this.Dependencies);
            conventionSet.EntityTypeAddedConventions.Add(storeKeyConvention);
            conventionSet.EntityTypeAddedConventions.Add(discriminatorConvention);
            this.ReplaceConvention(conventionSet.EntityTypeAddedConventions, (KeyDiscoveryConvention)keyDiscoveryConvention);

            this.ReplaceConvention(conventionSet.EntityTypeRemovedConventions, (DiscriminatorConvention)discriminatorConvention);

            conventionSet.EntityTypeBaseTypeChangedConventions.Add(storeKeyConvention);
            this.ReplaceConvention(conventionSet.EntityTypeBaseTypeChangedConventions, (DiscriminatorConvention)discriminatorConvention);
            this.ReplaceConvention(conventionSet.EntityTypeBaseTypeChangedConventions, (KeyDiscoveryConvention)keyDiscoveryConvention);

            conventionSet.EntityTypePrimaryKeyChangedConventions.Add(storeKeyConvention);

            conventionSet.KeyAddedConventions.Add(storeKeyConvention);

            conventionSet.KeyRemovedConventions.Add(storeKeyConvention);
            this.ReplaceConvention(conventionSet.KeyRemovedConventions, (KeyDiscoveryConvention)keyDiscoveryConvention);

            this.ReplaceConvention(conventionSet.ForeignKeyAddedConventions, (KeyDiscoveryConvention)keyDiscoveryConvention);

            conventionSet.ForeignKeyRemovedConventions.Add(discriminatorConvention);
            conventionSet.ForeignKeyRemovedConventions.Add(storeKeyConvention);
            this.ReplaceConvention(conventionSet.ForeignKeyRemovedConventions, (KeyDiscoveryConvention)keyDiscoveryConvention);

            this.ReplaceConvention(conventionSet.ForeignKeyPropertiesChangedConventions, (KeyDiscoveryConvention)keyDiscoveryConvention);

            this.ReplaceConvention(conventionSet.ForeignKeyUniquenessChangedConventions, (KeyDiscoveryConvention)keyDiscoveryConvention);

            conventionSet.ForeignKeyOwnershipChangedConventions.Add(discriminatorConvention);
            conventionSet.ForeignKeyOwnershipChangedConventions.Add(storeKeyConvention);
            this.ReplaceConvention(conventionSet.ForeignKeyOwnershipChangedConventions, (KeyDiscoveryConvention)keyDiscoveryConvention);

            conventionSet.EntityTypeAnnotationChangedConventions.Add(storeKeyConvention);
            conventionSet.EntityTypeAnnotationChangedConventions.Add(keyDiscoveryConvention);

            this.ReplaceConvention(conventionSet.PropertyAddedConventions, (KeyDiscoveryConvention)keyDiscoveryConvention);

            conventionSet.PropertyAnnotationChangedConventions.Add(storeKeyConvention);

            return conventionSet;
        }

        /// <summary>
        ///     <para>
        ///         Call this method to build a <see cref="ModelBuilder" /> for BrightChain outside of <see cref="DbContext.OnModelCreating" />.
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
                .AddEntityFrameworkBrightChain()
                .AddDbContext<DbContext>(
                    (p, o) =>
                        o.UseBrightChain("localhost", "_", "_")
                            .UseInternalServiceProvider(p))
                .BuildServiceProvider();

            return serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        }
    }
}
