// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using BrightChain.EntityFrameworkCore.Properties;
using BrightChain.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;

#nullable disable warnings

namespace BrightChain.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SelectExpression : Expression
    {
        private const string RootAlias = "c";

        private IDictionary<ProjectionMember, Expression> _projectionMapping = new Dictionary<ProjectionMember, Expression>();
        private readonly List<ProjectionExpression> _projection = new();
        private readonly List<OrderingExpression> _orderings = new();

        private ValueConverter _partitionKeyValueConverter;
        private Expression _partitionKeyValue;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SelectExpression(IEntityType entityType)
        {
            this.Container = entityType.GetContainer();
            this.FromExpression = new RootReferenceExpression(entityType, RootAlias);
            this._projectionMapping[new ProjectionMember()] = new EntityProjectionExpression(entityType, this.FromExpression);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SelectExpression(
            List<ProjectionExpression> projections,
            RootReferenceExpression fromExpression,
            List<OrderingExpression> orderings)
        {
            this._projection = projections;
            this.FromExpression = fromExpression;
            this._orderings = orderings;
        }

        private SelectExpression(
            List<ProjectionExpression> projections,
            RootReferenceExpression fromExpression,
            List<OrderingExpression> orderings,
            string container)
            : this(projections, fromExpression, orderings)
        {
            this.Container = container;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string Container { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyList<ProjectionExpression> Projection
            => this._projection;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual RootReferenceExpression FromExpression { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyList<OrderingExpression> Orderings
            => this._orderings;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression Predicate { get; private set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression Limit { get; private set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression Offset { get; private set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsDistinct { get; private set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression GetMappedProjection(ProjectionMember projectionMember)
        {
            return this._projectionMapping[projectionMember];
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetPartitionKey(IProperty partitionKeyProperty, Expression expression)
        {
            this._partitionKeyValueConverter = partitionKeyProperty.GetTypeMapping().Converter;
            this._partitionKeyValue = expression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string GetPartitionKey(IReadOnlyDictionary<string, object> parameterValues)
        {
            switch (this._partitionKeyValue)
            {
                case ConstantExpression constantExpression:
                    return GetString(this._partitionKeyValueConverter, constantExpression.Value);

                case ParameterExpression parameterExpression
                    when parameterValues.TryGetValue(parameterExpression.Name, out var value):
                    return GetString(this._partitionKeyValueConverter, value);

                default:
                    return null;
            }

            static string GetString(ValueConverter converter, object value)
            {
                return converter is null
                                   ? (string)value
                                   : (string)converter.ConvertToProvider(value);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ApplyProjection()
        {
            if (this.Projection.Any())
            {
                return;
            }

            var result = new Dictionary<ProjectionMember, Expression>();
            foreach (var keyValuePair in this._projectionMapping)
            {
                result[keyValuePair.Key] = Constant(
                    this.AddToProjection(
                        keyValuePair.Value,
                        keyValuePair.Key.Last?.Name));
            }

            this._projectionMapping = result;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ReplaceProjectionMapping(IDictionary<ProjectionMember, Expression> projectionMapping)
        {
            this._projectionMapping.Clear();
            foreach (var kvp in projectionMapping)
            {
                this._projectionMapping[kvp.Key] = kvp.Value;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual int AddToProjection(SqlExpression sqlExpression)
        {
            return this.AddToProjection(sqlExpression, null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual int AddToProjection(EntityProjectionExpression entityProjection)
        {
            return this.AddToProjection(entityProjection, null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual int AddToProjection(ObjectArrayProjectionExpression objectArrayProjection)
        {
            return this.AddToProjection(objectArrayProjection, null);
        }

        private int AddToProjection(Expression expression, string alias)
        {
            var existingIndex = this._projection.FindIndex(pe => pe.Expression.Equals(expression));
            if (existingIndex != -1)
            {
                return existingIndex;
            }

            var baseAlias = alias
                ?? (expression as IAccessExpression)?.Name
                ?? "c";

            var currentAlias = baseAlias;
            var counter = 0;
            while (this._projection.Any(pe => string.Equals(pe.Alias, currentAlias, StringComparison.OrdinalIgnoreCase)))
            {
                currentAlias = $"{baseAlias}{counter++}";
            }

            this._projection.Add(new ProjectionExpression(expression, currentAlias));

            return this._projection.Count - 1;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ApplyDistinct()
        {
            this.IsDistinct = true;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ClearOrdering()
        {
            this._orderings.Clear();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ApplyPredicate(SqlExpression expression)
        {
            if (expression is SqlConstantExpression sqlConstant
                && sqlConstant.Value is bool boolValue
                && boolValue)
            {
                return;
            }

            this.Predicate = this.Predicate == null
                ? expression
                : new SqlBinaryExpression(
                    ExpressionType.AndAlso,
                    this.Predicate,
                    expression,
                    typeof(bool),
                    expression.TypeMapping);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ApplyLimit(SqlExpression sqlExpression)
        {
            if (this.Limit != null)
            {
                throw new InvalidOperationException("See issue#16156");
            }

            this.Limit = sqlExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ApplyOffset(SqlExpression sqlExpression)
        {
            if (this.Limit != null
                || this.Offset != null)
            {
                throw new InvalidOperationException("See issue#16156");
            }

            this.Offset = sqlExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ApplyOrdering(OrderingExpression orderingExpression)
        {
            if (this.IsDistinct
                || this.Limit != null
                || this.Offset != null)
            {
                throw new InvalidOperationException("See issue#16156");
            }

            this._orderings.Clear();
            this._orderings.Add(orderingExpression);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void AppendOrdering(OrderingExpression orderingExpression)
        {
            if (this._orderings.FirstOrDefault(o => o.Expression.Equals(orderingExpression.Expression)) == null)
            {
                this._orderings.Add(orderingExpression);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ReverseOrderings()
        {
            if (this.Limit != null
                || this.Offset != null)
            {
                throw new InvalidOperationException(BrightChainStrings.ReverseAfterSkipTakeNotSupported);
            }

            var existingOrderings = this._orderings.ToArray();

            this._orderings.Clear();

            foreach (var existingOrdering in existingOrderings)
            {
                this._orderings.Add(
                    new OrderingExpression(
                        existingOrdering.Expression,
                        !existingOrdering.IsAscending));
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Type Type
            => typeof(object);

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
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var changed = false;

            var projections = new List<ProjectionExpression>();
            IDictionary<ProjectionMember, Expression> projectionMapping;
            if (this.Projection.Any())
            {
                projectionMapping = this._projectionMapping;
                foreach (var item in this.Projection)
                {
                    var projection = (ProjectionExpression)visitor.Visit(item);
                    projections.Add(projection);

                    changed |= projection != item;
                }
            }
            else
            {
                projectionMapping = new Dictionary<ProjectionMember, Expression>();
                foreach (var mapping in this._projectionMapping)
                {
                    var newProjection = visitor.Visit(mapping.Value);
                    changed |= newProjection != mapping.Value;

                    projectionMapping[mapping.Key] = newProjection;
                }
            }

            var fromExpression = (RootReferenceExpression)visitor.Visit(this.FromExpression);
            changed |= fromExpression != this.FromExpression;

            var predicate = (SqlExpression)visitor.Visit(this.Predicate);
            changed |= predicate != this.Predicate;

            var orderings = new List<OrderingExpression>();
            foreach (var ordering in this._orderings)
            {
                var orderingExpression = (SqlExpression)visitor.Visit(ordering.Expression);
                changed |= orderingExpression != ordering.Expression;
                orderings.Add(ordering.Update(orderingExpression));
            }

            var offset = (SqlExpression)visitor.Visit(this.Offset);
            changed |= offset != this.Offset;

            var limit = (SqlExpression)visitor.Visit(this.Limit);
            changed |= limit != this.Limit;

            if (changed)
            {
                var newSelectExpression = new SelectExpression(projections, fromExpression, orderings)
                {
                    _projectionMapping = projectionMapping,
                    Predicate = predicate,
                    Offset = offset,
                    Limit = limit,
                    IsDistinct = IsDistinct
                };

                return newSelectExpression;
            }

            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SelectExpression Update(
            List<ProjectionExpression> projections,
            RootReferenceExpression fromExpression,
            SqlExpression? predicate,
            List<OrderingExpression>? orderings,
            SqlExpression? limit,
            SqlExpression? offset)
        {
            Check.NotNull(projections, nameof(projections));
            Check.NotNull(fromExpression, nameof(fromExpression));

            var projectionMapping = new Dictionary<ProjectionMember, Expression>();
            foreach (var kvp in this._projectionMapping)
            {
                projectionMapping[kvp.Key] = kvp.Value;
            }

            return new SelectExpression(projections, fromExpression, orderings, this.Container)
            {
                _projectionMapping = projectionMapping,
                Predicate = predicate,
                Offset = offset,
                Limit = limit,
                IsDistinct = IsDistinct,
            };
        }
    }
}
