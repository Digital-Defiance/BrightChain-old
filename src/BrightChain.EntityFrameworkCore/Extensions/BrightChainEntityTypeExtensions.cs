// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Metadata.Internal;
using BrightChain.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Metadata;

// ReSharper disable once CheckNamespace
namespace BrightChain.EntityFrameworkCore
{
    /// <summary>
    ///     Entity type extension methods for BrightChain metadata.
    /// </summary>
    public static class BrightChainEntityTypeExtensions
    {
        /// <summary>
        ///     Returns the name of the container to which the entity type is mapped.
        /// </summary>
        /// <param name="entityType"> The entity type to get the container name for. </param>
        /// <returns> The name of the container to which the entity type is mapped. </returns>
        public static string? GetContainer(this IReadOnlyEntityType entityType)
        {
            return entityType.BaseType != null
                           ? entityType.GetRootType().GetContainer()
                           : (string?)entityType[BrightChainAnnotationNames.ContainerName]
                           ?? GetDefaultContainer(entityType);
        }

        private static string? GetDefaultContainer(IReadOnlyEntityType entityType)
        {
            return entityType.IsOwned()
                           ? null
                           : entityType.Model.GetDefaultContainer()
                           ?? entityType.ShortName();
        }

        /// <summary>
        ///     Sets the name of the container to which the entity type is mapped.
        /// </summary>
        /// <param name="entityType"> The entity type to set the container name for. </param>
        /// <param name="name"> The name to set. </param>
        public static void SetContainer(this IMutableEntityType entityType, string? name)
        {
            entityType.SetOrRemoveAnnotation(
                           BrightChainAnnotationNames.ContainerName,
                           Check.NullButNotEmpty(name, nameof(name)));
        }

        /// <summary>
        ///     Sets the name of the container to which the entity type is mapped.
        /// </summary>
        /// <param name="entityType"> The entity type to set the container name for. </param>
        /// <param name="name"> The name to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetContainer(
            this IConventionEntityType entityType,
            string? name,
            bool fromDataAnnotation = false)
        {
            entityType.SetOrRemoveAnnotation(
                           BrightChainAnnotationNames.ContainerName,
                           Check.NullButNotEmpty(name, nameof(name)),
                           fromDataAnnotation);
        }

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the container to which the entity type is mapped.
        /// </summary>
        /// <param name="entityType"> The entity type to find configuration source for. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the container to which the entity type is mapped. </returns>
        public static ConfigurationSource? GetContainerConfigurationSource(this IConventionEntityType entityType)
        {
            return entityType.FindAnnotation(BrightChainAnnotationNames.ContainerName)
                           ?.GetConfigurationSource();
        }

        /// <summary>
        ///     Returns the name of the parent property to which the entity type is mapped.
        /// </summary>
        /// <param name="entityType"> The entity type to get the containing property name for. </param>
        /// <returns> The name of the parent property to which the entity type is mapped. </returns>
        public static string? GetContainingPropertyName(this IReadOnlyEntityType entityType)
        {
            return entityType[BrightChainAnnotationNames.PropertyName] as string
                           ?? GetDefaultContainingPropertyName(entityType);
        }

        private static string? GetDefaultContainingPropertyName(IReadOnlyEntityType entityType)
        {
            return entityType.FindOwnership() is IReadOnlyForeignKey ownership
                           ? ownership.PrincipalToDependent!.Name
                           : null;
        }

        /// <summary>
        ///     Sets the name of the parent property to which the entity type is mapped.
        /// </summary>
        /// <param name="entityType"> The entity type to set the containing property name for. </param>
        /// <param name="name"> The name to set. </param>
        public static void SetContainingPropertyName(this IMutableEntityType entityType, string? name)
        {
            entityType.SetOrRemoveAnnotation(
                           BrightChainAnnotationNames.PropertyName,
                           Check.NullButNotEmpty(name, nameof(name)));
        }

