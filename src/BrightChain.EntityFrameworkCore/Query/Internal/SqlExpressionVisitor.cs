// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
    public abstract class SqlExpressionVisitor : ExpressionVisitor
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            Check.NotNull(extensionExpression, nameof(extensionExpression));

            switch (extensionExpression)
            {
                case ShapedQueryExpression shapedQueryExpression:
                    return shapedQueryExpression.Update(
                        this.Visit(shapedQueryExpression.QueryExpression), shapedQueryExpression.ShaperExpression);

                case ReadItemExpression readItemExpression:
                    return readItemExpression;

                case SelectExpression selectExpression:
                    return this.VisitSelect(selectExpression);

                case ProjectionExpression projectionExpression:
                    return this.VisitProjection(projectionExpression);

                case EntityProjectionExpression entityProjectionExpression:
                    return this.VisitEntityProjection(entityProjectionExpression);

                case ObjectArrayProjectionExpression arrayProjectionExpression:
                    return this.VisitObjectArrayProjection(arrayProjectionExpression);

                case RootReferenceExpression rootReferenceExpression:
                    return this.VisitRootReference(rootReferenceExpression);

                case KeyAccessExpression keyAccessExpression:
                    return this.VisitKeyAccess(keyAccessExpression);

                case ObjectAccessExpression objectAccessExpression:
                    return this.VisitObjectAccess(objectAccessExpression);

                case SqlBinaryExpression sqlBinaryExpression:
                    return this.VisitSqlBinary(sqlBinaryExpression);

                case SqlConstantExpression sqlConstantExpression:
                    return this.VisitSqlConstant(sqlConstantExpression);

                case SqlUnaryExpression sqlUnaryExpression:
                    return this.VisitSqlUnary(sqlUnaryExpression);

                case SqlConditionalExpression sqlConditionalExpression:
                    return this.VisitSqlConditional(sqlConditionalExpression);

                case SqlParameterExpression sqlParameterExpression:
                    return this.VisitSqlParameter(sqlParameterExpression);

                case InExpression inExpression:
                    return this.VisitIn(inExpression);

                case SqlFunctionExpression sqlFunctionExpression:
                    return this.VisitSqlFunction(sqlFunctionExpression);

                case OrderingExpression orderingExpression:
                    return this.VisitOrdering(orderingExpression);
            }

            return base.VisitExtension(extensionExpression);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected abstract Expression VisitOrdering(OrderingExpression orderingExpression);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected abstract Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected abstract Expression VisitIn(InExpression inExpression);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected abstract Expression VisitSqlParameter(SqlParameterExpression sqlParameterExpression);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected abstract Expression VisitSqlConditional(SqlConditionalExpression sqlConditionalExpression);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected abstract Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected abstract Expression VisitSqlConstant(SqlConstantExpression sqlConstantExpression);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected abstract Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected abstract Expression VisitKeyAccess(KeyAccessExpression keyAccessExpression);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected abstract Expression VisitObjectAccess(ObjectAccessExpression objectAccessExpression);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected abstract Expression VisitRootReference(RootReferenceExpression rootReferenceExpression);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected abstract Expression VisitEntityProjection(EntityProjectionExpression entityProjectionExpression);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected abstract Expression VisitObjectArrayProjection(ObjectArrayProjectionExpression objectArrayProjectionExpression);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected abstract Expression VisitProjection(ProjectionExpression projectionExpression);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected abstract Expression VisitSelect(SelectExpression selectExpression);
    }
}
