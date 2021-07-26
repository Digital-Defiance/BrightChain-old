// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Linq.Expressions;
using System.Text.Json.Nodes;
using BrightChain.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace BrightChain.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqlConstantExpression : SqlExpression
    {
        private readonly ConstantExpression _constantExpression;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlConstantExpression(ConstantExpression constantExpression, CoreTypeMapping? typeMapping)
            : base(constantExpression.Type, typeMapping)
        {
            this._constantExpression = constantExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual object? Value
            => this._constantExpression.Value;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression ApplyTypeMapping(CoreTypeMapping? typeMapping)
        {
            return new SqlConstantExpression(this._constantExpression, typeMapping ?? this.TypeMapping);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            this.Print(this.Value, expressionPrinter);
        }

        private void Print(
            object? value,
            ExpressionPrinter expressionPrinter)
        {
            if (value is IEnumerable enumerable
                && !(value is string)
                && !(value is byte[]))
            {
                var first = true;
                foreach (var item in enumerable)
                {
                    if (!first)
                    {
                        expressionPrinter.Append(", ");
                    }

                    first = false;
                    this.Print(item, expressionPrinter);
                }
            }
            else
            {
                var jToken = this.GenerateJToken(this.Value, this.TypeMapping);

                expressionPrinter.Append(jToken == null ? "null" : jToken.ToString());
            }
        }

        private JsonNode? GenerateJToken(object? value, CoreTypeMapping? typeMapping)
        {
            var mappingClrType = typeMapping?.ClrType.UnwrapNullableType() ?? this.Type;
            if (value?.GetType().IsInteger() == true
                && mappingClrType.IsEnum)
            {
                value = Enum.ToObject(mappingClrType, value);
            }

            var converter = typeMapping?.Converter;
            if (converter != null)
            {
                value = converter.ConvertToProvider(value);
            }

            if (value == null)
            {
                return null;
            }

            return (JsonNode)((value as JsonNode) ?? value);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj != null
                           && (ReferenceEquals(this, obj)
                               || obj is SqlConstantExpression sqlConstantExpression
                               && this.Equals(sqlConstantExpression));
        }

        private bool Equals(SqlConstantExpression sqlConstantExpression)
        {
            return base.Equals(sqlConstantExpression)
                           && (this.Value == null
                               ? sqlConstantExpression.Value == null
                               : this.Value.Equals(sqlConstantExpression.Value));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), this.Value);
        }
    }
}