        /// <summary>
        ///     Sets the name of the parent property to which the entity type is mapped.
        /// </summary>
        /// <param name="entityType"> The entity type to set the containing property name for. </param>
        /// <param name="name"> The name to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetContainingPropertyName(
            this IConventionEntityType entityType,
            string? name,
            bool fromDataAnnotation = false)
        {
            entityType.SetOrRemoveAnnotation(
                           BrightChainAnnotationNames.PropertyName,
                           Check.NullButNotEmpty(name, nameof(name)),
                           fromDataAnnotation);
        }

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the parent property to which the entity type is mapped.
        /// </summary>
        /// <param name="entityType"> The entity type to find configuration source for. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the parent property to which the entity type is mapped. </returns>
        public static ConfigurationSource? GetContainingPropertyNameConfigurationSource(this IConventionEntityType entityType)
        {
            return entityType.FindAnnotation(BrightChainAnnotationNames.PropertyName)
                           ?.GetConfigurationSource();
        }

        /// <summary>
        ///     Returns the name of the property that is used to store the partition key.
        /// </summary>
        /// <param name="entityType"> The entity type to get the partition key property name for. </param>
        /// <returns> The name of the partition key property. </returns>
        public static string? GetPartitionKeyPropertyName(this IReadOnlyEntityType entityType)
        {
            return entityType[BrightChainAnnotationNames.PartitionKeyName] as string;
        }

        /// <summary>
        ///     Sets the name of the property that is used to store the partition key key.
        /// </summary>
        /// <param name="entityType"> The entity type to set the partition key property name for. </param>
        /// <param name="name"> The name to set. </param>
        public static void SetPartitionKeyPropertyName(this IMutableEntityType entityType, string? name)
        {
            entityType.SetOrRemoveAnnotation(
                           BrightChainAnnotationNames.PartitionKeyName,
                           Check.NullButNotEmpty(name, nameof(name)));
        }

        /// <summary>
        ///     Sets the name of the property that is used to store the partition key.
        /// </summary>
        /// <param name="entityType"> The entity type to set the partition key property name for. </param>
        /// <param name="name"> The name to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetPartitionKeyPropertyName(
            this IConventionEntityType entityType,
            string? name,
            bool fromDataAnnotation = false)
        {
            entityType.SetOrRemoveAnnotation(
                           BrightChainAnnotationNames.PartitionKeyName,
                           Check.NullButNotEmpty(name, nameof(name)),
                           fromDataAnnotation);
        }

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the property that is used to store the partition key.
        /// </summary>
        /// <param name="entityType"> The entity type to find configuration source for. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the partition key property. </returns>
        public static ConfigurationSource? GetPartitionKeyPropertyNameConfigurationSource(this IConventionEntityType entityType)
        {
            return entityType.FindAnnotation(BrightChainAnnotationNames.PartitionKeyName)
                           ?.GetConfigurationSource();
        }

        /// <summary>
        ///     Returns the name of the property that is used to store the ETag.
        /// </summary>
        /// <param name="entityType"> The entity type to get the etag property name for. </param>
        /// <returns> The name of the etag property. </returns>
        public static string? GetETagPropertyName(this IReadOnlyEntityType entityType)
        {
            return entityType[BrightChainAnnotationNames.ETagName] as string;
        }

        /// <summary>
        ///     Sets the name of the property that is used to store the ETag key.
        /// </summary>
        /// <param name="entityType"> The entity type to set the etag property name for. </param>
        /// <param name="name"> The name to set. </param>
        public static void SetETagPropertyName(this IMutableEntityType entityType, string? name)
        {
            entityType.SetOrRemoveAnnotation(
                           BrightChainAnnotationNames.ETagName,
                           Check.NullButNotEmpty(name, nameof(name)));
        }

        /// <summary>
        ///     Sets the name of the property that is used to store the ETag.
        /// </summary>
        /// <param name="entityType"> The entity type to set the ETag property name for. </param>
        /// <param name="name"> The name to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetETagPropertyName(
            this IConventionEntityType entityType,
            string? name,
            bool fromDataAnnotation = false)
        {
            entityType.SetOrRemoveAnnotation(
                           BrightChainAnnotationNames.ETagName,
                           Check.NullButNotEmpty(name, nameof(name)),
                           fromDataAnnotation);
        }

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the property that is used to store the etag.
        /// </summary>
        /// <param name="entityType"> The entity type to find configuration source for. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the etag property. </returns>
        public static ConfigurationSource? GetETagPropertyNameConfigurationSource(this IConventionEntityType entityType)
        {
            return entityType.FindAnnotation(BrightChainAnnotationNames.ETagName)
                           ?.GetConfigurationSource();
        }

        /// <summary>
        ///     Gets the property on this entity that is mapped to brightChain ETag, if it exists.
        /// </summary>
        /// <param name="entityType"> The entity type to get the ETag property for. </param>
        /// <returns> The property mapped to ETag, or <see langword="null" /> if no property is mapped to ETag. </returns>
        public static IReadOnlyProperty? GetETagProperty(this IReadOnlyEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));
            var etagPropertyName = entityType.GetETagPropertyName();
            return !string.IsNullOrEmpty(etagPropertyName) ? entityType.FindProperty(etagPropertyName) : null;
        }

        /// <summary>
        ///     Gets the property on this entity that is mapped to brightChain ETag, if it exists.
        /// </summary>
        /// <param name="entityType"> The entity type to get the ETag property for. </param>
        /// <returns> The property mapped to etag, or <see langword="null" /> if no property is mapped to ETag. </returns>
        public static IProperty? GetETagProperty(this IEntityType entityType)
        {
            return (IProperty?)((IReadOnlyEntityType)entityType).GetETagProperty();
        }
    }
}
