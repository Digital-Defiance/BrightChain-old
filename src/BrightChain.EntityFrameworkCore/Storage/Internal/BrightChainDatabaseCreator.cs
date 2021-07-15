// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrightChain.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class BrightChainDatabaseCreator : IDatabaseCreator
    {
        private readonly IBrightChainClientWrapper _brightChainClient;
        private readonly IDesignTimeModel _designTimeModel;
        private readonly IUpdateAdapterFactory _updateAdapterFactory;
        private readonly IDatabase _database;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public BrightChainDatabaseCreator(
            IBrightChainClientWrapper brightChainClient,
            IDesignTimeModel designTimeModel,
            IUpdateAdapterFactory updateAdapterFactory,
            IDatabase database)
        {
            _brightChainClient = brightChainClient;
            _designTimeModel = designTimeModel;
            _updateAdapterFactory = updateAdapterFactory;
            _database = database;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool EnsureCreated()
        {
            var created = _brightChainClient.CreateDatabaseIfNotExists();
            foreach (var entityType in _designTimeModel.Model.GetEntityTypes())
            {
                var containerName = entityType.GetContainer();
                if (containerName != null)
                {
                    created |= _brightChainClient.CreateContainerIfNotExists(
                        containerName,
                        GetPartitionKeyStoreName(entityType));
                }
            }

            if (created)
            {
                Seed();
            }

            return created;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual async Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default)
        {
            var created = await _brightChainClient.CreateDatabaseIfNotExistsAsync(cancellationToken)
                .ConfigureAwait(false);
            foreach (var entityType in _designTimeModel.Model.GetEntityTypes())
            {
                var containerName = entityType.GetContainer();
                if (containerName != null)
                {
                    created |= await _brightChainClient.CreateContainerIfNotExistsAsync(
                            containerName,
                            GetPartitionKeyStoreName(entityType),
                            cancellationToken)
                        .ConfigureAwait(false);
                }
            }

            if (created)
            {
                await SeedAsync(cancellationToken).ConfigureAwait(false);
            }

            return created;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void Seed()
        {
            var updateAdapter = AddSeedData();

            _database.SaveChanges(updateAdapter.GetEntriesToSave());
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Task SeedAsync(CancellationToken cancellationToken = default)
        {
            var updateAdapter = AddSeedData();

            return _database.SaveChangesAsync(updateAdapter.GetEntriesToSave(), cancellationToken);
        }

        private IUpdateAdapter AddSeedData()
        {
            var updateAdapter = _updateAdapterFactory.CreateStandalone();
            foreach (var entityType in _designTimeModel.Model.GetEntityTypes())
            {
                IEntityType? targetEntityType = null;
                foreach (var targetSeed in entityType.GetSeedData())
                {
                    targetEntityType ??= updateAdapter.Model.FindEntityType(entityType.Name)!;
                    var entry = updateAdapter.CreateEntry(targetSeed, entityType);
                    entry.EntityState = EntityState.Added;
                }
            }

            return updateAdapter;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool EnsureDeleted()
        {
            return _brightChainClient.DeleteDatabase();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default)
        {
            return _brightChainClient.DeleteDatabaseAsync(cancellationToken);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanConnect()
        {
            throw new NotSupportedException(BrightChainStrings.CanConnectNotSupported);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException(BrightChainStrings.CanConnectNotSupported);
        }

        /// <summary>
        ///     Returns the store name of the property that is used to store the partition key.
        /// </summary>
        /// <param name="entityType"> The entity type to get the partition key property name for. </param>
        /// <returns> The name of the partition key property. </returns>
        private static string GetPartitionKeyStoreName(IEntityType entityType)
        {
            var name = entityType.GetPartitionKeyPropertyName();
            if (name != null)
            {
                return entityType.FindProperty(name)!.GetJsonPropertyName();
            }

            return BrightChainClientWrapper.DefaultPartitionKey;
        }
    }
}
