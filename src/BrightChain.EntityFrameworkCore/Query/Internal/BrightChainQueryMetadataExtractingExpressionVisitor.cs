// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Utilities;
using System.Linq.Expressions;

namespace BrightChain.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class BrightChainQueryMetadataExtractingExpressionVisitor : ExpressionVisitor
    {
        private readonly BrightChainQueryCompilationContext _brightChainQueryCompilationContext;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public BrightChainQueryMetadataExtractingExpressionVisitor(BrightChainQueryCompilationContext brightChainQueryCompilationContext)
        {
            Check.NotNull(brightChainQueryCompilationContext, nameof(brightChainQueryCompilationContext));
            _brightChainQueryCompilationContext = brightChainQueryCompilationContext;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() == BrightChainQueryableExtensions.WithPartitionKeyMethodInfo)
            {
                var innerQueryable = Visit(methodCallExpression.Arguments[0]);

                _brightChainQueryCompilationContext.PartitionKeyFromExtension = methodCallExpression.Arguments[1].GetConstantValue<string>();

                return innerQueryable;
            }

            return base.VisitMethodCall(methodCallExpression);
        }
    }
}
