// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

#nullable disable

namespace BrightChain.EntityFrameworkCore.Query.Internal
{
    public partial class BrightChainShapedQueryCompilingExpressionVisitor
    {
        private sealed class BrightChainProjectionBindingRemovingReadItemExpressionVisitor : BrightChainProjectionBindingRemovingExpressionVisitorBase
        {
            private readonly ReadItemExpression _readItemExpression;

            public BrightChainProjectionBindingRemovingReadItemExpressionVisitor(
                ReadItemExpression readItemExpression,
                ParameterExpression jObjectParameter,
                bool trackQueryResults)
                : base(jObjectParameter, trackQueryResults)
            {
                _readItemExpression = readItemExpression;
            }

            protected override ProjectionExpression GetProjection(ProjectionBindingExpression _)
            {
                return _readItemExpression.ProjectionExpression;
            }
        }
    }
}
