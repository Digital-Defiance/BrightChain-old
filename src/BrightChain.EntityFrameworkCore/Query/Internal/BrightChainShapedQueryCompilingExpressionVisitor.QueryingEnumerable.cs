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
                this._brightChainQueryContext = brightChainQueryContext;
                this._sqlExpressionFactory = sqlExpressionFactory;
                this._querySqlGeneratorFactory = querySqlGeneratorFactory;
                this._selectExpression = selectExpression;
                this._shaper = shaper;
                this._contextType = contextType;
                this._queryLogger = brightChainQueryContext.QueryLogger;
                this._standAloneStateManager = standAloneStateManager;
                this._threadSafetyChecksEnabled = threadSafetyChecksEnabled;

                var partitionKey = selectExpression.GetPartitionKey(brightChainQueryContext.ParameterValues);
                if (partitionKey != null && partitionKeyFromExtension != null && partitionKeyFromExtension != partitionKey)
                {
                    throw new InvalidOperationException(BrightChainStrings.PartitionKeyMismatch(partitionKeyFromExtension, partitionKey));
                }

                this._partitionKey = partitionKey ?? partitionKeyFromExtension;
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
                return this.GetEnumerator();
            }

            private BrightChainSqlQuery GenerateQuery()
            {
                return this._querySqlGeneratorFactory.Create().GetSqlQuery(
                                   (SelectExpression)new InExpressionValuesExpandingExpressionVisitor(
                                           this._sqlExpressionFactory,
                                           this._brightChainQueryContext.ParameterValues)
                                       .Visit(this._selectExpression),
                                   this._brightChainQueryContext.ParameterValues);
            }

            public string ToQueryString()
            {
                var sqlQuery = this.GenerateQuery();
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
                    this._queryingEnumerable = queryingEnumerable;
                    this._brightChainQueryContext = queryingEnumerable._brightChainQueryContext;
                    this._shaper = queryingEnumerable._shaper;
                    this._selectExpression = queryingEnumerable._selectExpression;
                    this._contextType = queryingEnumerable._contextType;
                    this._partitionKey = queryingEnumerable._partitionKey;
                    this._queryLogger = queryingEnumerable._queryLogger;
                    this._standAloneStateManager = queryingEnumerable._standAloneStateManager;

                    this._concurrencyDetector = queryingEnumerable._threadSafetyChecksEnabled
                        ? this._brightChainQueryContext.ConcurrencyDetector
                        : null;
                }

                public T Current { get; private set; }

                object IEnumerator.Current
                    => this.Current;

                public bool MoveNext()
                {
                    try
                    {
                        this._concurrencyDetector?.EnterCriticalSection();

                        try
                        {
                            if (this._enumerator == null)
                            {
                                var sqlQuery = this._queryingEnumerable.GenerateQuery();

                                EntityFrameworkEventSource.Log.QueryExecuting();

                                this._enumerator = this._brightChainQueryContext.BrightChainClient
                                    .ExecuteSqlQuery(
                                        this._selectExpression.Container,
                                        this._partitionKey,
                                        sqlQuery)
                                    .GetEnumerator();
                                this._brightChainQueryContext.InitializeStateManager(this._standAloneStateManager);
                            }

                            var hasNext = this._enumerator.MoveNext();

                            this.Current
                                = hasNext
                                    ? this._shaper(this._brightChainQueryContext, this._enumerator.Current)
                                    : default;

                            return hasNext;
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
                    this._enumerator?.Dispose();
                    this._enumerator = null;
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
                    this._queryingEnumerable = queryingEnumerable;
                    this._brightChainQueryContext = queryingEnumerable._brightChainQueryContext;
                    this._shaper = queryingEnumerable._shaper;
                    this._selectExpression = queryingEnumerable._selectExpression;
                    this._contextType = queryingEnumerable._contextType;
                    this._partitionKey = queryingEnumerable._partitionKey;
                    this._queryLogger = queryingEnumerable._queryLogger;
                    this._standAloneStateManager = queryingEnumerable._standAloneStateManager;
                    this._cancellationToken = cancellationToken;

                    this._concurrencyDetector = queryingEnumerable._threadSafetyChecksEnabled
                        ? this._brightChainQueryContext.ConcurrencyDetector
                        : null;
                }

                public T Current { get; private set; }

                public async ValueTask<bool> MoveNextAsync()
                {
                    try
                    {
                        this._concurrencyDetector?.EnterCriticalSection();

                        try
                        {
                            if (this._enumerator == null)
                            {
                                var sqlQuery = this._queryingEnumerable.GenerateQuery();

                                EntityFrameworkEventSource.Log.QueryExecuting();

                                this._enumerator = this._brightChainQueryContext.BrightChainClient
                                    .ExecuteSqlQueryAsync(
                                        this._selectExpression.Container,
                                        this._partitionKey,
                                        sqlQuery)
                                    .GetAsyncEnumerator(this._cancellationToken);
                                this._brightChainQueryContext.InitializeStateManager(this._standAloneStateManager);
                            }

                            var hasNext = await this._enumerator.MoveNextAsync().ConfigureAwait(false);

                            this.Current
                                = hasNext
                                    ? this._shaper(this._brightChainQueryContext, this._enumerator.Current)
                                    : default;

                            return hasNext;
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

                public ValueTask DisposeAsync()
                {
                    var enumerator = this._enumerator;
                    if (enumerator != null)
                    {
                        this._enumerator = null;
                        return enumerator.DisposeAsync();
                    }
                    return default;
                }
            }
        }
    }
}
