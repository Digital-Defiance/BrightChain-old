// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json.Nodes;
using BrightChain.EntityFrameworkCore.Properties;
using BrightChain.EntityFrameworkCore.Storage.Internal;
using BrightChain.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

#nullable disable

namespace BrightChain.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class QuerySqlGenerator : SqlExpressionVisitor
    {
        private readonly StringBuilder _sqlBuilder = new();
        private IReadOnlyDictionary<string, object> _parameterValues;
        private List<SqlParameter> _sqlParameters;
        private bool _useValueProjection;

        private readonly IDictionary<ExpressionType, string> _operatorMap = new Dictionary<ExpressionType, string>
        {
            // Arithmetic
            { ExpressionType.Add, " + " },
            { ExpressionType.Subtract, " - " },
            { ExpressionType.Multiply, " * " },
            { ExpressionType.Divide, " / " },
            { ExpressionType.Modulo, " % " },

            // Bitwise >>> (zero-fill right shift) not available in C#
            { ExpressionType.Or, " | " },
            { ExpressionType.And, " & " },
            { ExpressionType.ExclusiveOr, " ^ " },
            { ExpressionType.LeftShift, " << " },
            { ExpressionType.RightShift, " >> " },

            // Logical
            { ExpressionType.AndAlso, " AND " },
            { ExpressionType.OrElse, " OR " },

            // Comparison
            { ExpressionType.Equal, " = " },
            { ExpressionType.NotEqual, " != " },
            { ExpressionType.GreaterThan, " > " },
            { ExpressionType.GreaterThanOrEqual, " >= " },
            { ExpressionType.LessThan, " < " },
            { ExpressionType.LessThanOrEqual, " <= " },

            // Unary
            { ExpressionType.UnaryPlus, "+" },
            { ExpressionType.Negate, "-" },
            { ExpressionType.Not, "~" }
        };

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainSqlQuery GetSqlQuery(
            SelectExpression selectExpression,
            IReadOnlyDictionary<string, object> parameterValues)
        {
            this._sqlBuilder.Clear();
            this._parameterValues = parameterValues;
            this._sqlParameters = new List<SqlParameter>();

            this.Visit(selectExpression);

            return new BrightChainSqlQuery(this._sqlBuilder.ToString(), this._sqlParameters);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitEntityProjection(EntityProjectionExpression entityProjectionExpression)
        {
            Check.NotNull(entityProjectionExpression, nameof(entityProjectionExpression));

            this.Visit(entityProjectionExpression.AccessExpression);

            return entityProjectionExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitObjectArrayProjection(ObjectArrayProjectionExpression objectArrayProjectionExpression)
        {
            Check.NotNull(objectArrayProjectionExpression, nameof(objectArrayProjectionExpression));

            this._sqlBuilder.Append(objectArrayProjectionExpression);

            return objectArrayProjectionExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitKeyAccess(KeyAccessExpression keyAccessExpression)
        {
            Check.NotNull(keyAccessExpression, nameof(keyAccessExpression));

            this._sqlBuilder.Append(keyAccessExpression);

            return keyAccessExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitObjectAccess(ObjectAccessExpression objectAccessExpression)
        {
            Check.NotNull(objectAccessExpression, nameof(objectAccessExpression));

            this._sqlBuilder.Append(objectAccessExpression);

            return objectAccessExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitProjection(ProjectionExpression projectionExpression)
        {
            Check.NotNull(projectionExpression, nameof(projectionExpression));

            if (this._useValueProjection)
            {
                this._sqlBuilder.Append('"').Append(projectionExpression.Alias).Append("\" : ");
            }

            this.Visit(projectionExpression.Expression);

            if (!this._useValueProjection
                && !string.IsNullOrEmpty(projectionExpression.Alias)
                && projectionExpression.Alias != projectionExpression.Name)
            {
                this._sqlBuilder.Append(" AS " + projectionExpression.Alias);
            }

            return projectionExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitRootReference(RootReferenceExpression rootReferenceExpression)
        {
            Check.NotNull(rootReferenceExpression, nameof(rootReferenceExpression));

            this._sqlBuilder.Append(rootReferenceExpression);

            return rootReferenceExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitSelect(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            this._sqlBuilder.Append("SELECT ");

            if (selectExpression.IsDistinct)
            {
                this._sqlBuilder.Append("DISTINCT ");
            }

            if (selectExpression.Projection.Count > 0)
            {
                if (selectExpression.Projection.Any(p => !string.IsNullOrEmpty(p.Alias) && p.Alias != p.Name)
                    && !selectExpression.Projection.Any(p => p.Expression is SqlFunctionExpression)) // Aggregates are not allowed
                {
                    this._useValueProjection = true;
                    this._sqlBuilder.Append("VALUE {");
                    this.GenerateList(selectExpression.Projection, e => this.Visit(e));
                    this._sqlBuilder.Append('}');
                    this._useValueProjection = false;
                }
                else
                {
                    this.GenerateList(selectExpression.Projection, e => this.Visit(e));
                }
            }
            else
            {
                this._sqlBuilder.Append('1');
            }

            this._sqlBuilder.AppendLine();

            this._sqlBuilder.Append("FROM root ");
            this.Visit(selectExpression.FromExpression);
            this._sqlBuilder.AppendLine();

            if (selectExpression.Predicate != null)
            {
                this._sqlBuilder.Append("WHERE ");
                this.Visit(selectExpression.Predicate);
            }

            if (selectExpression.Orderings.Any())
            {
                this._sqlBuilder.AppendLine().Append("ORDER BY ");

                this.GenerateList(selectExpression.Orderings, e => this.Visit(e));
            }

            if (selectExpression.Offset != null
                || selectExpression.Limit != null)
            {
                this._sqlBuilder.AppendLine().Append("OFFSET ");

                if (selectExpression.Offset != null)
                {
                    this.Visit(selectExpression.Offset);
                }
                else
                {
                    this._sqlBuilder.Append('0');
                }

                this._sqlBuilder.Append(" LIMIT ");

                if (selectExpression.Limit != null)
                {
                    this.Visit(selectExpression.Limit);
                }
                else
                {
                    // TODO: See Issue#18923
                    throw new InvalidOperationException(BrightChainStrings.OffsetRequiresLimit);
                }
            }

            return selectExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitOrdering(OrderingExpression orderingExpression)
        {
            Check.NotNull(orderingExpression, nameof(orderingExpression));

            this.Visit(orderingExpression.Expression);

            if (!orderingExpression.IsAscending)
            {
                this._sqlBuilder.Append(" DESC");
            }

            return orderingExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression)
        {
            Check.NotNull(sqlBinaryExpression, nameof(sqlBinaryExpression));

            var op = this._operatorMap[sqlBinaryExpression.OperatorType];
            this._sqlBuilder.Append('(');
            this.Visit(sqlBinaryExpression.Left);

            if (sqlBinaryExpression.OperatorType == ExpressionType.Add
                && sqlBinaryExpression.Left.Type == typeof(string))
            {
                op = " || ";
            }

            this._sqlBuilder.Append(op);

            this.Visit(sqlBinaryExpression.Right);
            this._sqlBuilder.Append(')');

            return sqlBinaryExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression)
        {
            Check.NotNull(sqlUnaryExpression, nameof(sqlUnaryExpression));

            var op = this._operatorMap[sqlUnaryExpression.OperatorType];

            if (sqlUnaryExpression.OperatorType == ExpressionType.Not
                && sqlUnaryExpression.Operand.Type == typeof(bool))
            {
                op = "NOT";
            }

            this._sqlBuilder.Append(op);

            this._sqlBuilder.Append('(');
            this.Visit(sqlUnaryExpression.Operand);
            this._sqlBuilder.Append(')');

            return sqlUnaryExpression;
        }

        private void GenerateList<T>(
            IReadOnlyList<T> items,
            Action<T> generationAction,
            Action<StringBuilder> joinAction = null)
        {
            joinAction ??= (isb => isb.Append(", "));

            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    joinAction(this._sqlBuilder);
                }

                generationAction(items[i]);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitSqlConstant(SqlConstantExpression sqlConstantExpression)
        {
            Check.NotNull(sqlConstantExpression, nameof(sqlConstantExpression));

            var jToken = this.GenerateJToken(sqlConstantExpression.Value, sqlConstantExpression.TypeMapping);

            this._sqlBuilder.Append(jToken == null ? "null" : jToken.ToString());

            return sqlConstantExpression;
        }

        private JsonNode GenerateJToken(object value, CoreTypeMapping typeMapping)
        {
            if (value?.GetType().IsInteger() == true)
            {
                var unwrappedType = typeMapping.ClrType.UnwrapNullableType();
                value = unwrappedType.IsEnum
                    ? Enum.ToObject(unwrappedType, value)
                    : unwrappedType == typeof(char)
                        ? Convert.ChangeType(value, unwrappedType)
                        : value;
            }

            var converter = typeMapping.Converter;
            if (converter != null)
            {
                value = converter.ConvertToProvider(value);
            }

            return (JsonNode)(value == null
                ? null
                : (value as JsonNode) ?? value);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitSqlConditional(SqlConditionalExpression sqlConditionalExpression)
        {
            Check.NotNull(sqlConditionalExpression, nameof(sqlConditionalExpression));

            this._sqlBuilder.Append('(');
            this.Visit(sqlConditionalExpression.Test);
            this._sqlBuilder.Append(" ? ");
            this.Visit(sqlConditionalExpression.IfTrue);
            this._sqlBuilder.Append(" : ");
            this.Visit(sqlConditionalExpression.IfFalse);
            this._sqlBuilder.Append(')');

            return sqlConditionalExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitSqlParameter(SqlParameterExpression sqlParameterExpression)
        {
            Check.NotNull(sqlParameterExpression, nameof(sqlParameterExpression));

            var parameterName = $"@{sqlParameterExpression.Name}";

            if (this._sqlParameters.All(sp => sp.Name != parameterName))
            {
                var jToken = this.GenerateJToken(this._parameterValues[sqlParameterExpression.Name], sqlParameterExpression.TypeMapping);
                this._sqlParameters.Add(new SqlParameter(parameterName, jToken));
            }

            this._sqlBuilder.Append(parameterName);

            return sqlParameterExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitIn(InExpression inExpression)
        {
            Check.NotNull(inExpression, nameof(inExpression));

            this.Visit(inExpression.Item);
            this._sqlBuilder.Append(inExpression.IsNegated ? " NOT IN " : " IN ");
            this._sqlBuilder.Append('(');
            var valuesConstant = (SqlConstantExpression)inExpression.Values;
            var valuesList = ((IEnumerable<object>)valuesConstant.Value)
                .Select(v => new SqlConstantExpression(Expression.Constant(v), valuesConstant.TypeMapping)).ToList();
            this.GenerateList(valuesList, e => this.Visit(e));
            this._sqlBuilder.Append(')');

            return inExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            Check.NotNull(sqlFunctionExpression, nameof(sqlFunctionExpression));

            this._sqlBuilder.Append(sqlFunctionExpression.Name);
            this._sqlBuilder.Append('(');
            this.GenerateList(sqlFunctionExpression.Arguments, e => this.Visit(e));
            this._sqlBuilder.Append(')');

            return sqlFunctionExpression;
        }
    }
}
