// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Helpers;
using BrightChain.EntityFrameworkCore.Infrastructure.Internal;
using Hangfire.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace BrightChain.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class BrightChainTableFactory : IBrightChainTableFactory
    {
        private readonly bool _sensitiveLoggingEnabled;
        private readonly bool _nullabilityCheckEnabled;

        private readonly ConcurrentDictionary<(IEntityType EntityType, IBrightChainTable? BaseTable), Func<IBrightChainTable>> _factories = new();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public BrightChainTableFactory(
            ILoggingOptions loggingOptions,
            IBrightChainSingletonOptions options)
        {
            Check.NotNull(loggingOptions, nameof(loggingOptions));
            Check.NotNull(options, nameof(options));

            this._sensitiveLoggingEnabled = loggingOptions.IsSensitiveDataLoggingEnabled;
            this._nullabilityCheckEnabled = options.IsNullabilityCheckEnabled;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IBrightChainTable Create(IEntityType entityType, IBrightChainTable? baseTable)
            => this._factories.GetOrAdd((entityType, baseTable), e => this.CreateTable(e.EntityType, e.BaseTable))();

        private Func<IBrightChainTable> CreateTable(IEntityType entityType, IBrightChainTable? baseTable)
            => (Func<IBrightChainTable>)typeof(BrightChainTableFactory).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateFactory))!
                .MakeGenericMethod(entityType.FindPrimaryKey()!.GetKeyType())
                .Invoke(null, new object?[] { entityType, baseTable, this._sensitiveLoggingEnabled, this._nullabilityCheckEnabled })!;

        [UsedImplicitly]
        private static Func<IBrightChainTable> CreateFactory<TKey>(
            IEntityType entityType,
            IBrightChainTable baseTable,
            bool sensitiveLoggingEnabled,
            bool nullabilityCheckEnabled)
            where TKey : notnull
            => () => new BrightChainTable<TKey>(entityType, baseTable, sensitiveLoggingEnabled, nullabilityCheckEnabled);
    }
}
