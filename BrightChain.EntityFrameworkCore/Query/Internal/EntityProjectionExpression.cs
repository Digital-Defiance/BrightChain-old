// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Helpers;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BrightChain.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class EntityProjectionExpression : Expression, IPrintableExpression
    {
        private readonly IReadOnlyDictionary<IProperty, MethodCallExpression> _readExpressionMap;
        private readonly Dictionary<INavigation, EntityShaperExpression> _navigationExpressionsCache = new();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public EntityProjectionExpression(
            IEntityType entityType,
            IReadOnlyDictionary<IProperty, MethodCallExpression> readExpressionMap)
        {
            this.EntityType = entityType;
            this._readExpressionMap = readExpressionMap;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEntityType EntityType { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Type Type
            => this.EntityType.ClrType;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public sealed override ExpressionType NodeType
            => ExpressionType.Extension;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityProjectionExpression UpdateEntityType(IEntityType derivedType)
        {
            if (!derivedType.GetAllBaseTypes().Contains(this.EntityType))
            {
                throw new InvalidOperationException(
                    String.Format("The specified entity type '{derivedType}' is not derived from '{entityType}'.", derivedType.DisplayName(), this.EntityType.DisplayName()));
            }

            var readExpressionMap = new Dictionary<IProperty, MethodCallExpression>();
            foreach (var kvp in this._readExpressionMap)
            {
                var property = kvp.Key;
                if (derivedType.IsAssignableFrom(property.DeclaringEntityType)
                    || property.DeclaringEntityType.IsAssignableFrom(derivedType))
                {
                    readExpressionMap[property] = kvp.Value;
                }
            }

            return new EntityProjectionExpression(derivedType, readExpressionMap);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual MethodCallExpression BindProperty(IProperty property)
        {
            if (!this.EntityType.IsAssignableFrom(property.DeclaringEntityType)
                && !property.DeclaringEntityType.IsAssignableFrom(this.EntityType))
            {
                throw new InvalidOperationException(
                    String.Format("Unable to bind '{memberType}' '{member}' to entity projection of '{entityType}'.", property.Name, this.EntityType.DisplayName()));
            }

            return this._readExpressionMap[property];
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void AddNavigationBinding(INavigation navigation, EntityShaperExpression entityShaper)
        {
            if (!this.EntityType.IsAssignableFrom(navigation.DeclaringEntityType)
                && !navigation.DeclaringEntityType.IsAssignableFrom(this.EntityType))
            {
                throw new InvalidOperationException(
                    String.Format(
                        "Unable to bind '{memberType}' '{member}' to entity projection of '{entityType}'.",
                        "navigation", navigation.Name, this.EntityType.DisplayName()));
            }

            this._navigationExpressionsCache[navigation] = entityShaper;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityShaperExpression? BindNavigation(INavigation navigation)
        {
            if (!this.EntityType.IsAssignableFrom(navigation.DeclaringEntityType)
                && !navigation.DeclaringEntityType.IsAssignableFrom(this.EntityType))
            {
                throw new InvalidOperationException(
                    String.Format(
                        "Unable to bind '{memberType}' '{member}' to entity projection of '{entityType}'.",
                        "navigation", navigation.Name, this.EntityType.DisplayName()));
            }

            return this._navigationExpressionsCache.TryGetValue(navigation, out var expression)
                ? expression
                : null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.AppendLine(nameof(EntityProjectionExpression) + ":");
            using (expressionPrinter.Indent())
            {
                foreach (var readExpressionMapEntry in this._readExpressionMap)
                {
                    expressionPrinter.Append(readExpressionMapEntry.Key + " -> ");
                    expressionPrinter.Visit(readExpressionMapEntry.Value);
                    expressionPrinter.AppendLine();
                }
            }
        }
    }
}