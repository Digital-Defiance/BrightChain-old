// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using BrightChain.EntityFrameworkCore.Metadata.Conventions;
using BrightChain.EntityFrameworkCore.Properties;
using BrightChain.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

#nullable disable

namespace BrightChain.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class EntityProjectionExpression : Expression, IPrintableExpression, IAccessExpression
    {
        private readonly Dictionary<IProperty, IAccessExpression> _propertyExpressionsMap = new();
        private readonly Dictionary<INavigation, IAccessExpression> _navigationExpressionsMap = new();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public EntityProjectionExpression(IEntityType entityType, Expression accessExpression)
        {
            this.EntityType = entityType;
            this.AccessExpression = accessExpression;
            this.Name = (accessExpression as IAccessExpression)?.Name;
        }

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
        public override Type Type
            => this.EntityType.ClrType;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression AccessExpression { get; }

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
        public virtual string Name { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return this.Update(visitor.Visit(this.AccessExpression));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression Update(Expression accessExpression)
        {
            return accessExpression != this.AccessExpression
                           ? new EntityProjectionExpression(this.EntityType, accessExpression)
                           : this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression BindProperty(IProperty property, bool clientEval)
        {
            if (!this.EntityType.IsAssignableFrom(property.DeclaringEntityType)
                && !property.DeclaringEntityType.IsAssignableFrom(this.EntityType))
            {
                throw new InvalidOperationException(
                    BrightChainStrings.UnableToBindMemberToEntityProjection("property", property.Name, this.EntityType.DisplayName()));
            }

            if (!this._propertyExpressionsMap.TryGetValue(property, out var expression))
            {
                expression = new KeyAccessExpression(property, this.AccessExpression);
                this._propertyExpressionsMap[property] = expression;
            }

            if (!clientEval
                // TODO: Remove once __jObject is translated to the access root in a better fashion and
                // would not otherwise be found to be non-translatable. See issues #17670 and #14121.
                && property.Name != StoreKeyConvention.JObjectPropertyName
                && expression.Name.Length == 0)
            {
                // Non-persisted property can't be translated
                return null;
            }

            return (Expression)expression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression BindNavigation(INavigation navigation, bool clientEval)
        {
            if (!this.EntityType.IsAssignableFrom(navigation.DeclaringEntityType)
                && !navigation.DeclaringEntityType.IsAssignableFrom(this.EntityType))
            {
                throw new InvalidOperationException(
                    BrightChainStrings.UnableToBindMemberToEntityProjection("navigation", navigation.Name, this.EntityType.DisplayName()));
            }

            if (!this._navigationExpressionsMap.TryGetValue(navigation, out var expression))
            {
                expression = navigation.IsCollection
                    ? new ObjectArrayProjectionExpression(navigation, this.AccessExpression)
                    : new EntityProjectionExpression(
                        navigation.TargetEntityType,
                        new ObjectAccessExpression(navigation, this.AccessExpression));

                this._navigationExpressionsMap[navigation] = expression;
            }

            if (!clientEval
                && expression.Name.Length == 0)
            {
                // Non-persisted navigation can't be translated
                return null;
            }

            return (Expression)expression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression BindMember(
            string name,
            Type entityType,
            bool clientEval,
            out IPropertyBase propertyBase)
        {
            return this.BindMember(MemberIdentity.Create(name), entityType, clientEval, out propertyBase);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression BindMember(
            MemberInfo memberInfo,
            Type entityType,
            bool clientEval,
            out IPropertyBase propertyBase)
        {
            return this.BindMember(MemberIdentity.Create(memberInfo), entityType, clientEval, out propertyBase);
        }

        private Expression BindMember(MemberIdentity member, Type entityClrType, bool clientEval, out IPropertyBase propertyBase)
        {
            var entityType = this.EntityType;
            if (entityClrType != null
                && !entityClrType.IsAssignableFrom(entityType.ClrType))
            {
                entityType = entityType.GetDerivedTypes().First(e => entityClrType.IsAssignableFrom(e.ClrType));
            }

            var property = member.MemberInfo == null
                ? entityType.FindProperty(member.Name)
                : entityType.FindProperty(member.MemberInfo);
            if (property != null)
            {
                propertyBase = property;
                return this.BindProperty(property, clientEval);
            }

            var navigation = member.MemberInfo == null
                ? entityType.FindNavigation(member.Name)
                : entityType.FindNavigation(member.MemberInfo);
            if (navigation != null)
            {
                propertyBase = navigation;
                return this.BindNavigation(navigation, clientEval);
            }

            // Entity member not found
            propertyBase = null;
            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityProjectionExpression UpdateEntityType(IEntityType derivedType)
        {
            Check.NotNull(derivedType, nameof(derivedType));

            if (!derivedType.GetAllBaseTypes().Contains(this.EntityType))
            {
                throw new InvalidOperationException(
                    BrightChainStrings.InvalidDerivedTypeInEntityProjection(
                        derivedType.DisplayName(), this.EntityType.DisplayName()));
            }

            return new EntityProjectionExpression(derivedType, this.AccessExpression);
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

            expressionPrinter.Visit(this.AccessExpression);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj != null
                           && (ReferenceEquals(this, obj)
                               || obj is EntityProjectionExpression entityProjectionExpression
                               && this.Equals(entityProjectionExpression));
        }

        private bool Equals(EntityProjectionExpression entityProjectionExpression)
        {
            return Equals(this.EntityType, entityProjectionExpression.EntityType)
                           && this.AccessExpression.Equals(entityProjectionExpression.AccessExpression);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(this.EntityType, this.AccessExpression);
        }
    }
}
