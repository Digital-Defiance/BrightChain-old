// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Linq;
using System.Text.Json.Nodes;
using BrightChain.EntityFrameworkCore.Metadata.Conventions;
using BrightChain.EntityFrameworkCore.Metadata.Internal;
using BrightChain.EntityFrameworkCore.Properties;
using BrightChain.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;

namespace BrightChain.EntityFrameworkCore.Update.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class DocumentSource
    {
        private readonly string _containerId;
        private readonly BrightChainDatabaseWrapper _database;
        private readonly IEntityType _entityType;
        private readonly IProperty? _idProperty;
        private readonly IProperty? _jObjectProperty;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public DocumentSource(IEntityType entityType, BrightChainDatabaseWrapper database)
        {
            this._containerId = entityType.GetContainer()!;
            this._database = database;
            this._entityType = entityType;
            this._idProperty = entityType.GetProperties().FirstOrDefault(p => p.GetJsonPropertyName() == StoreKeyConvention.IdPropertyJsonName);
            this._jObjectProperty = entityType.FindProperty(StoreKeyConvention.JObjectPropertyName);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string GetContainerId()
        {
            return this._containerId;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string GetId(IUpdateEntry entry)
        {
            return this._idProperty is null
                           ? throw new InvalidOperationException(BrightChainStrings.NoIdProperty(this._entityType.DisplayName()))
                           : (string)entry.GetCurrentProviderValue(this._idProperty)!;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual JsonNode CreateDocument(IUpdateEntry entry)
        {
            return this.CreateDocument(entry, null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual JsonNode CreateDocument(IUpdateEntry entry, int? ordinal)
        {
            var document = new JsonObject();
            foreach (var property in entry.EntityType.GetProperties())
            {
                var storeName = property.GetJsonPropertyName();
                if (storeName.Length != 0)
                {
                    document[storeName] = ConvertPropertyValue(property, entry);
                }
                else if (entry.HasTemporaryValue(property))
                {
                    if (ordinal != null
                        && property.IsOrdinalKeyProperty())
                    {
                        entry.SetStoreGeneratedValue(property, ordinal.Value);
                    }
                }
            }

            foreach (var embeddedNavigation in entry.EntityType.GetNavigations())
            {
                var fk = embeddedNavigation.ForeignKey;
                if (!fk.IsOwnership
                    || embeddedNavigation.IsOnDependent
                    || fk.DeclaringEntityType.IsDocumentRoot())
                {
                    continue;
                }

                var embeddedValue = entry.GetCurrentValue(embeddedNavigation);
                var embeddedPropertyName = fk.DeclaringEntityType.GetContainingPropertyName();
                if (embeddedValue == null)
                {
                    document[embeddedPropertyName] = null;
                }
                else if (fk.IsUnique)
                {
#pragma warning disable EF1001 // Internal EF Core API usage.
                    // #16707
                    var dependentEntry = ((InternalEntityEntry)entry).StateManager.TryGetEntry(embeddedValue, fk.DeclaringEntityType)!;
                    document[embeddedPropertyName] = this._database.GetDocumentSource(dependentEntry.EntityType).CreateDocument(dependentEntry);
#pragma warning restore EF1001 // Internal EF Core API usage.
                }
                else
                {
                    var embeddedOrdinal = 1;
                    var array = new JsonArray();
                    foreach (var dependent in (IEnumerable)embeddedValue)
                    {
#pragma warning disable EF1001 // Internal EF Core API usage.
                        // #16707
                        var dependentEntry = ((InternalEntityEntry)entry).StateManager.TryGetEntry(dependent, fk.DeclaringEntityType)!;
                        array.Add(this._database.GetDocumentSource(dependentEntry.EntityType).CreateDocument(dependentEntry, embeddedOrdinal));
#pragma warning restore EF1001 // Internal EF Core API usage.
                        embeddedOrdinal++;
                    }

                    document[embeddedPropertyName] = array;
                }
            }

            return document;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual JsonNode? UpdateDocument(JsonNode document, IUpdateEntry entry)
        {
            return this.UpdateDocument(document, entry, null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual JsonNode? UpdateDocument(JsonNode document, IUpdateEntry entry, int? ordinal)
        {
            var anyPropertyUpdated = false;
#pragma warning disable EF1001 // Internal EF Core API usage.
            // #16707
            var stateManager = ((InternalEntityEntry)entry).StateManager;
#pragma warning restore EF1001 // Internal EF Core API usage.
            foreach (var property in entry.EntityType.GetProperties())
            {
                if (entry.EntityState == EntityState.Added
                    || entry.IsModified(property))
                {
                    var storeName = property.GetJsonPropertyName();
                    if (storeName.Length != 0)
                    {
                        document[storeName] = ConvertPropertyValue(property, entry);
                        anyPropertyUpdated = true;
                    }
                }

                if (ordinal != null
                    && entry.HasTemporaryValue(property)
                    && property.IsOrdinalKeyProperty())
                {
                    entry.SetStoreGeneratedValue(property, ordinal.Value);
                }
            }

            foreach (var ownedNavigation in entry.EntityType.GetNavigations())
            {
                var fk = ownedNavigation.ForeignKey;
                if (!fk.IsOwnership
                    || ownedNavigation.IsOnDependent
                    || fk.DeclaringEntityType.IsDocumentRoot())
                {
                    continue;
                }

                var embeddedDocumentSource = this._database.GetDocumentSource(fk.DeclaringEntityType);
                var embeddedValue = entry.GetCurrentValue(ownedNavigation);
                var embeddedPropertyName = fk.DeclaringEntityType.GetContainingPropertyName();
                if (embeddedValue == null)
                {
                    if (document[embeddedPropertyName] != null)
                    {
                        document[embeddedPropertyName] = null;
                        anyPropertyUpdated = true;
                    }
                }
                else if (fk.IsUnique)
                {
#pragma warning disable EF1001 // Internal EF Core API usage.
                    // #16707
                    var embeddedEntry = ((InternalEntityEntry)entry).StateManager.TryGetEntry(embeddedValue, fk.DeclaringEntityType);
#pragma warning restore EF1001 // Internal EF Core API usage.
                    if (embeddedEntry == null)
                    {
                        continue;
                    }

                    var embeddedDocument = embeddedDocumentSource.GetCurrentDocument(embeddedEntry);
                    embeddedDocument = embeddedDocument != null
                        ? embeddedDocumentSource.UpdateDocument(embeddedDocument, embeddedEntry)
                        : embeddedDocumentSource.CreateDocument(embeddedEntry);

                    if (embeddedDocument != null)
                    {
                        document[embeddedPropertyName] = embeddedDocument;
                        anyPropertyUpdated = true;
                    }
                }
                else
                {
                    var embeddedOrdinal = 1;
                    var ordinalKeyProperty = this.FindOrdinalKeyProperty(fk.DeclaringEntityType);
                    if (ordinalKeyProperty != null)
                    {
                        var shouldSetTemporaryKeys = false;
                        foreach (var dependent in (IEnumerable)embeddedValue)
                        {
#pragma warning disable EF1001 // Internal EF Core API usage.
                            // #16707
                            var embeddedEntry = stateManager.TryGetEntry(dependent, fk.DeclaringEntityType);
                            if (embeddedEntry == null)
                            {
                                continue;
                            }

                            if ((int)embeddedEntry.GetCurrentValue(ordinalKeyProperty)! != embeddedOrdinal)
                            {
                                shouldSetTemporaryKeys = true;
                                break;
                            }
#pragma warning restore EF1001 // Internal EF Core API usage.

                            embeddedOrdinal++;
                        }

                        if (shouldSetTemporaryKeys)
                        {
                            var temporaryOrdinal = -1;
                            foreach (var dependent in (IEnumerable)embeddedValue)
                            {
#pragma warning disable EF1001 // Internal EF Core API usage.
                                // #16707
                                var embeddedEntry = stateManager.TryGetEntry(dependent, fk.DeclaringEntityType);
                                if (embeddedEntry == null)
                                {
                                    continue;
                                }

                                embeddedEntry.SetTemporaryValue(ordinalKeyProperty, temporaryOrdinal, setModified: false);
#pragma warning restore EF1001 // Internal EF Core API usage.

                                temporaryOrdinal--;
                            }
                        }
                    }

                    embeddedOrdinal = 1;
                    var array = new JsonArray();
                    foreach (var dependent in (IEnumerable)embeddedValue)
                    {
#pragma warning disable EF1001 // Internal EF Core API usage.
                        // #16707
                        var embeddedEntry = ((InternalEntityEntry)entry).StateManager.TryGetEntry(dependent, fk.DeclaringEntityType);
#pragma warning restore EF1001 // Internal EF Core API usage.
                        if (embeddedEntry == null)
                        {
                            continue;
                        }

                        var embeddedDocument = embeddedDocumentSource.GetCurrentDocument(embeddedEntry);
                        embeddedDocument = embeddedDocument != null
                            ? embeddedDocumentSource.UpdateDocument(embeddedDocument, embeddedEntry, embeddedOrdinal) ?? embeddedDocument
                            : embeddedDocumentSource.CreateDocument(embeddedEntry, embeddedOrdinal);

                        array.Add(embeddedDocument);
                        embeddedOrdinal++;
                    }

                    document[embeddedPropertyName] = array;
                    anyPropertyUpdated = true;
                }
            }

            return anyPropertyUpdated ? document : null;
        }

        private IProperty? FindOrdinalKeyProperty(IEntityType entityType)
        {
            return entityType.FindPrimaryKey()!.Properties.FirstOrDefault(
                           p =>
                               p.GetJsonPropertyName().Length == 0 && p.IsOrdinalKeyProperty());
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual JsonNode? GetCurrentDocument(IUpdateEntry entry)
        {
            throw new NotImplementedException();
        }

        //=> _jObjectProperty != null
        //    ? (JsonElement?)(entry.SharedIdentityEntry ?? entry).GetCurrentValue(_jObjectProperty)
        //    : null;

        private static JsonNode? ConvertPropertyValue(IProperty property, IUpdateEntry entry)
        {
            var value = entry.GetCurrentProviderValue(property);
            throw new NotImplementedException();
            //return value == null
            //    ? null
            //    : (value as JsonElement) ?? JToken.FromObject(value, BrightChainClientWrapper.Serializer);
        }
    }
}
