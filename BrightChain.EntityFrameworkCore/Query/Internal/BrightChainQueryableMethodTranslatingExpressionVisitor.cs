// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Extensions;
using BrightChain.EntityFrameworkCore.Helpers;
using BrightChain.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BrightChain.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class BrightChainQueryableMethodTranslatingExpressionVisitor : QueryableMethodTranslatingExpressionVisitor
    {
        private readonly BrightChainExpressionTranslatingExpressionVisitor _expressionTranslator;
        private readonly WeakEntityExpandingExpressionVisitor _weakEntityExpandingExpressionVisitor;
        private readonly BrightChainProjectionBindingExpressionVisitor _projectionBindingExpressionVisitor;
        private readonly IModel _model;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public BrightChainQueryableMethodTranslatingExpressionVisitor(
            QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
            QueryCompilationContext queryCompilationContext)
            : base(dependencies, queryCompilationContext, subquery: false)
        {
            this._expressionTranslator = new BrightChainExpressionTranslatingExpressionVisitor(queryCompilationContext, this);
            this._weakEntityExpandingExpressionVisitor = new WeakEntityExpandingExpressionVisitor(this._expressionTranslator);
            this._projectionBindingExpressionVisitor = new BrightChainProjectionBindingExpressionVisitor(this, this._expressionTranslator);
            this._model = queryCompilationContext.Model;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected BrightChainQueryableMethodTranslatingExpressionVisitor(
            BrightChainQueryableMethodTranslatingExpressionVisitor parentVisitor)
            : base(parentVisitor.Dependencies, parentVisitor.QueryCompilationContext, subquery: true)
        {
            this._expressionTranslator = new BrightChainExpressionTranslatingExpressionVisitor(this.QueryCompilationContext, parentVisitor);
            this._weakEntityExpandingExpressionVisitor = new WeakEntityExpandingExpressionVisitor(this._expressionTranslator);
            this._projectionBindingExpressionVisitor = new BrightChainProjectionBindingExpressionVisitor(this, this._expressionTranslator);
            this._model = parentVisitor._model;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override QueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor()
            => new BrightChainQueryableMethodTranslatingExpressionVisitor(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Arguments.Count == 1
                && methodCallExpression.Arguments[0].Type.TryGetSequenceType() != null
                && (string.Equals(methodCallExpression.Method.Name, "AsSplitQuery", StringComparison.Ordinal)
                    || string.Equals(methodCallExpression.Method.Name, "AsSingleQuery", StringComparison.Ordinal)))
            {
                return this.Visit(methodCallExpression.Arguments[0]);
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [Obsolete("Use overload which takes IEntityType.")]
        protected override ShapedQueryExpression CreateShapedQueryExpression(Type elementType)
        {
            Check.NotNull(elementType, nameof(elementType));

            // Let it throw if null found.
            return this.CreateShapedQueryExpression(this._model.FindEntityType(elementType)!);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression CreateShapedQueryExpression(IEntityType entityType)
            => CreateShapedQueryExpressionStatic(entityType);

        private static ShapedQueryExpression CreateShapedQueryExpressionStatic(IEntityType entityType)
        {
            var queryExpression = new BrightChainQueryExpression(entityType);

            return new ShapedQueryExpression(
                queryExpression,
                new EntityShaperExpression(
                    entityType,
                    new ProjectionBindingExpression(
                        queryExpression,
                        new ProjectionMember(),
                        typeof(ValueBuffer)),
                    false));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateAll(ShapedQueryExpression source, LambdaExpression predicate)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(predicate, nameof(predicate));

            predicate = Expression.Lambda(Expression.Not(predicate.Body), predicate.Parameters);
            var newSource = this.TranslateWhere(source, predicate);
            if (newSource == null)
            {
                return null;
            }
            source = newSource;

            var brightChainQueryExpression = (BrightChainQueryExpression)source.QueryExpression;

            if (source.ShaperExpression is GroupByShaperExpression)
            {
                brightChainQueryExpression.ReplaceProjection(new Dictionary<ProjectionMember, Expression>());
            }

            brightChainQueryExpression.UpdateServerQueryExpression(
                Expression.Not(
                    Expression.Call(
                        EnumerableMethods.AnyWithoutPredicate.MakeGenericMethod(brightChainQueryExpression.CurrentParameter.Type),
                        brightChainQueryExpression.ServerQueryExpression)));

            return source.UpdateShaperExpression(Expression.Convert(brightChainQueryExpression.GetSingleScalarProjection(), typeof(bool)));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateAny(ShapedQueryExpression source, LambdaExpression? predicate)
        {
            if (predicate != null)
            {
                var newSource = this.TranslateWhere(source, predicate);
                if (newSource == null)
                {
                    return null;
                }
                source = newSource;
            }

            var brightChainQueryExpression = (BrightChainQueryExpression)source.QueryExpression;

            if (source.ShaperExpression is GroupByShaperExpression)
            {
                brightChainQueryExpression.ReplaceProjection(new Dictionary<ProjectionMember, Expression>());
            }

            brightChainQueryExpression.UpdateServerQueryExpression(
                Expression.Call(
                    EnumerableMethods.AnyWithoutPredicate.MakeGenericMethod(brightChainQueryExpression.CurrentParameter.Type),
                    brightChainQueryExpression.ServerQueryExpression));

            return source.UpdateShaperExpression(Expression.Convert(brightChainQueryExpression.GetSingleScalarProjection(), typeof(bool)));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateAverage(ShapedQueryExpression source, LambdaExpression? selector, Type resultType)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(resultType, nameof(resultType));

            return this.TranslateScalarAggregate(source, selector, nameof(Enumerable.Average), resultType);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateCast(ShapedQueryExpression source, Type resultType)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(resultType, nameof(resultType));

            return source.ShaperExpression.Type != resultType
                ? source.UpdateShaperExpression(Expression.Convert(source.ShaperExpression, resultType))
                : source;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateConcat(ShapedQueryExpression source1, ShapedQueryExpression source2)
        {
            Check.NotNull(source1, nameof(source1));
            Check.NotNull(source2, nameof(source2));

            return this.TranslateSetOperation(EnumerableMethods.Concat, source1, source2);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateContains(ShapedQueryExpression source, Expression item)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(item, nameof(item));

            var brightChainQueryExpression = (BrightChainQueryExpression)source.QueryExpression;
            var newItem = this.TranslateExpression(item, preserveType: true);
            if (newItem == null)
            {
                return null;
            }

            item = newItem;

            brightChainQueryExpression.UpdateServerQueryExpression(
                Expression.Call(
                    EnumerableMethods.Contains.MakeGenericMethod(item.Type),
                    Expression.Call(
                        EnumerableMethods.Select.MakeGenericMethod(brightChainQueryExpression.CurrentParameter.Type, item.Type),
                        brightChainQueryExpression.ServerQueryExpression,
                        Expression.Lambda(
                            brightChainQueryExpression.GetProjection(
                                new ProjectionBindingExpression(brightChainQueryExpression, new ProjectionMember(), item.Type)),
                            brightChainQueryExpression.CurrentParameter)),
                    item));

            return source.UpdateShaperExpression(Expression.Convert(brightChainQueryExpression.GetSingleScalarProjection(), typeof(bool)));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateCount(ShapedQueryExpression source, LambdaExpression? predicate)
        {
            Check.NotNull(source, nameof(source));

            if (predicate != null)
            {
                var newSource = this.TranslateWhere(source, predicate);
                if (newSource == null)
                {
                    return null;
                }
                source = newSource;
            }

            var brightChainQueryExpression = (BrightChainQueryExpression)source.QueryExpression;

            if (source.ShaperExpression is GroupByShaperExpression)
            {
                brightChainQueryExpression.ReplaceProjection(new Dictionary<ProjectionMember, Expression>());
            }

            brightChainQueryExpression.UpdateServerQueryExpression(
                Expression.Call(
                    EnumerableMethods.CountWithoutPredicate.MakeGenericMethod(brightChainQueryExpression.CurrentParameter.Type),
                    brightChainQueryExpression.ServerQueryExpression));

            return source.UpdateShaperExpression(Expression.Convert(brightChainQueryExpression.GetSingleScalarProjection(), typeof(int)));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateDefaultIfEmpty(ShapedQueryExpression source, Expression? defaultValue)
        {
            Check.NotNull(source, nameof(source));

            if (defaultValue == null)
            {
                ((BrightChainQueryExpression)source.QueryExpression).ApplyDefaultIfEmpty();
                return source.UpdateShaperExpression(this.MarkShaperNullable(source.ShaperExpression));
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateDistinct(ShapedQueryExpression source)
        {
            Check.NotNull(source, nameof(source));

            ((BrightChainQueryExpression)source.QueryExpression).ApplyDistinct();

            return source;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateElementAtOrDefault(
            ShapedQueryExpression source,
            Expression index,
            bool returnDefault)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(index, nameof(index));

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateExcept(ShapedQueryExpression source1, ShapedQueryExpression source2)
        {
            Check.NotNull(source1, nameof(source1));
            Check.NotNull(source2, nameof(source2));

            return this.TranslateSetOperation(EnumerableMethods.Except, source1, source2);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateFirstOrDefault(
            ShapedQueryExpression source,
            LambdaExpression? predicate,
            Type returnType,
            bool returnDefault)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(returnType, nameof(returnType));

            return this.TranslateSingleResultOperator(
                source,
                predicate,
                returnType,
                returnDefault
                    ? EnumerableMethods.FirstOrDefaultWithoutPredicate
                    : EnumerableMethods.FirstWithoutPredicate);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateGroupBy(
            ShapedQueryExpression source,
            LambdaExpression keySelector,
            LambdaExpression? elementSelector,
            LambdaExpression? resultSelector)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(keySelector, nameof(keySelector));

            var remappedKeySelector = this.RemapLambdaBody(source, keySelector);

            var translatedKey = this.TranslateGroupingKey(remappedKeySelector);
            if (translatedKey != null)
            {
                var brightChainQueryExpression = (BrightChainQueryExpression)source.QueryExpression;
                var defaultElementSelector = elementSelector == null || elementSelector.Body == elementSelector.Parameters[0];
                if (!defaultElementSelector)
                {
                    source = this.TranslateSelect(source, elementSelector!);
                }

                var groupByShaper = brightChainQueryExpression.ApplyGrouping(translatedKey, source.ShaperExpression, defaultElementSelector);

                if (resultSelector == null)
                {
                    return source.UpdateShaperExpression(groupByShaper);
                }

                var original1 = resultSelector.Parameters[0];
                var original2 = resultSelector.Parameters[1];

                var newResultSelectorBody = new ReplacingExpressionVisitor(
                    new Expression[] { original1, original2 },
                    new[] { groupByShaper.KeySelector, groupByShaper }).Visit(resultSelector.Body);

                newResultSelectorBody = this.ExpandWeakEntities(brightChainQueryExpression, newResultSelectorBody);
                var newShaper = this._projectionBindingExpressionVisitor.Translate(brightChainQueryExpression, newResultSelectorBody);

                return source.UpdateShaperExpression(newShaper);
            }

            return null;
        }

        private Expression? TranslateGroupingKey(Expression expression)
        {
            switch (expression)
            {
                case NewExpression newExpression:
                    if (newExpression.Arguments.Count == 0)
                    {
                        return newExpression;
                    }

                    var newArguments = new Expression[newExpression.Arguments.Count];
                    for (var i = 0; i < newArguments.Length; i++)
                    {
                        var key = this.TranslateGroupingKey(newExpression.Arguments[i]);
                        if (key == null)
                        {
                            return null;
                        }
                        newArguments[i] = key;
                    }

                    return newExpression.Update(newArguments);

                case MemberInitExpression memberInitExpression:
                    var updatedNewExpression = (NewExpression?)this.TranslateGroupingKey(memberInitExpression.NewExpression);
                    if (updatedNewExpression == null)
                    {
                        return null;
                    }

                    var newBindings = new MemberAssignment[memberInitExpression.Bindings.Count];
                    for (var i = 0; i < newBindings.Length; i++)
                    {
                        var memberAssignment = (MemberAssignment)memberInitExpression.Bindings[i];
                        var visitedExpression = this.TranslateGroupingKey(memberAssignment.Expression);
                        if (visitedExpression == null)
                        {
                            return null;
                        }

                        newBindings[i] = memberAssignment.Update(visitedExpression);
                    }

                    return memberInitExpression.Update(updatedNewExpression, newBindings);

                default:
                    var translation = this.TranslateExpression(expression);
                    if (translation == null)
                    {
                        return null;
                    }

                    return translation.Type == expression.Type
                        ? translation
                        : Expression.Convert(translation, expression.Type);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateGroupJoin(
            ShapedQueryExpression outer,
            ShapedQueryExpression inner,
            LambdaExpression outerKeySelector,
            LambdaExpression innerKeySelector,
            LambdaExpression resultSelector)
        {
            Check.NotNull(outer, nameof(outer));
            Check.NotNull(inner, nameof(inner));
            Check.NotNull(outerKeySelector, nameof(outerKeySelector));
            Check.NotNull(innerKeySelector, nameof(innerKeySelector));
            Check.NotNull(resultSelector, nameof(resultSelector));

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateIntersect(ShapedQueryExpression source1, ShapedQueryExpression source2)
        {
            Check.NotNull(source1, nameof(source1));
            Check.NotNull(source2, nameof(source2));

            return this.TranslateSetOperation(EnumerableMethods.Intersect, source1, source2);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateJoin(
            ShapedQueryExpression outer,
            ShapedQueryExpression inner,
            LambdaExpression outerKeySelector,
            LambdaExpression innerKeySelector,
            LambdaExpression resultSelector)
        {
            Check.NotNull(outer, nameof(outer));
            Check.NotNull(inner, nameof(inner));
            Check.NotNull(outerKeySelector, nameof(outerKeySelector));
            Check.NotNull(innerKeySelector, nameof(innerKeySelector));
            Check.NotNull(resultSelector, nameof(resultSelector));

            var (newOuterKeySelector, newInnerKeySelector) = this.ProcessJoinKeySelector(outer, inner, outerKeySelector, innerKeySelector);

            if (newOuterKeySelector == null
                || newInnerKeySelector == null)
            {
                return null;
            }
            (outerKeySelector, innerKeySelector) = (newOuterKeySelector, newInnerKeySelector);

            var outerShaperExpression = ((BrightChainQueryExpression)outer.QueryExpression).AddInnerJoin(
                (BrightChainQueryExpression)inner.QueryExpression,
                outerKeySelector,
                innerKeySelector,
                outer.ShaperExpression,
                inner.ShaperExpression);

            outer = outer.UpdateShaperExpression(outerShaperExpression);

            return this.TranslateTwoParameterSelector(outer, resultSelector);
        }

        private (LambdaExpression? OuterKeySelector, LambdaExpression? InnerKeySelector) ProcessJoinKeySelector(
            ShapedQueryExpression outer,
            ShapedQueryExpression inner,
            LambdaExpression outerKeySelector,
            LambdaExpression innerKeySelector)
        {
            var left = this.RemapLambdaBody(outer, outerKeySelector);
            var right = this.RemapLambdaBody(inner, innerKeySelector);

            var joinCondition = this.TranslateExpression(Expression.Equal(left, right));

            var (outerKeyBody, innerKeyBody) = DecomposeJoinCondition(joinCondition);

            if (outerKeyBody == null
                || innerKeyBody == null)
            {
                return (null, null);
            }

            outerKeySelector = Expression.Lambda(outerKeyBody, ((BrightChainQueryExpression)outer.QueryExpression).CurrentParameter);
            innerKeySelector = Expression.Lambda(innerKeyBody, ((BrightChainQueryExpression)inner.QueryExpression).CurrentParameter);

            return AlignKeySelectorTypes(outerKeySelector, innerKeySelector);
        }

        private static (Expression?, Expression?) DecomposeJoinCondition(Expression? joinCondition)
        {
            var leftExpressions = new List<Expression>();
            var rightExpressions = new List<Expression>();

            return ProcessJoinCondition(joinCondition, leftExpressions, rightExpressions)
                ? leftExpressions.Count == 1
                    ? (leftExpressions[0], rightExpressions[0])
                    : (CreateAnonymousObject(leftExpressions), CreateAnonymousObject(rightExpressions))
                : (null, null);

            // BrightChain joins need to use AnonymousObject to perform correct key comparison for server side joins
            static Expression CreateAnonymousObject(List<Expression> expressions)
                => Expression.New(
                    AnonymousObject.AnonymousObjectCtor,
                    Expression.NewArrayInit(
                        typeof(object),
                        expressions.Select(e => Expression.Convert(e, typeof(object)))));
        }

        private static bool ProcessJoinCondition(
            Expression? joinCondition,
            List<Expression> leftExpressions,
            List<Expression> rightExpressions)
        {
            if (joinCondition is BinaryExpression binaryExpression)
            {
                if (binaryExpression.NodeType == ExpressionType.Equal)
                {
                    leftExpressions.Add(binaryExpression.Left);
                    rightExpressions.Add(binaryExpression.Right);

                    return true;
                }

                if (binaryExpression.NodeType == ExpressionType.AndAlso)
                {
                    return ProcessJoinCondition(binaryExpression.Left, leftExpressions, rightExpressions)
                        && ProcessJoinCondition(binaryExpression.Right, leftExpressions, rightExpressions);
                }
            }

            if (joinCondition is MethodCallExpression methodCallExpression
                && methodCallExpression.Method.IsStatic
                && methodCallExpression.Method.DeclaringType == typeof(object)
                && methodCallExpression.Method.Name == nameof(object.Equals)
                && methodCallExpression.Arguments.Count == 2)
            {
                leftExpressions.Add(methodCallExpression.Arguments[0]);
                rightExpressions.Add(methodCallExpression.Arguments[1]);

                return true;
            }

            return false;
        }

        private static (LambdaExpression OuterKeySelector, LambdaExpression InnerKeySelector)
            AlignKeySelectorTypes(LambdaExpression outerKeySelector, LambdaExpression innerKeySelector)
        {
            if (outerKeySelector.Body.Type != innerKeySelector.Body.Type)
            {
                if (IsConvertedToNullable(outerKeySelector.Body, innerKeySelector.Body))
                {
                    innerKeySelector = Expression.Lambda(
                        Expression.Convert(innerKeySelector.Body, outerKeySelector.Body.Type), innerKeySelector.Parameters);
                }
                else if (IsConvertedToNullable(innerKeySelector.Body, outerKeySelector.Body))
                {
                    outerKeySelector = Expression.Lambda(
                        Expression.Convert(outerKeySelector.Body, innerKeySelector.Body.Type), outerKeySelector.Parameters);
                }
            }

            return (outerKeySelector, innerKeySelector);

            static bool IsConvertedToNullable(Expression outer, Expression inner)
                => outer.Type.IsNullableType()
                    && !inner.Type.IsNullableType()
                    && outer.Type.UnwrapNullableType() == inner.Type;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateLastOrDefault(
            ShapedQueryExpression source,
            LambdaExpression? predicate,
            Type returnType,
            bool returnDefault)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(returnType, nameof(returnType));

            return this.TranslateSingleResultOperator(
                source,
                predicate,
                returnType,
                returnDefault
                    ? EnumerableMethods.LastOrDefaultWithoutPredicate
                    : EnumerableMethods.LastWithoutPredicate);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateLeftJoin(
            ShapedQueryExpression outer,
            ShapedQueryExpression inner,
            LambdaExpression outerKeySelector,
            LambdaExpression innerKeySelector,
            LambdaExpression resultSelector)
        {
            Check.NotNull(outer, nameof(outer));
            Check.NotNull(inner, nameof(inner));
            Check.NotNull(outerKeySelector, nameof(outerKeySelector));
            Check.NotNull(innerKeySelector, nameof(innerKeySelector));
            Check.NotNull(resultSelector, nameof(resultSelector));

            var (newOuterKeySelector, newInnerKeySelector) = this.ProcessJoinKeySelector(outer, inner, outerKeySelector, innerKeySelector);

            if (newOuterKeySelector == null
                || newInnerKeySelector == null)
            {
                return null;
            }

            (outerKeySelector, innerKeySelector) = (newOuterKeySelector, newInnerKeySelector);

            var outerShaperExpression = ((BrightChainQueryExpression)outer.QueryExpression).AddLeftJoin(
                (BrightChainQueryExpression)inner.QueryExpression,
                outerKeySelector,
                innerKeySelector,
                outer.ShaperExpression,
                inner.ShaperExpression);

            outer = outer.UpdateShaperExpression(outerShaperExpression);

            return this.TranslateTwoParameterSelector(outer, resultSelector);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateLongCount(ShapedQueryExpression source, LambdaExpression? predicate)
        {
            Check.NotNull(source, nameof(source));

            if (predicate != null)
            {
                var newSource = this.TranslateWhere(source, predicate);
                if (newSource == null)
                {
                    return null;
                }

                source = newSource;
            }

            var brightChainQueryExpression = (BrightChainQueryExpression)source.QueryExpression;

            if (source.ShaperExpression is GroupByShaperExpression)
            {
                brightChainQueryExpression.ReplaceProjection(new Dictionary<ProjectionMember, Expression>());
            }

            brightChainQueryExpression.UpdateServerQueryExpression(
                Expression.Call(
                    EnumerableMethods.LongCountWithoutPredicate.MakeGenericMethod(
                        brightChainQueryExpression.CurrentParameter.Type),
                    brightChainQueryExpression.ServerQueryExpression));

            return source.UpdateShaperExpression(Expression.Convert(brightChainQueryExpression.GetSingleScalarProjection(), typeof(long)));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateMax(
            ShapedQueryExpression source,
            LambdaExpression? selector,
            Type resultType)
        {
            Check.NotNull(source, nameof(source));

            return this.TranslateScalarAggregate(source, selector, nameof(Enumerable.Max), resultType);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateMin(ShapedQueryExpression source, LambdaExpression? selector, Type resultType)
        {
            Check.NotNull(source, nameof(source));

            return this.TranslateScalarAggregate(source, selector, nameof(Enumerable.Min), resultType);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateOfType(ShapedQueryExpression source, Type resultType)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(resultType, nameof(resultType));

            if (source.ShaperExpression is EntityShaperExpression entityShaperExpression)
            {
                var entityType = entityShaperExpression.EntityType;
                if (entityType.ClrType == resultType)
                {
                    return source;
                }

                var parameterExpression = Expression.Parameter(entityShaperExpression.Type);
                var predicate = Expression.Lambda(Expression.TypeIs(parameterExpression, resultType), parameterExpression);
                var newSource = this.TranslateWhere(source, predicate);
                if (newSource == null)
                {
                    // EntityType is not part of hierarchy
                    return null;
                }
                source = newSource;

                var baseType = entityType.GetAllBaseTypes().SingleOrDefault(et => et.ClrType == resultType);
                if (baseType != null)
                {
                    return source.UpdateShaperExpression(entityShaperExpression.WithEntityType(baseType));
                }

                var derivedType = entityType.GetDerivedTypes().Single(et => et.ClrType == resultType);
                var brightChainQueryExpression = (BrightChainQueryExpression)source.QueryExpression;

                var projectionBindingExpression = (ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression;
                var projectionMember = projectionBindingExpression.ProjectionMember;
                Check.DebugAssert(new ProjectionMember().Equals(projectionMember), "Invalid ProjectionMember when processing OfType");

                var entityProjectionExpression = (EntityProjectionExpression)brightChainQueryExpression.GetProjection(projectionBindingExpression);
                brightChainQueryExpression.ReplaceProjection(
                    new Dictionary<ProjectionMember, Expression>
                    {
                        { projectionMember, entityProjectionExpression.UpdateEntityType(derivedType) }
                    });

                return source.UpdateShaperExpression(entityShaperExpression.WithEntityType(derivedType));
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateOrderBy(
            ShapedQueryExpression source,
            LambdaExpression keySelector,
            bool ascending)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(keySelector, nameof(keySelector));

            var brightChainQueryExpression = (BrightChainQueryExpression)source.QueryExpression;

            var newKeySelector = this.TranslateLambdaExpression(source, keySelector);
            if (newKeySelector == null)
            {
                return null;
            }
            keySelector = newKeySelector;

            var orderBy = ascending ? EnumerableMethods.OrderBy : EnumerableMethods.OrderByDescending;
            brightChainQueryExpression.UpdateServerQueryExpression(
                Expression.Call(
                    orderBy.MakeGenericMethod(brightChainQueryExpression.CurrentParameter.Type, keySelector.ReturnType),
                    brightChainQueryExpression.ServerQueryExpression,
                    keySelector));

            return source;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateReverse(ShapedQueryExpression source)
        {
            Check.NotNull(source, nameof(source));

            var brightChainQueryExpression = (BrightChainQueryExpression)source.QueryExpression;

            brightChainQueryExpression.UpdateServerQueryExpression(
                Expression.Call(
                    EnumerableMethods.Reverse.MakeGenericMethod(brightChainQueryExpression.CurrentParameter.Type),
                    brightChainQueryExpression.ServerQueryExpression));

            return source;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression TranslateSelect(ShapedQueryExpression source, LambdaExpression selector)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            if (selector.Body == selector.Parameters[0])
            {
                return source;
            }

            var newSelectorBody = ReplacingExpressionVisitor.Replace(
                selector.Parameters.Single(), source.ShaperExpression, selector.Body);

            var queryExpression = (BrightChainQueryExpression)source.QueryExpression;

            var newShaper = this._projectionBindingExpressionVisitor.Translate(queryExpression, newSelectorBody);

            return source.UpdateShaperExpression(newShaper);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateSelectMany(
            ShapedQueryExpression source,
            LambdaExpression collectionSelector,
            LambdaExpression resultSelector)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(collectionSelector, nameof(collectionSelector));
            Check.NotNull(resultSelector, nameof(resultSelector));

            var defaultIfEmpty = new DefaultIfEmptyFindingExpressionVisitor().IsOptional(collectionSelector);
            var collectionSelectorBody = this.RemapLambdaBody(source, collectionSelector);

            if (this.Visit(collectionSelectorBody) is ShapedQueryExpression inner)
            {
                var outerShaperExpression = ((BrightChainQueryExpression)source.QueryExpression).AddSelectMany(
                    (BrightChainQueryExpression)inner.QueryExpression, source.ShaperExpression, inner.ShaperExpression, defaultIfEmpty);

                source = source.UpdateShaperExpression(outerShaperExpression);

                return this.TranslateTwoParameterSelector(source, resultSelector);
            }

            return null;
        }

        private sealed class DefaultIfEmptyFindingExpressionVisitor : ExpressionVisitor
        {
            private bool _defaultIfEmpty;

            public bool IsOptional(LambdaExpression lambdaExpression)
            {
                this._defaultIfEmpty = false;

                this.Visit(lambdaExpression.Body);

                return this._defaultIfEmpty;
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                Check.NotNull(methodCallExpression, nameof(methodCallExpression));

                if (methodCallExpression.Method.IsGenericMethod
                    && methodCallExpression.Method.GetGenericMethodDefinition() == QueryableMethods.DefaultIfEmptyWithoutArgument)
                {
                    this._defaultIfEmpty = true;
                }

                return base.VisitMethodCall(methodCallExpression);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateSelectMany(ShapedQueryExpression source, LambdaExpression selector)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            var innerParameter = Expression.Parameter(selector.ReturnType.GetSequenceType(), "i");
            var resultSelector = Expression.Lambda(
                innerParameter, Expression.Parameter(source.Type.GetSequenceType()), innerParameter);

            return this.TranslateSelectMany(source, selector, resultSelector);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateSingleOrDefault(
            ShapedQueryExpression source,
            LambdaExpression? predicate,
            Type returnType,
            bool returnDefault)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(returnType, nameof(returnType));

            return this.TranslateSingleResultOperator(
                source,
                predicate,
                returnType,
                returnDefault
                    ? EnumerableMethods.SingleOrDefaultWithoutPredicate
                    : EnumerableMethods.SingleWithoutPredicate);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateSkip(ShapedQueryExpression source, Expression count)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(count, nameof(count));

            var brightChainQueryExpression = (BrightChainQueryExpression)source.QueryExpression;
            var newCount = this.TranslateExpression(count);
            if (newCount == null)
            {
                return null;
            }
            count = newCount;

            brightChainQueryExpression.UpdateServerQueryExpression(
                Expression.Call(
                    EnumerableMethods.Skip.MakeGenericMethod(brightChainQueryExpression.CurrentParameter.Type),
                    brightChainQueryExpression.ServerQueryExpression,
                    count));

            return source;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateSkipWhile(ShapedQueryExpression source, LambdaExpression predicate)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(predicate, nameof(predicate));

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateSum(ShapedQueryExpression source, LambdaExpression? selector, Type resultType)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(resultType, nameof(resultType));

            return this.TranslateScalarAggregate(source, selector, nameof(Enumerable.Sum), resultType);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateTake(ShapedQueryExpression source, Expression count)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(count, nameof(count));

            var brightChainQueryExpression = (BrightChainQueryExpression)source.QueryExpression;
            var newCount = this.TranslateExpression(count);
            if (newCount == null)
            {
                return null;
            }
            count = newCount;

            brightChainQueryExpression.UpdateServerQueryExpression(
                Expression.Call(
                    EnumerableMethods.Take.MakeGenericMethod(brightChainQueryExpression.CurrentParameter.Type),
                    brightChainQueryExpression.ServerQueryExpression,
                    count));

            return source;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateTakeWhile(ShapedQueryExpression source, LambdaExpression predicate)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(predicate, nameof(predicate));

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateThenBy(ShapedQueryExpression source, LambdaExpression keySelector, bool ascending)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(keySelector, nameof(keySelector));

            var brightChainQueryExpression = (BrightChainQueryExpression)source.QueryExpression;
            var newKeySelector = this.TranslateLambdaExpression(source, keySelector);
            if (newKeySelector == null)
            {
                return null;
            }
            keySelector = newKeySelector;

            brightChainQueryExpression.UpdateServerQueryExpression(
                Expression.Call(
                    (ascending ? EnumerableMethods.ThenBy : EnumerableMethods.ThenByDescending)
                    .MakeGenericMethod(brightChainQueryExpression.CurrentParameter.Type, keySelector.ReturnType),
                    brightChainQueryExpression.ServerQueryExpression,
                    keySelector));

            return source;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateUnion(ShapedQueryExpression source1, ShapedQueryExpression source2)
        {
            Check.NotNull(source1, nameof(source1));
            Check.NotNull(source2, nameof(source2));

            return this.TranslateSetOperation(EnumerableMethods.Union, source1, source2);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ShapedQueryExpression? TranslateWhere(ShapedQueryExpression source, LambdaExpression predicate)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(predicate, nameof(predicate));

            var brightChainQueryExpression = (BrightChainQueryExpression)source.QueryExpression;
            var newPredicate = this.TranslateLambdaExpression(source, predicate, preserveType: true);
            if (newPredicate == null)
            {
                return null;
            }
            predicate = newPredicate;

            brightChainQueryExpression.UpdateServerQueryExpression(
                Expression.Call(
                    EnumerableMethods.Where.MakeGenericMethod(brightChainQueryExpression.CurrentParameter.Type),
                    brightChainQueryExpression.ServerQueryExpression,
                    predicate));

            return source;
        }

        private Expression? TranslateExpression(Expression expression, bool preserveType = false)
        {
            var translation = this._expressionTranslator.Translate(expression);
            if (translation == null && this._expressionTranslator.TranslationErrorDetails != null)
            {
                this.AddTranslationErrorDetails(this._expressionTranslator.TranslationErrorDetails);
            }

            if (expression != null
                && translation != null
                && preserveType
                && expression.Type != translation.Type)
            {
                translation = expression.Type == typeof(bool)
                    ? Expression.Equal(translation, Expression.Constant(true, translation.Type))
                    : Expression.Convert(translation, expression.Type);
            }

            return translation;
        }

        private LambdaExpression? TranslateLambdaExpression(
            ShapedQueryExpression shapedQueryExpression,
            LambdaExpression lambdaExpression,
            bool preserveType = false)
        {
            var lambdaBody = this.TranslateExpression(this.RemapLambdaBody(shapedQueryExpression, lambdaExpression), preserveType);

            return lambdaBody != null
                ? Expression.Lambda(
                    lambdaBody,
                    ((BrightChainQueryExpression)shapedQueryExpression.QueryExpression).CurrentParameter)
                : null;
        }

        private Expression RemapLambdaBody(ShapedQueryExpression shapedQueryExpression, LambdaExpression lambdaExpression)
        {
            var lambdaBody = ReplacingExpressionVisitor.Replace(
                lambdaExpression.Parameters.Single(), shapedQueryExpression.ShaperExpression, lambdaExpression.Body);

            return this.ExpandWeakEntities((BrightChainQueryExpression)shapedQueryExpression.QueryExpression, lambdaBody);
        }

        internal Expression ExpandWeakEntities(BrightChainQueryExpression queryExpression, Expression lambdaBody)
            => this._weakEntityExpandingExpressionVisitor.Expand(queryExpression, lambdaBody);

        private sealed class WeakEntityExpandingExpressionVisitor : ExpressionVisitor
        {
            private static readonly MethodInfo _objectEqualsMethodInfo
                = typeof(object).GetRequiredRuntimeMethod(nameof(object.Equals), new[] { typeof(object), typeof(object) });

            private readonly BrightChainExpressionTranslatingExpressionVisitor _expressionTranslator;

            private BrightChainQueryExpression _queryExpression;

            public WeakEntityExpandingExpressionVisitor(BrightChainExpressionTranslatingExpressionVisitor expressionTranslator)
            {
                this._expressionTranslator = expressionTranslator;
                this._queryExpression = null!;
            }

            public string? TranslationErrorDetails
                => this._expressionTranslator.TranslationErrorDetails;

            public Expression Expand(BrightChainQueryExpression queryExpression, Expression lambdaBody)
            {
                this._queryExpression = queryExpression;

                return this.Visit(lambdaBody);
            }

            protected override Expression VisitMember(MemberExpression memberExpression)
            {
                Check.NotNull(memberExpression, nameof(memberExpression));

                var innerExpression = this.Visit(memberExpression.Expression);

                return this.TryExpand(innerExpression, MemberIdentity.Create(memberExpression.Member))
                    ?? memberExpression.Update(innerExpression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                Check.NotNull(methodCallExpression, nameof(methodCallExpression));

                if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var navigationName))
                {
                    source = this.Visit(source);

                    return this.TryExpand(source, MemberIdentity.Create(navigationName))
                        ?? methodCallExpression.Update(null!, new[] { source, methodCallExpression.Arguments[1] });
                }

                if (methodCallExpression.TryGetEFPropertyArguments(out source, out navigationName))
                {
                    source = this.Visit(source);

                    return this.TryExpand(source, MemberIdentity.Create(navigationName))
                        ?? methodCallExpression.Update(source, new[] { methodCallExpression.Arguments[0] });
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                Check.NotNull(extensionExpression, nameof(extensionExpression));

                return extensionExpression is EntityShaperExpression
                    || extensionExpression is ShapedQueryExpression
                    ? extensionExpression
                    : base.VisitExtension(extensionExpression);
            }

            private Expression? TryExpand(Expression? source, MemberIdentity member)
            {
                source = source.UnwrapTypeConversion(out var convertedType);
                if (source is not EntityShaperExpression entityShaperExpression)
                {
                    return null;
                }

                var entityType = entityShaperExpression.EntityType;
                if (convertedType != null)
                {
                    entityType = entityType.GetRootType().GetDerivedTypesInclusive()
                        .FirstOrDefault(et => et.ClrType == convertedType);

                    if (entityType == null)
                    {
                        return null;
                    }
                }

                var navigation = member.MemberInfo != null
                    ? entityType.FindNavigation(member.MemberInfo)
                    : entityType.FindNavigation(member.Name!);

                if (navigation == null)
                {
                    return null;
                }

                var targetEntityType = navigation.TargetEntityType;
                if (targetEntityType == null
                    || !targetEntityType.IsOwned())
                {
                    return null;
                }

                var foreignKey = navigation.ForeignKey;
                if (navigation.IsCollection)
                {
                    var innerShapedQuery = CreateShapedQueryExpressionStatic(targetEntityType);
                    var innerQueryExpression = (BrightChainQueryExpression)innerShapedQuery.QueryExpression;

                    var makeNullable = foreignKey.PrincipalKey.Properties
                        .Concat(foreignKey.Properties)
                        .Select(p => p.ClrType)
                        .Any(t => t.IsNullableType());

                    var outerKey = entityShaperExpression.CreateKeyValuesExpression(
                        navigation.IsOnDependent
                            ? foreignKey.Properties
                            : foreignKey.PrincipalKey.Properties,
                        makeNullable);
                    var innerKey = innerShapedQuery.ShaperExpression.CreateKeyValuesExpression(
                        navigation.IsOnDependent
                            ? foreignKey.PrincipalKey.Properties
                            : foreignKey.Properties,
                        makeNullable);

                    var keyComparison = Expression.Call(_objectEqualsMethodInfo, AddConvertToObject(outerKey), AddConvertToObject(innerKey));

                    var predicate = makeNullable
                        ? Expression.AndAlso(
                            outerKey is NewArrayExpression newArrayExpression
                                ? newArrayExpression.Expressions
                                    .Select(
                                        e =>
                                        {
                                            var left = (e as UnaryExpression)?.Operand ?? e;

                                            return Expression.NotEqual(left, Expression.Constant(null, left.Type));
                                        })
                                    .Aggregate((l, r) => Expression.AndAlso(l, r))
                                : Expression.NotEqual(outerKey, Expression.Constant(null, outerKey.Type)),
                            keyComparison)
                        : (Expression)keyComparison;

                    var correlationPredicate = this._expressionTranslator.Translate(predicate)!;
                    innerQueryExpression.UpdateServerQueryExpression(
                        Expression.Call(
                            EnumerableMethods.Where.MakeGenericMethod(innerQueryExpression.CurrentParameter.Type),
                            innerQueryExpression.ServerQueryExpression,
                            Expression.Lambda(correlationPredicate, innerQueryExpression.CurrentParameter)));

                    return innerShapedQuery;
                }

                var entityProjectionExpression = entityShaperExpression.ValueBufferExpression is ProjectionBindingExpression projectionBindingExpression
                    ? (EntityProjectionExpression)this._queryExpression.GetProjection(projectionBindingExpression)
                    : (EntityProjectionExpression)entityShaperExpression.ValueBufferExpression;
                var innerShaper = entityProjectionExpression.BindNavigation(navigation);
                if (innerShaper == null)
                {
                    var innerShapedQuery = CreateShapedQueryExpressionStatic(targetEntityType);
                    var innerQueryExpression = (BrightChainQueryExpression)innerShapedQuery.QueryExpression;

                    var makeNullable = foreignKey.PrincipalKey.Properties
                        .Concat(foreignKey.Properties)
                        .Select(p => p.ClrType)
                        .Any(t => t.IsNullableType());

                    var outerKey = entityShaperExpression.CreateKeyValuesExpression(
                        navigation.IsOnDependent
                            ? foreignKey.Properties
                            : foreignKey.PrincipalKey.Properties,
                        makeNullable);
                    var innerKey = innerShapedQuery.ShaperExpression.CreateKeyValuesExpression(
                        navigation.IsOnDependent
                            ? foreignKey.PrincipalKey.Properties
                            : foreignKey.Properties,
                        makeNullable);

                    if (foreignKey.Properties.Count > 1)
                    {
                        outerKey = Expression.New(AnonymousObject.AnonymousObjectCtor, outerKey);
                        innerKey = Expression.New(AnonymousObject.AnonymousObjectCtor, innerKey);
                    }

                    var outerKeySelector = Expression.Lambda(this._expressionTranslator.Translate(outerKey)!, this._queryExpression.CurrentParameter);
                    var innerKeySelector = Expression.Lambda(
                        this._expressionTranslator.Translate(innerKey)!, innerQueryExpression.CurrentParameter);
                    (outerKeySelector, innerKeySelector) = AlignKeySelectorTypes(outerKeySelector, innerKeySelector);
                    innerShaper = this._queryExpression.AddNavigationToWeakEntityType(
                        entityProjectionExpression, navigation, innerQueryExpression, outerKeySelector, innerKeySelector);
                }

                return innerShaper;
            }

            private static Expression AddConvertToObject(Expression expression)
                => expression.Type.IsValueType
                    ? Expression.Convert(expression, typeof(object))
                    : expression;
        }

        private ShapedQueryExpression TranslateTwoParameterSelector(ShapedQueryExpression source, LambdaExpression resultSelector)
        {
            var transparentIdentifierType = source.ShaperExpression.Type;
            var transparentIdentifierParameter = Expression.Parameter(transparentIdentifierType);

            Expression original1 = resultSelector.Parameters[0];
            var replacement1 = AccessField(transparentIdentifierType, transparentIdentifierParameter, "Outer");
            Expression original2 = resultSelector.Parameters[1];
            var replacement2 = AccessField(transparentIdentifierType, transparentIdentifierParameter, "Inner");
            var newResultSelector = Expression.Lambda(
                new ReplacingExpressionVisitor(
                        new[] { original1, original2 }, new[] { replacement1, replacement2 })
                    .Visit(resultSelector.Body),
                transparentIdentifierParameter);

            return this.TranslateSelect(source, newResultSelector);
        }

        private static Expression AccessField(
            Type transparentIdentifierType,
            Expression targetExpression,
            string fieldName)
            => Expression.Field(targetExpression, transparentIdentifierType.GetRequiredDeclaredField(fieldName));

        private ShapedQueryExpression? TranslateScalarAggregate(
            ShapedQueryExpression source,
            LambdaExpression? selector,
            string methodName,
            Type returnType)
        {
            var brightChainQueryExpression = (BrightChainQueryExpression)source.QueryExpression;

            selector = selector == null
                || selector.Body == selector.Parameters[0]
                    ? Expression.Lambda(
                        brightChainQueryExpression.GetProjection(new ProjectionBindingExpression(
                            brightChainQueryExpression, new ProjectionMember(), returnType)),
                        brightChainQueryExpression.CurrentParameter)
                    : this.TranslateLambdaExpression(source, selector, preserveType: true);

            if (selector == null)
            {
                return null;
            }

            var method = GetMethod();
            method = method.GetGenericArguments().Length == 2
                ? method.MakeGenericMethod(typeof(ValueBuffer), selector.ReturnType)
                : method.MakeGenericMethod(typeof(ValueBuffer));

            brightChainQueryExpression.UpdateServerQueryExpression(
                Expression.Call(method, brightChainQueryExpression.ServerQueryExpression, selector));

            return source.UpdateShaperExpression(Expression.Convert(brightChainQueryExpression.GetSingleScalarProjection(), returnType));

            MethodInfo GetMethod()
                => methodName switch
                {
                    nameof(Enumerable.Average) => EnumerableMethods.GetAverageWithSelector(selector.ReturnType),
                    nameof(Enumerable.Max) => EnumerableMethods.GetMaxWithSelector(selector.ReturnType),
                    nameof(Enumerable.Min) => EnumerableMethods.GetMinWithSelector(selector.ReturnType),
                    nameof(Enumerable.Sum) => EnumerableMethods.GetSumWithSelector(selector.ReturnType),
                    _ => throw new InvalidOperationException(CoreStrings.UnknownEntity("Aggregate Operator")),
                };
        }

        private ShapedQueryExpression? TranslateSingleResultOperator(
            ShapedQueryExpression source,
            LambdaExpression? predicate,
            Type returnType,
            MethodInfo method)
        {
            var brightChainQueryExpression = (BrightChainQueryExpression)source.QueryExpression;

            if (predicate != null)
            {
                var newSource = this.TranslateWhere(source, predicate);
                if (newSource == null)
                {
                    return null;
                }
                source = newSource;
            }

            brightChainQueryExpression.ConvertToSingleResult(method);

            return source.ShaperExpression.Type != returnType
                ? source.UpdateShaperExpression(Expression.Convert(source.ShaperExpression, returnType))
                : source;
        }

        private ShapedQueryExpression TranslateSetOperation(
            MethodInfo setOperationMethodInfo,
            ShapedQueryExpression source1,
            ShapedQueryExpression source2)
        {
            var brightChainQueryExpression1 = (BrightChainQueryExpression)source1.QueryExpression;
            var brightChainQueryExpression2 = (BrightChainQueryExpression)source2.QueryExpression;

            brightChainQueryExpression1.ApplySetOperation(setOperationMethodInfo, brightChainQueryExpression2);

            if (setOperationMethodInfo.Equals(EnumerableMethods.Except))
            {
                return source1;
            }

            var makeNullable = setOperationMethodInfo != EnumerableMethods.Intersect;

            return source1.UpdateShaperExpression(this.MatchShaperNullabilityForSetOperation(
                source1.ShaperExpression, source2.ShaperExpression, makeNullable));
        }

        private Expression MatchShaperNullabilityForSetOperation(Expression shaper1, Expression shaper2, bool makeNullable)
        {
            switch (shaper1)
            {
                case EntityShaperExpression entityShaperExpression1
                when shaper2 is EntityShaperExpression entityShaperExpression2:
                    return entityShaperExpression1.IsNullable != entityShaperExpression2.IsNullable
                        ? entityShaperExpression1.MakeNullable(makeNullable)
                        : entityShaperExpression1;

                case NewExpression newExpression1
                when shaper2 is NewExpression newExpression2:
                    var newArguments = new Expression[newExpression1.Arguments.Count];
                    for (var i = 0; i < newArguments.Length; i++)
                    {
                        newArguments[i] = this.MatchShaperNullabilityForSetOperation(
                            newExpression1.Arguments[i], newExpression2.Arguments[i], makeNullable);
                    }

                    return newExpression1.Update(newArguments);

                case MemberInitExpression memberInitExpression1
                when shaper2 is MemberInitExpression memberInitExpression2:
                    var newExpression = (NewExpression)this.MatchShaperNullabilityForSetOperation(
                        memberInitExpression1.NewExpression, memberInitExpression2.NewExpression, makeNullable);

                    var memberBindings = new MemberBinding[memberInitExpression1.Bindings.Count];
                    for (var i = 0; i < memberBindings.Length; i++)
                    {
                        var memberAssignment = memberInitExpression1.Bindings[i] as MemberAssignment;
                        Check.DebugAssert(memberAssignment != null, "Only member assignment bindings are supported");


                        memberBindings[i] = memberAssignment.Update(this.MatchShaperNullabilityForSetOperation(
                            memberAssignment.Expression, ((MemberAssignment)memberInitExpression2.Bindings[i]).Expression, makeNullable));
                    }

                    return memberInitExpression1.Update(newExpression, memberBindings);

                default:
                    return shaper1;
            }
        }
    }
}
