// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using BrightChain.EntityFrameworkCore.Metadata.Conventions;
using BrightChain.EntityFrameworkCore.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

#nullable disable

namespace BrightChain.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public partial class BrightChainShapedQueryCompilingExpressionVisitor
    {
        private sealed class ReadItemQueryingEnumerable<T> : IEnumerable<T>, IAsyncEnumerable<T>, IQueryingEnumerable
        {
            private readonly BrightChainQueryContext _brightChainQueryContext;
            private readonly ReadItemExpression _readItemExpression;
            private readonly Func<BrightChainQueryContext, JsonNode, T> _shaper;
            private readonly Type _contextType;
            private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
            private readonly bool _standAloneStateManager;
            private readonly bool _threadSafetyChecksEnabled;

            public ReadItemQueryingEnumerable(
                BrightChainQueryContext brightChainQueryContext,
                ReadItemExpression readItemExpression,
                Func<BrightChainQueryContext, JsonNode, T> shaper,
                Type contextType,
                bool standAloneStateManager,
                bool threadSafetyChecksEnabled)
            {
                this._brightChainQueryContext = brightChainQueryContext;
                this._readItemExpression = readItemExpression;
                this._shaper = shaper;
                this._contextType = contextType;
                this._queryLogger = this._brightChainQueryContext.QueryLogger;
                this._standAloneStateManager = standAloneStateManager;
                this._threadSafetyChecksEnabled = threadSafetyChecksEnabled;
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new Enumerator(this, cancellationToken);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new Enumerator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public string ToQueryString()
            {
                this.TryGetResourceId(out var resourceId);
                this.TryGetPartitionId(out var partitionKey);
                return BrightChainStrings.NoReadItemQueryString(resourceId, partitionKey);
            }

            private bool TryGetPartitionId(out string partitionKey)
            {
                partitionKey = null;

                var partitionKeyPropertyName = this._readItemExpression.EntityType.GetPartitionKeyPropertyName();
                if (partitionKeyPropertyName == null)
                {
                    return true;
                }

                var partitionKeyProperty = this._readItemExpression.EntityType.FindProperty(partitionKeyPropertyName);

                if (this.TryGetParameterValue(partitionKeyProperty, out var value))
                {
                    partitionKey = GetString(partitionKeyProperty, value);

                    return !string.IsNullOrEmpty(partitionKey);
                }

                return false;
            }

            private bool TryGetResourceId(out string resourceId)
            {
                var idProperty = this._readItemExpression.EntityType.GetProperties()
                    .FirstOrDefault(p => p.GetJsonPropertyName() == StoreKeyConvention.IdPropertyJsonName);

                if (this.TryGetParameterValue(idProperty, out var value))
                {
                    resourceId = GetString(idProperty, value);

                    if (string.IsNullOrEmpty(resourceId))
                    {
                        throw new InvalidOperationException(BrightChainStrings.InvalidResourceId);
                    }

                    return true;
                }

                if (this.TryGenerateIdFromKeys(idProperty, out var generatedValue))
                {
                    resourceId = GetString(idProperty, generatedValue);

                    return true;
                }

                resourceId = null;
                return false;
            }

            private bool TryGetParameterValue(IProperty property, out object value)
            {
                value = null;
                return this._readItemExpression.PropertyParameters.TryGetValue(property, out var parameterName)
                    && this._brightChainQueryContext.ParameterValues.TryGetValue(parameterName, out value);
            }

            private static string GetString(IProperty property, object value)
            {
                var converter = property.GetTypeMapping().Converter;

                return converter is null
                    ? (string)value
                    : (string)converter.ConvertToProvider(value);
            }

            private bool TryGenerateIdFromKeys(IProperty idProperty, out object value)
            {
                var entityEntry = Activator.CreateInstance(this._readItemExpression.EntityType.ClrType);

#pragma warning disable EF1001 // Internal EF Core API usage.
                var internalEntityEntry = new InternalEntityEntry(
                    this._brightChainQueryContext.Context.GetDependencies().StateManager, this._readItemExpression.EntityType, entityEntry);
#pragma warning restore EF1001 // Internal EF Core API usage.

                foreach (var keyProperty in this._readItemExpression.EntityType.FindPrimaryKey().Properties)
                {
                    var property = this._readItemExpression.EntityType.FindProperty(keyProperty.Name);

                    if (this.TryGetParameterValue(property, out var parameterValue))
                    {
#pragma warning disable EF1001 // Internal EF Core API usage.
                        internalEntityEntry[property] = parameterValue;
#pragma warning restore EF1001 // Internal EF Core API usage.
                    }
                }

#pragma warning disable EF1001 // Internal EF Core API usage.
                internalEntityEntry.SetEntityState(EntityState.Added);

                value = internalEntityEntry[idProperty];

                internalEntityEntry.SetEntityState(EntityState.Detached);
#pragma warning restore EF1001 // Internal EF Core API usage.

                return value != null;
            }

            private sealed class Enumerator : IEnumerator<T>, IAsyncEnumerator<T>
            {
                private readonly BrightChainQueryContext _brightChainQueryContext;
                private readonly ReadItemExpression _readItemExpression;
                private readonly Func<BrightChainQueryContext, JsonNode, T> _shaper;
                private readonly Type _contextType;
                private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
                private readonly bool _standAloneStateManager;
                private readonly IConcurrencyDetector _concurrencyDetector;
                private readonly ReadItemQueryingEnumerable<T> _readItemEnumerable;
                private readonly CancellationToken _cancellationToken;

                private JsonNode _item;
                private bool _hasExecuted;

                public Enumerator(ReadItemQueryingEnumerable<T> readItemEnumerable, CancellationToken cancellationToken = default)
                {
                    this._brightChainQueryContext = readItemEnumerable._brightChainQueryContext;
                    this._readItemExpression = readItemEnumerable._readItemExpression;
                    this._shaper = readItemEnumerable._shaper;
                    this._contextType = readItemEnumerable._contextType;
                    this._queryLogger = readItemEnumerable._queryLogger;
                    this._standAloneStateManager = readItemEnumerable._standAloneStateManager;
                    this._readItemEnumerable = readItemEnumerable;
                    this._cancellationToken = cancellationToken;

                    this._concurrencyDetector = readItemEnumerable._threadSafetyChecksEnabled
                        ? this._brightChainQueryContext.ConcurrencyDetector
                        : null;
                }

                object IEnumerator.Current
                    => this.Current;

                public T Current { get; private set; }

                public bool MoveNext()
                {
                    try
                    {
                        this._concurrencyDetector?.EnterCriticalSection();

                        try
                        {
                            if (!this._hasExecuted)
                            {
                                if (!this._readItemEnumerable.TryGetResourceId(out var resourceId))
                                {
                                    throw new InvalidOperationException(BrightChainStrings.ResourceIdMissing);
                                }

                                if (!this._readItemEnumerable.TryGetPartitionId(out var partitionKey))
                                {
                                    throw new InvalidOperationException(BrightChainStrings.PartitionKeyMissing);
                                }

                                EntityFrameworkEventSource.Log.QueryExecuting();

                                this._item = this._brightChainQueryContext.BrightChainClient.ExecuteReadItem(
                                    this._readItemExpression.Container,
                                    partitionKey,
                                    resourceId);

                                return this.ShapeResult();
                            }

                            return false;
                        }
                        finally
                        {
                            this._concurrencyDetector?.ExitCriticalSection();
                        }
                    }
                    catch (Exception exception)
                    {
                        this._queryLogger.QueryIterationFailed(this._contextType, exception);

                        throw;
                    }
                }

                public async ValueTask<bool> MoveNextAsync()
                {
                    try
                    {
                        this._concurrencyDetector?.EnterCriticalSection();

                        try
                        {
                            if (!this._hasExecuted)
                            {
                                if (!this._readItemEnumerable.TryGetResourceId(out var resourceId))
                                {
                                    throw new InvalidOperationException(BrightChainStrings.ResourceIdMissing);
                                }

                                if (!this._readItemEnumerable.TryGetPartitionId(out var partitionKey))
                                {
                                    throw new InvalidOperationException(BrightChainStrings.PartitionKeyMissing);
                                }

                                EntityFrameworkEventSource.Log.QueryExecuting();

                                this._item = await this._brightChainQueryContext.BrightChainClient.ExecuteReadItemAsync(
                                        this._readItemExpression.Container,
                                        partitionKey,
                                        resourceId,
                                        this._cancellationToken)
                                    .ConfigureAwait(false);

                                return this.ShapeResult();
                            }

                            return false;
                        }
                        finally
                        {
                            this._concurrencyDetector?.ExitCriticalSection();
                        }
                    }
                    catch (Exception exception)
                    {
                        this._queryLogger.QueryIterationFailed(this._contextType, exception);

                        throw;
                    }
                }

                public void Dispose()
                {
                    this._item = null;
                    this._hasExecuted = false;
                }

                public ValueTask DisposeAsync()
                {
                    this.Dispose();

                    return default;
                }

                public void Reset()
                {
                    throw new NotSupportedException(CoreStrings.EnumerableResetNotSupported);
                }

                private bool ShapeResult()
                {
                    var hasNext = !(this._item is null);

                    this._brightChainQueryContext.InitializeStateManager(this._standAloneStateManager);

                    this.Current
                        = hasNext
                            ? this._shaper(this._brightChainQueryContext, this._item)
                            : default;

                    this._hasExecuted = true;

                    return hasNext;
                }
            }
        }
    }
}
