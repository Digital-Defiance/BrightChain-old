// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using BrightChain.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

// ReSharper disable once CheckNamespace
namespace BrightChain.EntityFrameworkCore
{
    /// <summary>
    ///     Property extension methods for BrightChain metadata.
    /// </summary>
    public static class BrightChainPropertyExtensions
    {
        /// <summary>
        ///     Returns the property name that the property is mapped to when targeting BrightChain.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> Returns the property name that the property is mapped to when targeting BrightChain. </returns>
        public static string GetJsonPropertyName(this IReadOnlyProperty property)
        {
            return (string?)property[BrightChainAnnotationNames.PropertyName]
                           ?? GetDefaultJsonPropertyName(property);
        }

        private static string GetDefaultJsonPropertyName(IReadOnlyProperty property)
        {
            var entityType = property.DeclaringEntityType;
            var ownership = entityType.FindOwnership();

            if (ownership != null
                && !entityType.IsDocumentRoot())
            {
                var pk = property.FindContainingPrimaryKey();
                if (pk != null
                    && (property.ClrType == typeof(int) || ownership.Properties.Contains(property))
                    && pk.Properties.Count == ownership.Properties.Count + (ownership.IsUnique ? 0 : 1)
                    && ownership.Properties.All(fkProperty => pk.Properties.Contains(fkProperty)))
                {
                    return "";
                }
            }

            return property.Name;
        }

        /// <summary>
        ///     Sets the property name that the property is mapped to when targeting BrightChain.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="name"> The name to set. </param>
        public static void SetJsonPropertyName(this IMutableProperty property, string? name)
        {
            property.SetOrRemoveAnnotation(
                           BrightChainAnnotationNames.PropertyName,
                           name);
        }

        /// <summary>
        ///     Sets the property name that the property is mapped to when targeting BrightChain.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="name"> The name to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string? SetJsonPropertyName(
            this IConventionProperty property,
            string? name,
            bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(
                BrightChainAnnotationNames.PropertyName,
                name,
                fromDataAnnotation);

            return name;
        }

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> the property name that the property is mapped to when targeting BrightChain.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns>
        ///     The <see cref="ConfigurationSource" /> the property name that the property is mapped to when targeting BrightChain.
        /// </returns>
        public static ConfigurationSource? GetJsonPropertyNameConfigurationSource(this IConventionProperty property)
        {
            return property.FindAnnotation(BrightChainAnnotationNames.PropertyName)?.GetConfigurationSource();
        }
    }
}
