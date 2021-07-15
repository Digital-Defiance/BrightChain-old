// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Metadata.Internal;
using BrightChain.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// ReSharper disable once CheckNamespace
namespace BrightChain.EntityFrameworkCore
{
    /// <summary>
    ///     BrightChain-specific extension methods for <see cref="ModelBuilder" />.
    /// </summary>
    public static class BrightChainModelBuilderExtensions
    {
        /// <summary>
        ///     Configures the default container name that will be used if no name
        ///     is explicitly configured for an entity type.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The default container name. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ModelBuilder HasDefaultContainer(
            this ModelBuilder modelBuilder,
            string? name)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            modelBuilder.Model.SetDefaultContainer(name);

            return modelBuilder;
        }

        /// <summary>
        ///     Configures the default container name that will be used if no name
        ///     is explicitly configured for an entity type.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The default container name. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionModelBuilder? HasDefaultContainer(
            this IConventionModelBuilder modelBuilder,
            string? name,
            bool fromDataAnnotation = false)
        {
            if (!modelBuilder.CanSetDefaultContainer(name, fromDataAnnotation))
            {
                return null;
            }

            modelBuilder.Metadata.SetDefaultContainer(name, fromDataAnnotation);

            return modelBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the given container name can be set as default.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The default container name. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given container name can be set as default. </returns>
        public static bool CanSetDefaultContainer(
            this IConventionModelBuilder modelBuilder,
            string? name,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            return modelBuilder.CanSetAnnotation(BrightChainAnnotationNames.ContainerName, name, fromDataAnnotation);
        }
    }
}
