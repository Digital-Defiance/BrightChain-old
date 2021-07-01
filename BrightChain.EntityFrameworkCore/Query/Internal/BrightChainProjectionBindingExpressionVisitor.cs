// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Extensions;
using BrightChain.EntityFrameworkCore.Helpers;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace BrightChain.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class BrightChainProjectionBindingExpressionVisitor : ExpressionVisitor
    {
        private readonly BrightChainQueryableMethodTranslatingExpressionVisitor _queryableMethodTranslatingExpressionVisitor;
        private readonly BrightChainExpressionTranslatingExpressionVisitor _expressionTranslatingExpressionVisitor;

        private BrightChainQueryExpression _queryExpression;
        private bool _indexBasedBinding;

        private Dictionary<EntityProjectionExpression, ProjectionBindingExpression>? _entityProjectionCache;

        private readonly Dictionary<ProjectionMember, Expression> _projectionMapping = new();
        private List<Expression>? _clientProjections;
        private readonly Stack<ProjectionMember> _projectionMembers = new();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public BrightChainProjectionBindingExpressionVisitor(
            BrightChainQueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor,
            BrightChainExpressionTranslatingExpressionVisitor expressionTranslatingExpressionVisitor)
        {
            this._queryableMethodTranslatingExpressionVisitor = queryableMethodTranslatingExpressionVisitor;
            this._expressionTranslatingExpressionVisitor = expressionTranslatingExpressionVisitor;
            this._queryExpression = null!;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression Translate(BrightChainQueryExpression queryExpression, Expression expression)
        {
            this._queryExpression = queryExpression;
            this._indexBasedBinding = false;

            this._projectionMembers.Push(new ProjectionMember());

            var expandedExpression = this._queryableMethodTranslatingExpressionVisitor.ExpandWeakEntities(this._queryExpression, expression);
            var result = this.Visit(expandedExpression);

            if (result == QueryCompilationContext.NotTranslatedExpression)
            {
                this._indexBasedBinding = true;
                this._projectionMapping.Clear();
                this._entityProjectionCache = new();
                this._clientProjections = new();

                expandedExpression = this._queryableMethodTranslatingExpressionVisitor.ExpandWeakEntities(this._queryExpression, expression);
                result = this.Visit(expandedExpression);

                this._queryExpression.ReplaceProjection(this._clientProjections);
                this._clientProjections = null;
            }
            else
            {
                this._queryExpression.ReplaceProjection(this._projectionMapping);
                this._projectionMapping.Clear();
            }

            this._queryExpression = null!;
            this._projectionMembers.Clear();
            result = MatchTypes(result!, expression.Type);

            return result;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            if (expression == null)
            {
                return null;
            }

            if (!(expression is NewExpression
                || expression is MemberInitExpression
                || expression is EntityShaperExpression
                || expression is IncludeExpression))
            {
                // This skips the group parameter from GroupJoin
                if (expression is ParameterExpression parameter
                    && parameter.Type.IsGenericType
                    && parameter.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return parameter;
                }

                if (this._indexBasedBinding)
                {
                    switch (expression)
                    {
                        case ConstantExpression _:
                            return expression;

                        case ProjectionBindingExpression projectionBindingExpression:
                            var mappedProjection = this._queryExpression.GetProjection(projectionBindingExpression);
                            if (mappedProjection is EntityProjectionExpression entityProjection)
                            {
                                return this.AddClientProjection(entityProjection, typeof(ValueBuffer));
                            }

                            if (mappedProjection is not BrightChainQueryExpression)
                            {
                                return this.AddClientProjection(mappedProjection, expression.Type.MakeNullable());
                            }

                            throw new InvalidOperationException(CoreStrings.TranslationFailed(projectionBindingExpression.Print()));

                        case MaterializeCollectionNavigationExpression materializeCollectionNavigationExpression:
                            {
                                var subquery = this._queryableMethodTranslatingExpressionVisitor.TranslateSubquery(
                                        materializeCollectionNavigationExpression.Subquery)!;
                                this._clientProjections!.Add(subquery.QueryExpression);
                                return new CollectionResultShaperExpression(
                                    new ProjectionBindingExpression(this._queryExpression, this._clientProjections.Count - 1, typeof(IEnumerable<ValueBuffer>)),
                                    subquery.ShaperExpression,
                                    materializeCollectionNavigationExpression.Navigation,
                                    materializeCollectionNavigationExpression.Navigation.ClrType.GetSequenceType());
                            }

                        case MethodCallExpression methodCallExpression:
                            if (methodCallExpression.Method.IsGenericMethod
                                && methodCallExpression.Method.DeclaringType == typeof(Enumerable)
                                && methodCallExpression.Method.Name == nameof(Enumerable.ToList)
                                && methodCallExpression.Arguments.Count == 1
                                && methodCallExpression.Arguments[0].Type.TryGetElementType(typeof(IQueryable<>)) != null)
                            {
                                var subquery = this._queryableMethodTranslatingExpressionVisitor.TranslateSubquery(
                                    methodCallExpression.Arguments[0]);
                                if (subquery != null)
                                {
                                    this._clientProjections!.Add(subquery.QueryExpression);
                                    return new CollectionResultShaperExpression(
                                        new ProjectionBindingExpression(this._queryExpression, this._clientProjections.Count - 1, typeof(IEnumerable<ValueBuffer>)),
                                        subquery.ShaperExpression,
                                        null,
                                        methodCallExpression.Method.GetGenericArguments()[0]);
                                }
                            }
                            else
                            {
                                var subquery = this._queryableMethodTranslatingExpressionVisitor.TranslateSubquery(methodCallExpression);
                                if (subquery != null)
                                {
                                    // This simplifies the check when subquery is translated and can be lifted as scalar.
                                    var scalarTranslation = this._expressionTranslatingExpressionVisitor.Translate(subquery);
                                    if (scalarTranslation != null)
                                    {
                                        return this.AddClientProjection(scalarTranslation, expression.Type.MakeNullable());
                                    }

                                    if (subquery.ResultCardinality == ResultCardinality.Enumerable)
                                    {
                                        this._clientProjections!.Add(subquery.QueryExpression);
                                        var projectionBindingExpression = new ProjectionBindingExpression(
                                            this._queryExpression, this._clientProjections.Count - 1, typeof(IEnumerable<ValueBuffer>));
                                        return new CollectionResultShaperExpression(
                                            projectionBindingExpression, subquery.ShaperExpression, navigation: null, subquery.ShaperExpression.Type);
                                    }
                                    else
                                    {
                                        this._clientProjections!.Add(subquery.QueryExpression);
                                        var projectionBindingExpression = new ProjectionBindingExpression(
                                            this._queryExpression, this._clientProjections.Count - 1, typeof(ValueBuffer));
                                        return new SingleResultShaperExpression(projectionBindingExpression, subquery.ShaperExpression);
                                    }
                                }
                            }
                            break;
                    }

                    var translation = this._expressionTranslatingExpressionVisitor.Translate(expression);
                    if (translation != null)
                    {
                        return this.AddClientProjection(translation, expression.Type.MakeNullable());
                    }

                    return base.Visit(expression);
                }
                else
                {
                    var translation = this._expressionTranslatingExpressionVisitor.Translate(expression);
                    if (translation == null)
                    {
                        return QueryCompilationContext.NotTranslatedExpression;
                    }

                    this._projectionMapping[this._projectionMembers.Peek()] = translation;

                    return new ProjectionBindingExpression(this._queryExpression, this._projectionMembers.Peek(), expression.Type.MakeNullable());
                }
            }

            return base.Visit(expression);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            var left = MatchTypes(this.Visit(binaryExpression.Left), binaryExpression.Left.Type);
            var right = MatchTypes(this.Visit(binaryExpression.Right), binaryExpression.Right.Type);

            return binaryExpression.Update(left, this.VisitAndConvert(binaryExpression.Conversion, "VisitBinary"), right);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
            var test = this.Visit(conditionalExpression.Test);
            var ifTrue = this.Visit(conditionalExpression.IfTrue);
            var ifFalse = this.Visit(conditionalExpression.IfFalse);

            if (test.Type == typeof(bool?))
            {
                test = Expression.Equal(test, Expression.Constant(true, typeof(bool?)));
            }

            return conditionalExpression.Update(test, ifTrue, ifFalse);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            Check.NotNull(extensionExpression, nameof(extensionExpression));

            if (extensionExpression is EntityShaperExpression entityShaperExpression)
            {
                EntityProjectionExpression entityProjectionExpression;
                if (entityShaperExpression.ValueBufferExpression is ProjectionBindingExpression projectionBindingExpression)
                {
                    if (projectionBindingExpression.ProjectionMember == null)
                    {
                        // We don't process binding with client projection
                        return QueryCompilationContext.NotTranslatedExpression;
                    }

                    entityProjectionExpression = (EntityProjectionExpression)((BrightChainQueryExpression)projectionBindingExpression.QueryExpression)
                        .GetProjection(projectionBindingExpression);
                }
                else
                {
                    entityProjectionExpression = (EntityProjectionExpression)entityShaperExpression.ValueBufferExpression;
                }

                if (this._indexBasedBinding)
                {
                    if (!this._entityProjectionCache!.TryGetValue(entityProjectionExpression, out var entityProjectionBinding))
                    {
                        entityProjectionBinding = this.AddClientProjection(entityProjectionExpression, typeof(ValueBuffer));
                        this._entityProjectionCache[entityProjectionExpression] = entityProjectionBinding;
                    }

                    return entityShaperExpression.Update(entityProjectionBinding);
                }

                this._projectionMapping[this._projectionMembers.Peek()] = entityProjectionExpression;

                return entityShaperExpression.Update(
                    new ProjectionBindingExpression(this._queryExpression, this._projectionMembers.Peek(), typeof(ValueBuffer)));
            }

            if (extensionExpression is IncludeExpression includeExpression)
            {
                return this._indexBasedBinding
                    ? base.VisitExtension(includeExpression)
                    : QueryCompilationContext.NotTranslatedExpression;
            }

            throw new InvalidOperationException(CoreStrings.TranslationFailed(extensionExpression.Print()));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ElementInit VisitElementInit(ElementInit elementInit)
            => elementInit.Update(elementInit.Arguments.Select(e => MatchTypes(this.Visit(e), e.Type)));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var expression = this.Visit(memberExpression.Expression);
            Expression updatedMemberExpression = memberExpression.Update(
                expression != null ? MatchTypes(expression, memberExpression.Expression!.Type) : expression);

            if (expression?.Type.IsNullableValueType() == true)
            {
                var nullableReturnType = memberExpression.Type.MakeNullable();
                if (!memberExpression.Type.IsNullableType())
                {
                    updatedMemberExpression = Expression.Convert(updatedMemberExpression, nullableReturnType);
                }

                updatedMemberExpression = Expression.Condition(
                    Expression.Equal(expression, Expression.Default(expression.Type)),
                    Expression.Constant(null, nullableReturnType),
                    updatedMemberExpression);
            }

            return updatedMemberExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment memberAssignment)
        {
            var expression = memberAssignment.Expression;
            Expression? visitedExpression;
            if (this._indexBasedBinding)
            {
                visitedExpression = this.Visit(memberAssignment.Expression);
            }
            else
            {
                var projectionMember = this._projectionMembers.Peek().Append(memberAssignment.Member);
                this._projectionMembers.Push(projectionMember);

                visitedExpression = this.Visit(memberAssignment.Expression);
                if (visitedExpression == QueryCompilationContext.NotTranslatedExpression)
                {
                    return memberAssignment.Update(Expression.Convert(visitedExpression, memberAssignment.Expression.Type));
                }

                this._projectionMembers.Pop();
            }

            visitedExpression = MatchTypes(visitedExpression, expression.Type);

            return memberAssignment.Update(visitedExpression);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMemberInit(MemberInitExpression memberInitExpression)
        {
            Check.NotNull(memberInitExpression, nameof(memberInitExpression));

            var newExpression = this.Visit(memberInitExpression.NewExpression);
            if (newExpression == QueryCompilationContext.NotTranslatedExpression)
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            var newBindings = new MemberBinding[memberInitExpression.Bindings.Count];
            for (var i = 0; i < newBindings.Length; i++)
            {
                if (memberInitExpression.Bindings[i].BindingType != MemberBindingType.Assignment)
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                newBindings[i] = this.VisitMemberBinding(memberInitExpression.Bindings[i]);
                if (((MemberAssignment)newBindings[i]).Expression is UnaryExpression unaryExpression
                    && unaryExpression.NodeType == ExpressionType.Convert
                    && unaryExpression.Operand == QueryCompilationContext.NotTranslatedExpression)
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }
            }

            return memberInitExpression.Update((NewExpression)newExpression, newBindings);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var @object = this.Visit(methodCallExpression.Object);
            var arguments = new Expression[methodCallExpression.Arguments.Count];
            for (var i = 0; i < methodCallExpression.Arguments.Count; i++)
            {
                var argument = methodCallExpression.Arguments[i];
                arguments[i] = MatchTypes(this.Visit(argument), argument.Type);
            }

            Expression updatedMethodCallExpression = methodCallExpression.Update(
                @object != null ? MatchTypes(@object, methodCallExpression.Object!.Type) : @object!,
                arguments);

            if (@object?.Type.IsNullableType() == true
                && !methodCallExpression.Object!.Type.IsNullableType())
            {
                var nullableReturnType = methodCallExpression.Type.MakeNullable();
                if (!methodCallExpression.Type.IsNullableType())
                {
                    updatedMethodCallExpression = Expression.Convert(updatedMethodCallExpression, nullableReturnType);
                }

                return Expression.Condition(
                    Expression.Equal(@object, Expression.Default(@object.Type)),
                    Expression.Constant(null, nullableReturnType),
                    updatedMethodCallExpression);
            }

            return updatedMethodCallExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitNew(NewExpression newExpression)
        {
            Check.NotNull(newExpression, nameof(newExpression));

            if (newExpression.Arguments.Count == 0)
            {
                return newExpression;
            }

            if (!this._indexBasedBinding
                && newExpression.Members == null)
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            var newArguments = new Expression[newExpression.Arguments.Count];
            for (var i = 0; i < newArguments.Length; i++)
            {
                var argument = newExpression.Arguments[i];
                Expression? visitedArgument;
                if (this._indexBasedBinding)
                {
                    visitedArgument = this.Visit(argument);
                }
                else
                {
                    var projectionMember = this._projectionMembers.Peek().Append(newExpression.Members![i]);
                    this._projectionMembers.Push(projectionMember);
                    visitedArgument = this.Visit(argument);
                    if (visitedArgument == QueryCompilationContext.NotTranslatedExpression)
                    {
                        return QueryCompilationContext.NotTranslatedExpression;
                    }

                    this._projectionMembers.Pop();
                }

                newArguments[i] = MatchTypes(visitedArgument, argument.Type);
            }

            return newExpression.Update(newArguments);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitNewArray(NewArrayExpression newArrayExpression)
            => newArrayExpression.Update(newArrayExpression.Expressions.Select(e => MatchTypes(this.Visit(e), e.Type)));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            var operand = this.Visit(unaryExpression.Operand);

            return (unaryExpression.NodeType == ExpressionType.Convert
                    || unaryExpression.NodeType == ExpressionType.ConvertChecked)
                && unaryExpression.Type == operand.Type
                    ? operand
                    : unaryExpression.Update(MatchTypes(operand, unaryExpression.Operand.Type));
        }

        private static Expression MatchTypes(Expression expression, Type targetType)
        {
            if (targetType != expression.Type
                && targetType.TryGetElementType(typeof(IQueryable<>)) == null)
            {
                Check.DebugAssert(targetType.MakeNullable() == expression.Type, "Not a nullable to non-nullable conversion");

                expression = Expression.Convert(expression, targetType);
            }

            return expression;
        }

        private ProjectionBindingExpression AddClientProjection(Expression expression, Type type)
        {
            var existingIndex = this._clientProjections!.FindIndex(e => e.Equals(expression));
            if (existingIndex == -1)
            {
                this._clientProjections.Add(expression);
                existingIndex = this._clientProjections.Count - 1;
            }

            return new ProjectionBindingExpression(this._queryExpression, existingIndex, type);
        }
    }
}
