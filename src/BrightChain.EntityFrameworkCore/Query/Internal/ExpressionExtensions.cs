// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BrightChain.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static CoreTypeMapping? InferTypeMapping(params Expression[] expressions)
        {
            for (var i = 0; i < expressions.Length; i++)
            {
                if (expressions[i] is SqlExpression sql
                    && sql.TypeMapping != null)
                {
                    return sql.TypeMapping;
                }
            }

            return null;
        }

        /// <summary>
        ///     <para>
        ///         MethodInfo which is used to generate an <see cref="Expression" /> tree representing reading a value from a
        ///         <see cref="ValueBuffer" />
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        public static readonly MethodInfo ValueBufferTryReadValueMethod
            = typeof(ExpressionExtensions).GetRequiredDeclaredMethod(nameof(ValueBufferTryReadValue));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TValue ValueBufferTryReadValue<TValue>(
#pragma warning disable IDE0060 // Remove unused parameter
            in ValueBuffer valueBuffer,
            int index,
            IPropertyBase property)
        {
            return (TValue)valueBuffer[index]!;
        }
    }
}
