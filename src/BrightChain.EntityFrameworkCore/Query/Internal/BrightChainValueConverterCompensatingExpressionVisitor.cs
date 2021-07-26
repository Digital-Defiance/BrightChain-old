// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using BrightChain.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Query;

namespace BrightChain.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class BrightChainValueConverterCompensatingExpressionVisitor : ExpressionVisitor
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public BrightChainValueConverterCompensatingExpressionVisitor(
            ISqlExpressionFactory sqlExpressionFactory)
        {
            this._sqlExpressionFactory = sqlExpressionFactory;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            return extensionExpression switch
            {
                ShapedQueryExpression shapedQueryExpression => this.VisitShapedQueryExpression(shapedQueryExpression),
                ReadItemExpression readItemExpression => readItemExpression,
                SelectExpression selectExpression => this.VisitSelect(selectExpression),
                SqlConditionalExpression sqlConditionalExpression => this.VisitSqlConditional(sqlConditionalExpression),
                _ => base.VisitExtension(extensionExpression),
            };
        }

        private Expression VisitShapedQueryExpression(ShapedQueryExpression shapedQueryExpression)
        {
            return shapedQueryExpression.Update(
                this.Visit(shapedQueryExpression.QueryExpression), shapedQueryExpression.ShaperExpression);
        }

        private Expression VisitSelect(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            var changed = false;

            var projections = new List<ProjectionExpression>();
            foreach (var item in selectExpression.Projection)
            {
                var updatedProjection = (ProjectionExpression)this.Visit(item);
                projections.Add(updatedProjection);
                changed |= updatedProjection != item;
            }

            var fromExpression = (RootReferenceExpression)this.Visit(selectExpression.FromExpression);
            changed |= fromExpression != selectExpression.FromExpression;

            var predicate = this.TryCompensateForBoolWithValueConverter((SqlExpression)this.Visit(selectExpression.Predicate));
            changed |= predicate != selectExpression.Predicate;

            var orderings = new List<OrderingExpression>();
            foreach (var ordering in selectExpression.Orderings)
            {
                var orderingExpression = (SqlExpression)this.Visit(ordering.Expression);
                changed |= orderingExpression != ordering.Expression;
                orderings.Add(ordering.Update(orderingExpression));
            }

            var limit = (SqlExpression)this.Visit(selectExpression.Limit);
            var offset = (SqlExpression)this.Visit(selectExpression.Offset);

            return changed
                ? selectExpression.Update(projections, fromExpression, predicate, orderings, limit, offset)
                : selectExpression;
        }

        private Expression VisitSqlConditional(SqlConditionalExpression sqlConditionalExpression)
        {
            Check.NotNull(sqlConditionalExpression, nameof(sqlConditionalExpression));

            var test = this.TryCompensateForBoolWithValueConverter((SqlExpression)this.Visit(sqlConditionalExpression.Test));
            var ifTrue = (SqlExpression)this.Visit(sqlConditionalExpression.IfTrue);
            var ifFalse = (SqlExpression)this.Visit(sqlConditionalExpression.IfFalse);

            return sqlConditionalExpression.Update(test, ifTrue, ifFalse);
        }

        private SqlExpression TryCompensateForBoolWithValueConverter(SqlExpression sqlExpression)
        {
            if (sqlExpression is KeyAccessExpression keyAccessExpression
                && keyAccessExpression.TypeMapping!.ClrType == typeof(bool)
                && keyAccessExpression.TypeMapping!.Converter != null)
            {
                return this._sqlExpressionFactory.Equal(
                    sqlExpression,
                    this._sqlExpressionFactory.Constant(true, sqlExpression.TypeMapping));
            }

            if (sqlExpression is SqlUnaryExpression sqlUnaryExpression)
            {
                return sqlUnaryExpression.Update(
                    this.TryCompensateForBoolWithValueConverter(sqlUnaryExpression.Operand));
            }

            if (sqlExpression is SqlBinaryExpression sqlBinaryExpression
                && (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso
                    || sqlBinaryExpression.OperatorType == ExpressionType.OrElse))
            {
                return sqlBinaryExpression.Update(
                    this.TryCompensateForBoolWithValueConverter(sqlBinaryExpression.Left),
                    this.TryCompensateForBoolWithValueConverter(sqlBinaryExpression.Right));
            }

            return sqlExpression;
        }
    }
}
