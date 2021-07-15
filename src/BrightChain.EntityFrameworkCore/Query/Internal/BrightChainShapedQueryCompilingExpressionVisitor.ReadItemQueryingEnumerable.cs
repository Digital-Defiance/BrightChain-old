// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Metadata.Conventions;
using BrightChain.EntityFrameworkCore.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

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
                _brightChainQueryContext = brightChainQueryContext;
                _readItemExpression = readItemExpression;
                _shaper = shaper;
                _contextType = contextType;
                _queryLogger = _brightChainQueryContext.QueryLogger;
                _standAloneStateManager = standAloneStateManager;
                _threadSafetyChecksEnabled = threadSafetyChecksEnabled;
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
                return GetEnumerator();
            }

            public string ToQueryString()
            {
                TryGetResourceId(out var resourceId);
                TryGetPartitionId(out var partitionKey);
                return BrightChainStrings.NoReadItemQueryString(resourceId, partitionKey);
            }

            private bool TryGetPartitionId(out string partitionKey)
            {
                partitionKey = null;

                var partitionKeyPropertyName = _readItemExpression.EntityType.GetPartitionKeyPropertyName();
                if (partitionKeyPropertyName == null)
                {
                    return true;
                }

                var partitionKeyProperty = _readItemExpression.EntityType.FindProperty(partitionKeyPropertyName);

                if (TryGetParameterValue(partitionKeyProperty, out var value))
                {
                    partitionKey = GetString(partitionKeyProperty, value);

                    return !string.IsNullOrEmpty(partitionKey);
                }

                return false;
            }

            private bool TryGetResourceId(out string resourceId)
            {
                var idProperty = _readItemExpression.EntityType.GetProperties()
                    .FirstOrDefault(p => p.GetJsonPropertyName() == StoreKeyConvention.IdPropertyJsonName);

                if (TryGetParameterValue(idProperty, out var value))
                {
                    resourceId = GetString(idProperty, value);

                    if (string.IsNullOrEmpty(resourceId))
                    {
                        throw new InvalidOperationException(BrightChainStrings.InvalidResourceId);
                    }

                    return true;
                }

                if (TryGenerateIdFromKeys(idProperty, out var generatedValue))
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
                return _readItemExpression.PropertyParameters.TryGetValue(property, out var parameterName)
                    && _brightChainQueryContext.ParameterValues.TryGetValue(parameterName, out value);
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
                var entityEntry = Activator.CreateInstance(_readItemExpression.EntityType.ClrType);

#pragma warning disable EF1001 // Internal EF Core API usage.
                var internalEntityEntry = new InternalEntityEntry(
                    _brightChainQueryContext.Context.GetDependencies().StateManager, _readItemExpression.EntityType, entityEntry);
#pragma warning restore EF1001 // Internal EF Core API usage.

                foreach (var keyProperty in _readItemExpression.EntityType.FindPrimaryKey().Properties)
                {
                    var property = _readItemExpression.EntityType.FindProperty(keyProperty.Name);

                    if (TryGetParameterValue(property, out var parameterValue))
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
                    _brightChainQueryContext = readItemEnumerable._brightChainQueryContext;
                    _readItemExpression = readItemEnumerable._readItemExpression;
                    _shaper = readItemEnumerable._shaper;
                    _contextType = readItemEnumerable._contextType;
                    _queryLogger = readItemEnumerable._queryLogger;
                    _standAloneStateManager = readItemEnumerable._standAloneStateManager;
                    _readItemEnumerable = readItemEnumerable;
                    _cancellationToken = cancellationToken;

                    _concurrencyDetector = readItemEnumerable._threadSafetyChecksEnabled
                        ? _brightChainQueryContext.ConcurrencyDetector
                        : null;
                }

                object IEnumerator.Current
                    => Current;

                public T Current { get; private set; }

                public bool MoveNext()
                {
                    try
                    {
                        _concurrencyDetector?.EnterCriticalSection();

                        try
                        {
                            if (!_hasExecuted)
                            {
                                if (!_readItemEnumerable.TryGetResourceId(out var resourceId))
                                {
                                    throw new InvalidOperationException(BrightChainStrings.ResourceIdMissing);
                                }

                                if (!_readItemEnumerable.TryGetPartitionId(out var partitionKey))
                                {
                                    throw new InvalidOperationException(BrightChainStrings.PartitionKeyMissing);
                                }

                                EntityFrameworkEventSource.Log.QueryExecuting();

                                _item = _brightChainQueryContext.BrightChainClient.ExecuteReadItem(
                                    _readItemExpression.Container,
                                    partitionKey,
                                    resourceId);

                                return ShapeResult();
                            }

                            return false;
                        }
                        finally
                        {
                            _concurrencyDetector?.ExitCriticalSection();
                        }
                    }
                    catch (Exception exception)
                    {
                        _queryLogger.QueryIterationFailed(_contextType, exception);

                        throw;
                    }
                }

                public async ValueTask<bool> MoveNextAsync()
                {
                    try
                    {
                        _concurrencyDetector?.EnterCriticalSection();

                        try
                        {
                            if (!_hasExecuted)
                            {
                                if (!_readItemEnumerable.TryGetResourceId(out var resourceId))
                                {
                                    throw new InvalidOperationException(BrightChainStrings.ResourceIdMissing);
                                }

                                if (!_readItemEnumerable.TryGetPartitionId(out var partitionKey))
                                {
                                    throw new InvalidOperationException(BrightChainStrings.PartitionKeyMissing);
                                }

                                EntityFrameworkEventSource.Log.QueryExecuting();

                                _item = await _brightChainQueryContext.BrightChainClient.ExecuteReadItemAsync(
                                        _readItemExpression.Container,
                                        partitionKey,
                                        resourceId,
                                        _cancellationToken)
                                    .ConfigureAwait(false);

                                return ShapeResult();
                            }

                            return false;
                        }
                        finally
                        {
                            _concurrencyDetector?.ExitCriticalSection();
                        }
                    }
                    catch (Exception exception)
                    {
                        _queryLogger.QueryIterationFailed(_contextType, exception);

                        throw;
                    }
                }

                public void Dispose()
                {
                    _item = null;
                    _hasExecuted = false;
                }

                public ValueTask DisposeAsync()
                {
                    Dispose();

                    return default;
                }

                public void Reset()
                {
                    throw new NotSupportedException(CoreStrings.EnumerableResetNotSupported);
                }

                private bool ShapeResult()
                {
                    var hasNext = !(_item is null);

                    _brightChainQueryContext.InitializeStateManager(_standAloneStateManager);

                    Current
                        = hasNext
                            ? _shaper(_brightChainQueryContext, _item)
                            : default;

                    _hasExecuted = true;

                    return hasNext;
                }
            }
        }
    }
}
