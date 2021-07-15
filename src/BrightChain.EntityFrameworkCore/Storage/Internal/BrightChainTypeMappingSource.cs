// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace BrightChain.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class BrightChainTypeMappingSource : TypeMappingSource
    {
        private readonly Dictionary<Type, BrightChainTypeMapping> _clrTypeMappings;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public BrightChainTypeMappingSource(TypeMappingSourceDependencies dependencies)
            : base(dependencies)
        {
            _clrTypeMappings
                = new Dictionary<Type, BrightChainTypeMapping>
                {
                    { typeof(byte[]), new BrightChainTypeMapping(typeof(byte[]), keyComparer: new ArrayStructuralComparer<byte>()) },
                    { typeof(JObject), new BrightChainTypeMapping(typeof(JObject)) }
                };
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override CoreTypeMapping? FindMapping(in TypeMappingInfo mappingInfo)
        {
            var clrType = mappingInfo.ClrType;
            Check.DebugAssert(clrType != null, "ClrType is null");

            if (_clrTypeMappings.TryGetValue(clrType, out var mapping))
            {
                return mapping;
            }

            if ((clrType.IsValueType
                    && !clrType.IsEnum)
                || clrType == typeof(string))
            {
                return new BrightChainTypeMapping(clrType);
            }

            return base.FindMapping(mappingInfo);
        }
    }
}
