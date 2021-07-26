// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using BrightChain.EntityFrameworkCore.Internal;
using BrightChain.EntityFrameworkCore.Properties;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

#nullable disable

namespace BrightChain.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqlExpressionFactory : ISqlExpressionFactory
    {
        private readonly ITypeMappingSource _typeMappingSource;
        private readonly CoreTypeMapping _boolTypeMapping;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlExpressionFactory(ITypeMappingSource typeMappingSource)
        {
            this._typeMappingSource = typeMappingSource;
            this._boolTypeMapping = typeMappingSource.FindMapping(typeof(bool));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression ApplyDefaultTypeMapping(SqlExpression sqlExpression)
        {
            return sqlExpression == null
                || sqlExpression.TypeMapping != null
                    ? sqlExpression
                    : this.ApplyTypeMapping(sqlExpression, this._typeMappingSource.FindMapping(sqlExpression.Type));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression ApplyTypeMapping(SqlExpression sqlExpression, CoreTypeMapping typeMapping)
        {
            if (sqlExpression == null
                || sqlExpression.TypeMapping != null)
            {
                return sqlExpression;
            }

#pragma warning disable IDE0066 // Convert switch statement to expression
            switch (sqlExpression)
#pragma warning restore IDE0066 // Convert switch statement to expression
            {
                case SqlConditionalExpression sqlConditionalExpression:
                    return this.ApplyTypeMappingOnSqlConditional(sqlConditionalExpression, typeMapping);

                case SqlBinaryExpression sqlBinaryExpression:
                    return this.ApplyTypeMappingOnSqlBinary(sqlBinaryExpression, typeMapping);

                case SqlUnaryExpression sqlUnaryExpression:
                    return this.ApplyTypeMappingOnSqlUnary(sqlUnaryExpression, typeMapping);

                case SqlConstantExpression sqlConstantExpression:
                    return sqlConstantExpression.ApplyTypeMapping(typeMapping);

                case SqlParameterExpression sqlParameterExpression:
                    return sqlParameterExpression.ApplyTypeMapping(typeMapping);

                case SqlFunctionExpression sqlFunctionExpression:
                    return sqlFunctionExpression.ApplyTypeMapping(typeMapping);

                default:
                    return sqlExpression;
            }
        }

        private SqlExpression ApplyTypeMappingOnSqlConditional(
            SqlConditionalExpression sqlConditionalExpression,
            CoreTypeMapping typeMapping)
        {
            return sqlConditionalExpression.Update(
                sqlConditionalExpression.Test,
                this.ApplyTypeMapping(sqlConditionalExpression.IfTrue, typeMapping),
                this.ApplyTypeMapping(sqlConditionalExpression.IfFalse, typeMapping));
        }

        private SqlExpression ApplyTypeMappingOnSqlUnary(
            SqlUnaryExpression sqlUnaryExpression,
            CoreTypeMapping typeMapping)
        {
            SqlExpression operand;
            Type resultType;
            CoreTypeMapping resultTypeMapping;
            switch (sqlUnaryExpression.OperatorType)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Not
                    when sqlUnaryExpression.IsLogicalNot():
                {
                    resultTypeMapping = this._boolTypeMapping;
                    resultType = typeof(bool);
                    operand = this.ApplyDefaultTypeMapping(sqlUnaryExpression.Operand);
                    break;
                }

                case ExpressionType.Convert:
                    resultTypeMapping = typeMapping;
                    // Since we are applying convert, resultTypeMapping decides the clrType
                    resultType = resultTypeMapping?.ClrType ?? sqlUnaryExpression.Type;
                    operand = this.ApplyDefaultTypeMapping(sqlUnaryExpression.Operand);
                    break;

                case ExpressionType.Not:
                case ExpressionType.Negate:
                    resultTypeMapping = typeMapping;
                    // While Not is logical, negate is numeric hence we use clrType from TypeMapping
                    resultType = resultTypeMapping?.ClrType ?? sqlUnaryExpression.Type;
                    operand = this.ApplyTypeMapping(sqlUnaryExpression.Operand, typeMapping);
                    break;

                default:
                    throw new InvalidOperationException(
                        BrightChainStrings.UnsupportedOperatorForSqlExpression(
                            sqlUnaryExpression.OperatorType, typeof(SqlUnaryExpression).ShortDisplayName()));
            }

            return new SqlUnaryExpression(sqlUnaryExpression.OperatorType, operand, resultType, resultTypeMapping);
        }

        private SqlExpression ApplyTypeMappingOnSqlBinary(
            SqlBinaryExpression sqlBinaryExpression,
            CoreTypeMapping typeMapping)
        {
            var left = sqlBinaryExpression.Left;
            var right = sqlBinaryExpression.Right;

            Type resultType;
            CoreTypeMapping resultTypeMapping;
            CoreTypeMapping inferredTypeMapping;
            switch (sqlBinaryExpression.OperatorType)
            {
                case ExpressionType.Equal:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.NotEqual:
                {
                    inferredTypeMapping = ExpressionExtensions.InferTypeMapping(left, right)
                        // We avoid object here since the result does not get typeMapping from outside.
                        ?? (left.Type != typeof(object)
                            ? this._typeMappingSource.FindMapping(left.Type)
                            : this._typeMappingSource.FindMapping(right.Type));
                    resultType = typeof(bool);
                    resultTypeMapping = this._boolTypeMapping;
                }
                break;

                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                {
                    inferredTypeMapping = this._boolTypeMapping;
                    resultType = typeof(bool);
                    resultTypeMapping = this._boolTypeMapping;
                }
                break;

                case ExpressionType.Add:
                case ExpressionType.Subtract:
                case ExpressionType.Multiply:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.LeftShift:
                case ExpressionType.RightShift:
                case ExpressionType.And:
                case ExpressionType.Or:
                {
                    inferredTypeMapping = typeMapping ?? ExpressionExtensions.InferTypeMapping(left, right);
                    resultType = inferredTypeMapping?.ClrType ?? left.Type;
                    resultTypeMapping = inferredTypeMapping;
                }
                break;

                default:
                    throw new InvalidOperationException(
                        BrightChainStrings.UnsupportedOperatorForSqlExpression(
                            sqlBinaryExpression.OperatorType, typeof(SqlBinaryExpression).ShortDisplayName()));
            }

            return new SqlBinaryExpression(
                sqlBinaryExpression.OperatorType,
                this.ApplyTypeMapping(left, inferredTypeMapping),
                this.ApplyTypeMapping(right, inferredTypeMapping),
                resultType,
                resultTypeMapping);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual CoreTypeMapping FindMapping(Type type)
        {
            return this._typeMappingSource.FindMapping(type);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression MakeBinary(
            ExpressionType operatorType,
            SqlExpression left,
            SqlExpression right,
            CoreTypeMapping typeMapping)
        {
            var returnType = left.Type;
            switch (operatorType)
            {
                case ExpressionType.Equal:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.NotEqual:
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    returnType = typeof(bool);
                    break;
            }

            return (SqlBinaryExpression)this.ApplyTypeMapping(
                new SqlBinaryExpression(operatorType, left, right, returnType, null), typeMapping);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression Equal(SqlExpression left, SqlExpression right)
        {
            return this.MakeBinary(ExpressionType.Equal, left, right, null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression NotEqual(SqlExpression left, SqlExpression right)
        {
            return this.MakeBinary(ExpressionType.NotEqual, left, right, null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression GreaterThan(SqlExpression left, SqlExpression right)
        {
            return this.MakeBinary(ExpressionType.GreaterThan, left, right, null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression GreaterThanOrEqual(SqlExpression left, SqlExpression right)
        {
            return this.MakeBinary(ExpressionType.GreaterThanOrEqual, left, right, null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression LessThan(SqlExpression left, SqlExpression right)
        {
            return this.MakeBinary(ExpressionType.LessThan, left, right, null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression LessThanOrEqual(SqlExpression left, SqlExpression right)
        {
            return this.MakeBinary(ExpressionType.LessThanOrEqual, left, right, null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression AndAlso(SqlExpression left, SqlExpression right)
        {
            return this.MakeBinary(ExpressionType.AndAlso, left, right, null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression OrElse(SqlExpression left, SqlExpression right)
        {
            return this.MakeBinary(ExpressionType.OrElse, left, right, null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression Add(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null)
        {
            return this.MakeBinary(ExpressionType.Add, left, right, typeMapping);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression Subtract(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null)
        {
            return this.MakeBinary(ExpressionType.Subtract, left, right, typeMapping);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression Multiply(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null)
        {
            return this.MakeBinary(ExpressionType.Multiply, left, right, typeMapping);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression Divide(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null)
        {
            return this.MakeBinary(ExpressionType.Divide, left, right, typeMapping);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression Modulo(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null)
        {
            return this.MakeBinary(ExpressionType.Modulo, left, right, typeMapping);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression And(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null)
        {
            return this.MakeBinary(ExpressionType.And, left, right, typeMapping);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression Or(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null)
        {
            return this.MakeBinary(ExpressionType.Or, left, right, typeMapping);
        }

        private SqlUnaryExpression MakeUnary(
            ExpressionType operatorType,
            SqlExpression operand,
            Type type,
            CoreTypeMapping typeMapping = null)
        {
            return (SqlUnaryExpression)this.ApplyTypeMapping(new SqlUnaryExpression(operatorType, operand, type, null), typeMapping);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression IsNull(SqlExpression operand)
        {
            return this.Equal(operand, this.Constant(null));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression IsNotNull(SqlExpression operand)
        {
            return this.NotEqual(operand, this.Constant(null));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlUnaryExpression Convert(SqlExpression operand, Type type, CoreTypeMapping typeMapping = null)
        {
            return this.MakeUnary(ExpressionType.Convert, operand, type, typeMapping);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlUnaryExpression Not(SqlExpression operand)
        {
            return this.MakeUnary(ExpressionType.Not, operand, operand.Type, operand.TypeMapping);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlUnaryExpression Negate(SqlExpression operand)
        {
            return this.MakeUnary(ExpressionType.Negate, operand, operand.Type, operand.TypeMapping);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlFunctionExpression Function(
            string functionName,
            IEnumerable<SqlExpression> arguments,
            Type returnType,
            CoreTypeMapping typeMapping = null)
        {
            var typeMappedArguments = new List<SqlExpression>();

            foreach (var argument in arguments)
            {
                typeMappedArguments.Add(this.ApplyDefaultTypeMapping(argument));
            }

            return new SqlFunctionExpression(
                functionName,
                typeMappedArguments,
                returnType,
                typeMapping);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlConditionalExpression Condition(SqlExpression test, SqlExpression ifTrue, SqlExpression ifFalse)
        {
            var typeMapping = ExpressionExtensions.InferTypeMapping(ifTrue, ifFalse);

            return new SqlConditionalExpression(
                this.ApplyTypeMapping(test, this._boolTypeMapping),
                this.ApplyTypeMapping(ifTrue, typeMapping),
                this.ApplyTypeMapping(ifFalse, typeMapping));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InExpression In(SqlExpression item, SqlExpression values, bool negated)
        {
            var typeMapping = item.TypeMapping ?? this._typeMappingSource.FindMapping(item.Type);

            item = this.ApplyTypeMapping(item, typeMapping);
            values = this.ApplyTypeMapping(values, typeMapping);

            return new InExpression(item, negated, values, this._boolTypeMapping);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlConstantExpression Constant(object value, CoreTypeMapping typeMapping = null)
        {
            return new(Expression.Constant(value), typeMapping);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SelectExpression Select(IEntityType entityType)
        {
            var selectExpression = new SelectExpression(entityType);
            this.AddDiscriminator(selectExpression, entityType);

            return selectExpression;
        }

        private void AddDiscriminator(SelectExpression selectExpression, IEntityType entityType)
        {
            var concreteEntityTypes = entityType.GetConcreteDerivedTypesInclusive().ToList();

            if (concreteEntityTypes.Count == 1)
            {
                var concreteEntityType = concreteEntityTypes[0];
                var discriminatorProperty = concreteEntityType.FindDiscriminatorProperty();
                if (discriminatorProperty != null)
                {
                    var discriminatorColumn = ((EntityProjectionExpression)selectExpression.GetMappedProjection(new ProjectionMember()))
                        .BindProperty(discriminatorProperty, clientEval: false);

                    selectExpression.ApplyPredicate(
                        this.Equal((SqlExpression)discriminatorColumn, this.Constant(concreteEntityType.GetDiscriminatorValue())));
                }
            }
            else
            {
                var discriminatorColumn = ((EntityProjectionExpression)selectExpression.GetMappedProjection(new ProjectionMember()))
                    .BindProperty(concreteEntityTypes[0].FindDiscriminatorProperty(), clientEval: false);

                selectExpression.ApplyPredicate(
                    this.In(
                        (SqlExpression)discriminatorColumn, this.Constant(concreteEntityTypes.Select(et => et.GetDiscriminatorValue()).ToList()),
                        negated: false));
            }
        }
    }
}
