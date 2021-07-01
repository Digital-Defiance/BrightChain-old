// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Extensions;
using BrightChain.EntityFrameworkCore.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
        private sealed class QueryingEnumerable<T> : IAsyncEnumerable<T>, IEnumerable<T>, IQueryingEnumerable
        {
            private readonly QueryContext _queryContext;
            private readonly IEnumerable<ValueBuffer> _innerEnumerable;
            private readonly Func<QueryContext, ValueBuffer, T> _shaper;
            private readonly Type _contextType;
            private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
            private readonly bool _standAloneStateManager;
            private readonly bool _threadSafetyChecksEnabled;

            public QueryingEnumerable(
                QueryContext queryContext,
                IEnumerable<ValueBuffer> innerEnumerable,
                Func<QueryContext, ValueBuffer, T> shaper,
                Type contextType,
                bool standAloneStateManager,
                bool threadSafetyChecksEnabled)
            {
                this._queryContext = queryContext;
                this._innerEnumerable = innerEnumerable;
                this._shaper = shaper;
                this._contextType = contextType;
                this._queryLogger = queryContext.QueryLogger;
                this._standAloneStateManager = standAloneStateManager;
                this._threadSafetyChecksEnabled = threadSafetyChecksEnabled;
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
                => new Enumerator(this, cancellationToken);

            public IEnumerator<T> GetEnumerator()
                => new Enumerator(this);

            IEnumerator IEnumerable.GetEnumerator()
                => this.GetEnumerator();

            public string ToQueryString()
                => BrightChainStrings.NoQueryStrings;

            private sealed class Enumerator : IEnumerator<T>, IAsyncEnumerator<T>
            {
                private readonly QueryContext _queryContext;
                private readonly IEnumerable<ValueBuffer> _innerEnumerable;
                private readonly Func<QueryContext, ValueBuffer, T> _shaper;
                private readonly Type _contextType;
                private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
                private readonly bool _standAloneStateManager;
                private readonly CancellationToken _cancellationToken;
                private readonly IConcurrencyDetector? _concurrencyDetector;

                private IEnumerator<ValueBuffer>? _enumerator;

                public Enumerator(QueryingEnumerable<T> queryingEnumerable, CancellationToken cancellationToken = default)
                {
                    this._queryContext = queryingEnumerable._queryContext;
                    this._innerEnumerable = queryingEnumerable._innerEnumerable;
                    this._shaper = queryingEnumerable._shaper;
                    this._contextType = queryingEnumerable._contextType;
                    this._queryLogger = queryingEnumerable._queryLogger;
                    this._standAloneStateManager = queryingEnumerable._standAloneStateManager;
                    this._cancellationToken = cancellationToken;
                    this.Current = default!;

                    this._concurrencyDetector = queryingEnumerable._threadSafetyChecksEnabled
                        ? this._queryContext.ConcurrencyDetector
                        : null;
                }

                public T Current { get; private set; }

                object IEnumerator.Current
                    => this.Current!;

                public bool MoveNext()
                {
                    try
                    {
                        this._concurrencyDetector?.EnterCriticalSection();

                        try
                        {
                            return this.MoveNextHelper();
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

                public ValueTask<bool> MoveNextAsync()
                {
                    try
                    {
                        this._concurrencyDetector?.EnterCriticalSection();

                        try
                        {
                            this._cancellationToken.ThrowIfCancellationRequested();

                            return new ValueTask<bool>(this.MoveNextHelper());
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

                private bool MoveNextHelper()
                {
                    if (this._enumerator == null)
                    {
                        EntityFrameworkEventSource.Log.QueryExecuting();

                        this._enumerator = this._innerEnumerable.GetEnumerator();
                        this._queryContext.InitializeStateManager(this._standAloneStateManager);
                    }

                    var hasNext = this._enumerator.MoveNext();

                    this.Current = hasNext
                        ? this._shaper(this._queryContext, this._enumerator.Current)
                        : default!;

                    return hasNext;
                }

                public void Dispose()
                {
                    this._enumerator?.Dispose();
                    this._enumerator = null;
                }

                public ValueTask DisposeAsync()
                {
                    var enumerator = this._enumerator;
                    this._enumerator = null;

                    return enumerator.DisposeAsyncIfAvailable();
                }

                public void Reset()
                    => throw new NotSupportedException(CoreStrings.EnumerableResetNotSupported);
            }
        }
    }
}
