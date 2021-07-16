// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;

#nullable disable

namespace BrightChain.EntityFrameworkCore.Query.Internal
{
    public partial class BrightChainShapedQueryCompilingExpressionVisitor
    {
        private sealed class BrightChainProjectionBindingRemovingExpressionVisitor : BrightChainProjectionBindingRemovingExpressionVisitorBase
        {
            private readonly SelectExpression _selectExpression;

            public BrightChainProjectionBindingRemovingExpressionVisitor(
                SelectExpression selectExpression,
                ParameterExpression jObjectParameter,
                bool trackQueryResults)
                : base(jObjectParameter, trackQueryResults)
            {
                _selectExpression = selectExpression;
            }

            protected override ProjectionExpression GetProjection(ProjectionBindingExpression projectionBindingExpression)
            {
                return _selectExpression.Projection[GetProjectionIndex(projectionBindingExpression)];
            }

            private int GetProjectionIndex(ProjectionBindingExpression projectionBindingExpression)
            {
                return projectionBindingExpression.ProjectionMember != null
                                   ? _selectExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember).GetConstantValue<int>()
                                   : projectionBindingExpression.Index
                                   ?? throw new InvalidOperationException(CoreStrings.TranslationFailed(projectionBindingExpression.Print()));
            }
        }
    }
}
