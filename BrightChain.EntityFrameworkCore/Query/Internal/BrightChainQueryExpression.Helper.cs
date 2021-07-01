// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Helpers;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace BrightChain.EntityFrameworkCore.Query.Internal
{
    public partial class BrightChainQueryExpression
    {
        private sealed class ResultEnumerable : IEnumerable<ValueBuffer>
        {
            private readonly Func<ValueBuffer> _getElement;

            public ResultEnumerable(Func<ValueBuffer> getElement) => this._getElement = getElement;

            public IEnumerator<ValueBuffer> GetEnumerator()
                => new ResultEnumerator(this._getElement());

            IEnumerator IEnumerable.GetEnumerator()
                => this.GetEnumerator();

            private sealed class ResultEnumerator : IEnumerator<ValueBuffer>
            {
                private readonly ValueBuffer _value;
                private bool _moved;

                public ResultEnumerator(ValueBuffer value)
                {
                    this._value = value;
                    this._moved = this._value.IsEmpty;
                }

                public bool MoveNext()
                {
                    if (!this._moved)
                    {
                        this._moved = true;

                        return this._moved;
                    }

                    return false;
                }

                public void Reset() => this._moved = false;

                object IEnumerator.Current
                    => this.Current;

                public ValueBuffer Current
                    => !this._moved ? ValueBuffer.Empty : this._value;

                void IDisposable.Dispose()
                {
                }
            }
        }

        private sealed class ProjectionMemberRemappingExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _queryExpression;
            private readonly Dictionary<ProjectionMember, ProjectionMember> _projectionMemberMappings;

            public ProjectionMemberRemappingExpressionVisitor(
                Expression queryExpression, Dictionary<ProjectionMember, ProjectionMember> projectionMemberMappings)
            {
                this._queryExpression = queryExpression;
                this._projectionMemberMappings = projectionMemberMappings;
            }

            [return: NotNullIfNotNull("expression")]
            public override Expression? Visit(Expression? expression)
            {
                if (expression is ProjectionBindingExpression projectionBindingExpression)
                {
                    Check.DebugAssert(projectionBindingExpression.ProjectionMember != null,
                        "ProjectionBindingExpression must have projection member.");

                    return new ProjectionBindingExpression(
                        this._queryExpression,
                        this._projectionMemberMappings[projectionBindingExpression.ProjectionMember],
                        projectionBindingExpression.Type);
                }

                return base.Visit(expression);
            }
        }

        private sealed class ProjectionMemberToIndexConvertingExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _queryExpression;
            private readonly Dictionary<ProjectionMember, int> _projectionMemberMappings;

            public ProjectionMemberToIndexConvertingExpressionVisitor(
                Expression queryExpression, Dictionary<ProjectionMember, int> projectionMemberMappings)
            {
                this._queryExpression = queryExpression;
                this._projectionMemberMappings = projectionMemberMappings;
            }

            [return: NotNullIfNotNull("expression")]
            public override Expression? Visit(Expression? expression)
            {
                if (expression is ProjectionBindingExpression projectionBindingExpression)
                {
                    Check.DebugAssert(projectionBindingExpression.ProjectionMember != null,
                        "ProjectionBindingExpression must have projection member.");

                    return new ProjectionBindingExpression(
                        this._queryExpression,
                        this._projectionMemberMappings[projectionBindingExpression.ProjectionMember],
                        projectionBindingExpression.Type);
                }

                return base.Visit(expression);
            }
        }

        private sealed class ProjectionIndexRemappingExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _oldExpression;
            private readonly Expression _newExpression;
            private readonly int[] _indexMap;

            public ProjectionIndexRemappingExpressionVisitor(
                Expression oldExpression, Expression newExpression, int[] indexMap)
            {
                this._oldExpression = oldExpression;
                this._newExpression = newExpression;
                this._indexMap = indexMap;
            }

            [return: NotNullIfNotNull("expression")]
            public override Expression? Visit(Expression? expression)
            {
                if (expression is ProjectionBindingExpression projectionBindingExpression
                    && ReferenceEquals(projectionBindingExpression.QueryExpression, this._oldExpression))
                {
                    Check.DebugAssert(projectionBindingExpression.Index != null,
                        "ProjectionBindingExpression must have index.");

                    return new ProjectionBindingExpression(
                        this._newExpression,
                        this._indexMap[projectionBindingExpression.Index.Value],
                        projectionBindingExpression.Type);
                }

                return base.Visit(expression);
            }
        }

        private sealed class EntityShaperNullableMarkingExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitExtension(Expression extensionExpression)
            {
                Check.NotNull(extensionExpression, nameof(extensionExpression));

                return extensionExpression is EntityShaperExpression entityShaper
                    ? entityShaper.MakeNullable()
                    : base.VisitExtension(extensionExpression);
            }
        }
    }
}
