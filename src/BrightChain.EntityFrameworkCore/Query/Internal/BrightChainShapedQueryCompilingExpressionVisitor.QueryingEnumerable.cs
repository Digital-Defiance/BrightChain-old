// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using BrightChain.EntityFrameworkCore.Properties;
using BrightChain.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
        private sealed class QueryingEnumerable<T> : IEnumerable<T>, IAsyncEnumerable<T>, IQueryingEnumerable
        {
            private readonly BrightChainQueryContext _brightChainQueryContext;
            private readonly ISqlExpressionFactory _sqlExpressionFactory;
            private readonly SelectExpression _selectExpression;
            private readonly Func<BrightChainQueryContext, JsonNode, T> _shaper;
            private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;
            private readonly Type _contextType;
            private readonly string _partitionKey;
            private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
            private readonly bool _standAloneStateManager;
            private readonly bool _threadSafetyChecksEnabled;

            public QueryingEnumerable(
                BrightChainQueryContext brightChainQueryContext,
                ISqlExpressionFactory sqlExpressionFactory,
                IQuerySqlGeneratorFactory querySqlGeneratorFactory,
                SelectExpression selectExpression,
                Func<BrightChainQueryContext, JsonNode, T> shaper,
                Type contextType,
                string partitionKeyFromExtension,
                bool standAloneStateManager,
                bool threadSafetyChecksEnabled)
            {
                _brightChainQueryContext = brightChainQueryContext;
                _sqlExpressionFactory = sqlExpressionFactory;
                _querySqlGeneratorFactory = querySqlGeneratorFactory;
                _selectExpression = selectExpression;
                _shaper = shaper;
                _contextType = contextType;
                _queryLogger = brightChainQueryContext.QueryLogger;
                _standAloneStateManager = standAloneStateManager;
                _threadSafetyChecksEnabled = threadSafetyChecksEnabled;

                var partitionKey = selectExpression.GetPartitionKey(brightChainQueryContext.ParameterValues);
                if (partitionKey != null && partitionKeyFromExtension != null && partitionKeyFromExtension != partitionKey)
                {
                    throw new InvalidOperationException(BrightChainStrings.PartitionKeyMismatch(partitionKeyFromExtension, partitionKey));
                }

                _partitionKey = partitionKey ?? partitionKeyFromExtension;
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new AsyncEnumerator(this, cancellationToken);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new Enumerator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private BrightChainSqlQuery GenerateQuery()
            {
                return _querySqlGeneratorFactory.Create().GetSqlQuery(
                                   (SelectExpression)new InExpressionValuesExpandingExpressionVisitor(
                                           _sqlExpressionFactory,
                                           _brightChainQueryContext.ParameterValues)
                                       .Visit(_selectExpression),
                                   _brightChainQueryContext.ParameterValues);
            }

            public string ToQueryString()
            {
                var sqlQuery = GenerateQuery();
                if (sqlQuery.Parameters.Count == 0)
                {
                    return sqlQuery.Query;
                }

                var builder = new StringBuilder();
                foreach (var parameter in sqlQuery.Parameters)
                {
                    builder
                        .Append("-- ")
                        .Append(parameter.Name)
                        .Append("='")
                        .Append(parameter.Value)
                        .AppendLine("'");
                }

                return builder.Append(sqlQuery.Query).ToString();
            }

            private sealed class Enumerator : IEnumerator<T>
            {
                private readonly QueryingEnumerable<T> _queryingEnumerable;
                private readonly BrightChainQueryContext _brightChainQueryContext;
                private readonly SelectExpression _selectExpression;
                private readonly Func<BrightChainQueryContext, JsonNode, T> _shaper;
                private readonly Type _contextType;
                private readonly string _partitionKey;
                private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
                private readonly bool _standAloneStateManager;
                private readonly IConcurrencyDetector _concurrencyDetector;

                private IEnumerator<JsonNode> _enumerator;

                public Enumerator(QueryingEnumerable<T> queryingEnumerable)
                {
                    _queryingEnumerable = queryingEnumerable;
                    _brightChainQueryContext = queryingEnumerable._brightChainQueryContext;
                    _shaper = queryingEnumerable._shaper;
                    _selectExpression = queryingEnumerable._selectExpression;
                    _contextType = queryingEnumerable._contextType;
                    _partitionKey = queryingEnumerable._partitionKey;
                    _queryLogger = queryingEnumerable._queryLogger;
                    _standAloneStateManager = queryingEnumerable._standAloneStateManager;

                    _concurrencyDetector = queryingEnumerable._threadSafetyChecksEnabled
                        ? _brightChainQueryContext.ConcurrencyDetector
                        : null;
                }

                public T Current { get; private set; }

                object IEnumerator.Current
                    => Current;

                public bool MoveNext()
                {
                    try
                    {
                        _concurrencyDetector?.EnterCriticalSection();

                        try
                        {
                            if (_enumerator == null)
                            {
                                var sqlQuery = _queryingEnumerable.GenerateQuery();

                                EntityFrameworkEventSource.Log.QueryExecuting();

                                _enumerator = _brightChainQueryContext.BrightChainClient
                                    .ExecuteSqlQuery(
                                        _selectExpression.Container,
                                        _partitionKey,
                                        sqlQuery)
                                    .GetEnumerator();
                                _brightChainQueryContext.InitializeStateManager(_standAloneStateManager);
                            }

                            var hasNext = _enumerator.MoveNext();

                            Current
                                = hasNext
                                    ? _shaper(_brightChainQueryContext, _enumerator.Current)
                                    : default;

                            return hasNext;
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
                    _enumerator?.Dispose();
                    _enumerator = null;
                }

                public void Reset()
                {
                    throw new NotSupportedException(CoreStrings.EnumerableResetNotSupported);
                }
            }

            private sealed class AsyncEnumerator : IAsyncEnumerator<T>
            {
                private readonly QueryingEnumerable<T> _queryingEnumerable;
                private readonly BrightChainQueryContext _brightChainQueryContext;
                private readonly SelectExpression _selectExpression;
                private readonly Func<BrightChainQueryContext, JsonNode, T> _shaper;
                private readonly Type _contextType;
                private readonly string _partitionKey;
                private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
                private readonly bool _standAloneStateManager;
                private readonly CancellationToken _cancellationToken;
                private readonly IConcurrencyDetector _concurrencyDetector;

                private IAsyncEnumerator<JsonNode> _enumerator;

                public AsyncEnumerator(QueryingEnumerable<T> queryingEnumerable, CancellationToken cancellationToken)
                {
                    _queryingEnumerable = queryingEnumerable;
                    _brightChainQueryContext = queryingEnumerable._brightChainQueryContext;
                    _shaper = queryingEnumerable._shaper;
                    _selectExpression = queryingEnumerable._selectExpression;
                    _contextType = queryingEnumerable._contextType;
                    _partitionKey = queryingEnumerable._partitionKey;
                    _queryLogger = queryingEnumerable._queryLogger;
                    _standAloneStateManager = queryingEnumerable._standAloneStateManager;
                    _cancellationToken = cancellationToken;

                    _concurrencyDetector = queryingEnumerable._threadSafetyChecksEnabled
                        ? _brightChainQueryContext.ConcurrencyDetector
                        : null;
                }

                public T Current { get; private set; }

                public async ValueTask<bool> MoveNextAsync()
                {
                    try
                    {
                        _concurrencyDetector?.EnterCriticalSection();

                        try
                        {
                            if (_enumerator == null)
                            {
                                var sqlQuery = _queryingEnumerable.GenerateQuery();

                                EntityFrameworkEventSource.Log.QueryExecuting();

                                _enumerator = _brightChainQueryContext.BrightChainClient
                                    .ExecuteSqlQueryAsync(
                                        _selectExpression.Container,
                                        _partitionKey,
                                        sqlQuery)
                                    .GetAsyncEnumerator(_cancellationToken);
                                _brightChainQueryContext.InitializeStateManager(_standAloneStateManager);
                            }

                            var hasNext = await _enumerator.MoveNextAsync().ConfigureAwait(false);

                            Current
                                = hasNext
                                    ? _shaper(_brightChainQueryContext, _enumerator.Current)
                                    : default;

                            return hasNext;
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

                public ValueTask DisposeAsync()
                {
                    var enumerator = _enumerator;
                    if (enumerator != null)
                    {
                        _enumerator = null;
                        return enumerator.DisposeAsync();
                    }
                    return default;
                }
            }
        }
    }
}
