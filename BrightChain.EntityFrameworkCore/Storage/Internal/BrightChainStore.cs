// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Helpers;
using BrightChain.EntityFrameworkCore.Internal;
using BrightChain.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;
using System.Collections.Generic;
using System.Linq;

namespace BrightChain.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class BrightChainStore : IBrightChainStore
    {
        private readonly IBrightChainTableFactory _tableFactory;
        private readonly bool _useNameMatching;

        private readonly object _lock = new();

        private Dictionary<object, IBrightChainTable>? _tables;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public BrightChainStore(
            IBrightChainTableFactory tableFactory,
            bool useNameMatching)
        {
            this._tableFactory = tableFactory;
            this._useNameMatching = useNameMatching;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual BrightChainIntegerValueGenerator<TProperty> GetIntegerValueGenerator<TProperty>(
            IProperty property)
        {
            lock (this._lock)
            {
                var entityType = property.DeclaringEntityType;

                return this.EnsureTable(entityType).GetIntegerValueGenerator<TProperty>(
                    property,
                    entityType.GetDerivedTypesInclusive().Select(type => this.EnsureTable(type)).ToArray());
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool EnsureCreated(
            IUpdateAdapterFactory updateAdapterFactory,
            IModel designModel,
            IDiagnosticsLogger<DbLoggerCategory.Update> updateLogger)
        {
            lock (this._lock)
            {
                var valuesSeeded = this._tables == null;
                if (valuesSeeded)
                {
                    // ReSharper disable once AssignmentIsFullyDiscarded
                    this._tables = CreateTables();

                    var updateAdapter = updateAdapterFactory.CreateStandalone();
                    var entries = new List<IUpdateEntry>();
                    foreach (var entityType in designModel.GetEntityTypes())
                    {
                        IEntityType? targetEntityType = null;
                        foreach (var targetSeed in entityType.GetSeedData())
                        {
                            targetEntityType ??= updateAdapter.Model.FindEntityType(entityType.Name)!;
                            var entry = updateAdapter.CreateEntry(targetSeed, targetEntityType);
                            entry.EntityState = EntityState.Added;
                            entries.Add(entry);
                        }
                    }

                    this.ExecuteTransaction(entries, updateLogger);
                }

                return valuesSeeded;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool Clear()
        {
            lock (this._lock)
            {
                if (this._tables == null)
                {
                    return false;
                }

                this._tables = null;

                return true;
            }
        }

        private static Dictionary<object, IBrightChainTable> CreateTables()
            => new();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyList<BrightChainTableSnapshot> GetTables(IEntityType entityType)
        {
            var data = new List<BrightChainTableSnapshot>();
            lock (this._lock)
            {
                if (this._tables != null)
                {
                    foreach (var et in entityType.GetDerivedTypesInclusive().Where(et => !et.IsAbstract()))
                    {
                        var key = this._useNameMatching ? (object)et.Name : et;
                        if (this._tables.TryGetValue(key, out var table))
                        {
                            data.Add(new BrightChainTableSnapshot(et, table.SnapshotRows()));
                        }
                    }
                }
            }

            return data;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual int ExecuteTransaction(
            IList<IUpdateEntry> entries,
            IDiagnosticsLogger<DbLoggerCategory.Update> updateLogger)
        {
            var rowsAffected = 0;

            lock (this._lock)
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    var entityType = entry.EntityType;

                    Check.DebugAssert(!entityType.IsAbstract(), "entityType is abstract");

                    var table = this.EnsureTable(entityType);

                    if (entry.SharedIdentityEntry != null)
                    {
                        if (entry.EntityState == EntityState.Deleted)
                        {
                            continue;
                        }

                        table.Delete(entry);
                    }

                    switch (entry.EntityState)
                    {
                        case EntityState.Added:
                            table.Create(entry);
                            break;
                        case EntityState.Deleted:
                            table.Delete(entry);
                            break;
                        case EntityState.Modified:
                            table.Update(entry);
                            break;
                    }

                    rowsAffected++;
                }
            }

            updateLogger.ChangesSaved(entries, rowsAffected);

            return rowsAffected;
        }

        // Must be called from inside the lock
        private IBrightChainTable EnsureTable(IEntityType entityType)
        {
            if (this._tables == null)
            {
                this._tables = CreateTables();
            }

            IBrightChainTable? baseTable = null;

            var entityTypes = entityType.GetAllBaseTypesInclusive();
            foreach (var currentEntityType in entityTypes)
            {
                var key = this._useNameMatching ? (object)currentEntityType.Name : currentEntityType;
                if (!this._tables.TryGetValue(key, out var table))
                {
                    this._tables.Add(key, table = this._tableFactory.Create(currentEntityType, baseTable));
                }

                baseTable = table;
            }

            return this._tables[this._useNameMatching ? entityType.Name : entityType];
        }
    }
}
