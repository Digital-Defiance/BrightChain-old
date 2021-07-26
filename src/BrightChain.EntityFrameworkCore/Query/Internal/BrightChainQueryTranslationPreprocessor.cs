// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace BrightChain.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class BrightChainQueryTranslationPreprocessor : QueryTranslationPreprocessor
    {
        private readonly BrightChainQueryCompilationContext _queryCompilationContext;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public BrightChainQueryTranslationPreprocessor(
            QueryTranslationPreprocessorDependencies dependencies,
            BrightChainQueryCompilationContext brightChainQueryCompilationContext)
            : base(dependencies, brightChainQueryCompilationContext)
        {
            this._queryCompilationContext = brightChainQueryCompilationContext;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Expression NormalizeQueryableMethod(Expression query)
        {
            query = new BrightChainQueryMetadataExtractingExpressionVisitor(this._queryCompilationContext).Visit(query);
            query = base.NormalizeQueryableMethod(query);

            return query;
        }
    }
}
