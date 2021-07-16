// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Text.Json;
using BrightChain.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
    public partial class BrightChainShapedQueryCompilingExpressionVisitor : ShapedQueryCompilingExpressionVisitor
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;
        private readonly Type _contextType;
        private readonly bool _threadSafetyChecksEnabled;
        private readonly string _partitionKeyFromExtension;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public BrightChainShapedQueryCompilingExpressionVisitor(
            ShapedQueryCompilingExpressionVisitorDependencies dependencies,
            BrightChainQueryCompilationContext brightChainQueryCompilationContext,
            ISqlExpressionFactory sqlExpressionFactory,
            IQuerySqlGeneratorFactory querySqlGeneratorFactory)
            : base(dependencies, brightChainQueryCompilationContext)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _querySqlGeneratorFactory = querySqlGeneratorFactory;
            _contextType = brightChainQueryCompilationContext.ContextType;
            _threadSafetyChecksEnabled = dependencies.CoreSingletonOptions.AreThreadSafetyChecksEnabled;
            _partitionKeyFromExtension = brightChainQueryCompilationContext.PartitionKeyFromExtension;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitShapedQuery(ShapedQueryExpression shapedQueryExpression)
        {
            Check.NotNull(shapedQueryExpression, nameof(shapedQueryExpression));

            var jObjectParameter = Expression.Parameter(typeof(JsonElement), "jsonElement");

            var shaperBody = shapedQueryExpression.ShaperExpression;
            shaperBody = new JObjectInjectingExpressionVisitor().Visit(shaperBody);
            shaperBody = InjectEntityMaterializers(shaperBody);

            switch (shapedQueryExpression.QueryExpression)
            {
                case SelectExpression selectExpression:

                    shaperBody = new BrightChainProjectionBindingRemovingExpressionVisitor(
                            selectExpression, jObjectParameter,
                            QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll)
                        .Visit(shaperBody);

                    var shaperLambda = Expression.Lambda(
                        shaperBody,
                        QueryCompilationContext.QueryContextParameter,
                        jObjectParameter);

                    return Expression.New(
                        typeof(QueryingEnumerable<>).MakeGenericType(shaperLambda.ReturnType).GetConstructors()[0],
                        Expression.Convert(
                            QueryCompilationContext.QueryContextParameter,
                            typeof(BrightChainQueryContext)),
                        Expression.Constant(_sqlExpressionFactory),
                        Expression.Constant(_querySqlGeneratorFactory),
                        Expression.Constant(selectExpression),
                        Expression.Constant(shaperLambda.Compile()),
                        Expression.Constant(_contextType),
                        Expression.Constant(_partitionKeyFromExtension, typeof(string)),
                        Expression.Constant(
                            QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
                        Expression.Constant(_threadSafetyChecksEnabled));

                case ReadItemExpression readItemExpression:

                    shaperBody = new BrightChainProjectionBindingRemovingReadItemExpressionVisitor(
                            readItemExpression, jObjectParameter,
                            QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll)
                        .Visit(shaperBody);

                    var shaperReadItemLambda = Expression.Lambda(
                        shaperBody,
                        QueryCompilationContext.QueryContextParameter,
                        jObjectParameter);

                    return Expression.New(
                        typeof(ReadItemQueryingEnumerable<>).MakeGenericType(shaperReadItemLambda.ReturnType).GetConstructors()[0],
                        Expression.Convert(
                            QueryCompilationContext.QueryContextParameter,
                            typeof(BrightChainQueryContext)),
                        Expression.Constant(readItemExpression),
                        Expression.Constant(shaperReadItemLambda.Compile()),
                        Expression.Constant(_contextType),
                        Expression.Constant(
                            QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
                        Expression.Constant(_threadSafetyChecksEnabled));

                default:
                    throw new NotSupportedException(CoreStrings.UnhandledExpressionNode(shapedQueryExpression.QueryExpression));
            }
        }
    }
}
