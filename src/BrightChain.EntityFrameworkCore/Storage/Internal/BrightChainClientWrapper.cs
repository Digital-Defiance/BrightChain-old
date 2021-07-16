// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using BrightChain.Engine.Client;
using BrightChain.EntityFrameworkCore.Diagnostics.Internal;
using BrightChain.EntityFrameworkCore.Infrastructure.Internal;
using BrightChain.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace BrightChain.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class BrightChainClientWrapper : IBrightChainClientWrapper
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static readonly string DefaultPartitionKey = "__partitionKey";

        private readonly ISingletonBrightChainClientWrapper _singletonWrapper;
        private readonly string _databaseId;
        private readonly IExecutionStrategyFactory _executionStrategyFactory;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Command> _commandLogger;
        private readonly bool? _enableContentResponseOnWrite;

        static BrightChainClientWrapper()
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public BrightChainClientWrapper(
            ISingletonBrightChainClientWrapper singletonWrapper,
            IDbContextOptions dbContextOptions,
            IExecutionStrategyFactory executionStrategyFactory,
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger)
        {
            var options = dbContextOptions.FindExtension<BrightChainOptionsExtension>();

            _singletonWrapper = singletonWrapper;
            _databaseId = options!.DatabaseName;
            _executionStrategyFactory = executionStrategyFactory;
            _commandLogger = commandLogger;
            _enableContentResponseOnWrite = options.EnableContentResponseOnWrite;
        }

        private BrightChainClient Client
            => _singletonWrapper.Client;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CreateDatabaseIfNotExists()
        {
            return _executionStrategyFactory.Create().Execute(
                           (object?)null, CreateDatabaseIfNotExistsOnce, null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CreateDatabaseIfNotExistsOnce(
            DbContext? context,
            object? state)
        {
            return CreateDatabaseIfNotExistsOnceAsync(context, state).GetAwaiter().GetResult();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Task<bool> CreateDatabaseIfNotExistsAsync(
            CancellationToken cancellationToken = default)
        {
            return _executionStrategyFactory.Create().ExecuteAsync(
                           (object?)null, CreateDatabaseIfNotExistsOnceAsync, null, cancellationToken);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual async Task<bool> CreateDatabaseIfNotExistsOnceAsync(
            DbContext? _,
            object? __,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
            //var response = await Client.CreateDatabaseIfNotExistsAsync(_databaseId, cancellationToken: cancellationToken)
            //    .ConfigureAwait(false);

            //return response.StatusCode == HttpStatusCode.Created;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool DeleteDatabase()
        {
            return _executionStrategyFactory.Create().Execute((object?)null, DeleteDatabaseOnce, null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool DeleteDatabaseOnce(
            DbContext? context,
            object? state)
        {
            return DeleteDatabaseOnceAsync(context, state).GetAwaiter().GetResult();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Task<bool> DeleteDatabaseAsync(
            CancellationToken cancellationToken = default)
        {
            return _executionStrategyFactory.Create().ExecuteAsync(
                           (object?)null, DeleteDatabaseOnceAsync, null, cancellationToken);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual async Task<bool> DeleteDatabaseOnceAsync(
            DbContext? _,
            object? __,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
            //using var response = await Client.GetDatabase(_databaseId).DeleteStreamAsync(cancellationToken: cancellationToken)
            //    .ConfigureAwait(false);
            //if (response.StatusCode == HttpStatusCode.NotFound)
            //{
            //    return false;
            //}

            //response.EnsureSuccessStatusCode();
            //return response.StatusCode == HttpStatusCode.NoContent;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CreateContainerIfNotExists(
            string containerId,
            string partitionKey)
        {
            return _executionStrategyFactory.Create().Execute(
                           (containerId, partitionKey), CreateContainerIfNotExistsOnce, null);
        }

        private bool CreateContainerIfNotExistsOnce(
            DbContext context,
            (string ContainerId, string PartitionKey) parameters)
        {
            return CreateContainerIfNotExistsOnceAsync(context, parameters).GetAwaiter().GetResult();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Task<bool> CreateContainerIfNotExistsAsync(
            string containerId,
            string partitionKey,
            CancellationToken cancellationToken = default)
        {
            return _executionStrategyFactory.Create().ExecuteAsync(
                           (containerId, partitionKey), CreateContainerIfNotExistsOnceAsync, null, cancellationToken);
        }

        private async Task<bool> CreateContainerIfNotExistsOnceAsync(
            DbContext _,
            (string ContainerId, string PartitionKey) parameters,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CreateItem(
            string containerId,
            JsonDocument document,
            IUpdateEntry entry)
        {
            return _executionStrategyFactory.Create().Execute(
                           (containerId, document, entry), CreateItemOnce, null);
        }

        private bool CreateItemOnce(
            DbContext context,
            (string ContainerId, JsonDocument Document, IUpdateEntry Entry) parameters)
        {
            return CreateItemOnceAsync(context, parameters).GetAwaiter().GetResult();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Task<bool> CreateItemAsync(
            string containerId,
            JsonDocument document,
            IUpdateEntry updateEntry,
            CancellationToken cancellationToken = default)
        {
            return _executionStrategyFactory.Create().ExecuteAsync(
                           (containerId, document, updateEntry), CreateItemOnceAsync, null, cancellationToken);
        }

        private async Task<bool> CreateItemOnceAsync(
            DbContext _,
            (string ContainerId, JsonDocument Document, IUpdateEntry Entry) parameters,
            CancellationToken cancellationToken = default)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, new UTF8Encoding(), bufferSize: 1024, leaveOpen: false);
            using var jsonWriter = new System.Text.Json.Utf8JsonWriter(utf8Json: stream, options: default);
            JsonSerializer.Serialize(jsonWriter, parameters.Document);
            await jsonWriter.FlushAsync(cancellationToken).ConfigureAwait(false);

            var entry = parameters.Entry;

            throw new NotImplementedException();
            //using var response = await container.CreateItemStreamAsync(stream, partitionKey, itemRequestOptions, cancellationToken)
            //    .ConfigureAwait(false);
            //ProcessResponse(response, entry);

            //return response.StatusCode == HttpStatusCode.Created;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool ReplaceItem(
            string collectionId,
            string documentId,
            JsonDocument document,
            IUpdateEntry entry)
        {
            return _executionStrategyFactory.Create().Execute(
                           (collectionId, documentId, document, entry),
                           ReplaceItemOnce,
                           null);
        }

        private bool ReplaceItemOnce(
            DbContext context,
            (string ContainerId, string ItemId, JsonDocument Document, IUpdateEntry Entry) parameters)
        {
            return ReplaceItemOnceAsync(context, parameters).GetAwaiter().GetResult();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Task<bool> ReplaceItemAsync(
            string collectionId,
            string documentId,
            JsonDocument document,
            IUpdateEntry updateEntry,
            CancellationToken cancellationToken = default)
        {
            return _executionStrategyFactory.Create().ExecuteAsync(
                           (collectionId, documentId, document, updateEntry),
                           ReplaceItemOnceAsync,
                           null,
                           cancellationToken);
        }

        private async Task<bool> ReplaceItemOnceAsync(
            DbContext _,
            (string ContainerId, string ItemId, JsonDocument Document, IUpdateEntry Entry) parameters,
            CancellationToken cancellationToken = default)
        {
            using var stream = new MemoryStream();
            using var jsonWriter = new Utf8JsonWriter(utf8Json: stream, options: default);
            JsonSerializer.Serialize(jsonWriter, parameters.Document);
            await jsonWriter.FlushAsync(cancellationToken).ConfigureAwait(false);

            var entry = parameters.Entry;
            throw new NotImplementedException();
            //using var response = await container.ReplaceItemStreamAsync(
            //        stream, parameters.ItemId, partitionKey, itemRequestOptions, cancellationToken)
            //    .ConfigureAwait(false);
            //ProcessResponse(response, entry);

            //return response.StatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool DeleteItem(
            string containerId,
            string documentId,
            IUpdateEntry entry)
        {
            return _executionStrategyFactory.Create().Execute(
                           (containerId, documentId, entry), DeleteItemOnce, null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool DeleteItemOnce(
            DbContext context,
            (string ContainerId, string DocumentId, IUpdateEntry Entry) parameters)
        {
            return DeleteItemOnceAsync(context, parameters).GetAwaiter().GetResult();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Task<bool> DeleteItemAsync(
            string containerId,
            string documentId,
            IUpdateEntry entry,
            CancellationToken cancellationToken = default)
        {
            return _executionStrategyFactory.Create().ExecuteAsync(
                           (containerId, documentId, entry), DeleteItemOnceAsync, null, cancellationToken);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual async Task<bool> DeleteItemOnceAsync(
            DbContext? _,
            (string ContainerId, string DocumentId, IUpdateEntry Entry) parameters,
            CancellationToken cancellationToken = default)
        {
            var entry = parameters.Entry;
            throw new NotImplementedException();
            //using var response = await items.DeleteItemStreamAsync(
            //        parameters.DocumentId, partitionKey, itemRequestOptions, cancellationToken: cancellationToken)
            //    .ConfigureAwait(false);
            //ProcessResponse(response, entry);

            //return response.StatusCode == HttpStatusCode.NoContent;
        }

        //private static void ProcessResponse(ResponseMessage response, IUpdateEntry entry)
        //{
        //    response.EnsureSuccessStatusCode();
        //    var etagProperty = entry.EntityType.GetETagProperty();
        //    if (etagProperty != null && entry.EntityState != EntityState.Deleted)
        //    {
        //        entry.SetStoreGeneratedValue(etagProperty, response.Headers.ETag);
        //    }

        //    var jObjectProperty = entry.EntityType.FindProperty(StoreKeyConvention.JObjectPropertyName);
        //    if (jObjectProperty != null
        //        && jObjectProperty.ValueGenerated == ValueGenerated.OnAddOrUpdate
        //        && response.Content != null)
        //    {
        //        using var responseStream = response.Content;
        //        using var reader = new StreamReader(responseStream);
        //        using var jsonReader = new JsonTextReader(reader);

        //        var createdDocument = Serializer.Deserialize<JObject>(jsonReader);

        //        entry.SetStoreGeneratedValue(jObjectProperty, createdDocument);
        //    }
        //}

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<JsonDocument> ExecuteSqlQuery(
            string containerId,
            string? partitionKey,
            BrightChainSqlQuery query)
        {
            _commandLogger.ExecutingSqlQuery(containerId, partitionKey, query);

            return new DocumentEnumerable(this, containerId, partitionKey, query);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IAsyncEnumerable<JsonDocument> ExecuteSqlQueryAsync(
            string containerId,
            string? partitionKey,
            BrightChainSqlQuery query)
        {
            _commandLogger.ExecutingSqlQuery(containerId, partitionKey, query);

            return new DocumentAsyncEnumerable(this, containerId, partitionKey, query);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual JsonNode ExecuteReadItem(
            string containerId,
            string? partitionKey,
            string resourceId)
        {
            _commandLogger.ExecutingReadItem(containerId, partitionKey, resourceId);

            var responseMessage = CreateSingleItemQuery(
                containerId, partitionKey, resourceId).GetAwaiter().GetResult();

            return JObjectFromReadItemResponseMessage(responseMessage);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual async Task<JsonNode> ExecuteReadItemAsync(
            string containerId,
            string? partitionKey,
            string resourceId,
            CancellationToken cancellationToken = default)
        {
            _commandLogger.ExecutingReadItem(containerId, partitionKey, resourceId);

            var responseMessage = await CreateSingleItemQuery(
                    containerId, partitionKey, resourceId, cancellationToken)
                .ConfigureAwait(false);

            return JObjectFromReadItemResponseMessage(responseMessage);
        }

        private static JsonNode JObjectFromReadItemResponseMessage(HttpWebResponse responseMessage)
        {
            if (responseMessage.StatusCode != HttpStatusCode.OK)
            {
                throw new BrightChain.Engine.Exceptions.BrightChainException(nameof(responseMessage.StatusCode));
            }

            var responseStream = responseMessage.GetResponseStream();
            using var reader = new StreamReader(responseStream);
            var jObject = JsonSerializer.Deserialize<JsonNode>(utf8Json: new ReadOnlySpan<byte>(reader.ReadToEnd().ToCharArray().Select(c => (byte)c).ToArray()), options: default);
            throw new NotImplementedException();
            //return new JsonDocument(new JProperty("c", jObject));
        }

        private async Task<HttpWebResponse> CreateSingleItemQuery(
            string containerId,
            string? partitionKey,
            string resourceId,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
            //var container = Client.GetDatabase(_databaseId).GetContainer(containerId);

            //return await container.ReadItemStreamAsync(
            //        resourceId,
            //        string.IsNullOrEmpty(partitionKey) ? PartitionKey.None : new PartitionKey(partitionKey),
            //        cancellationToken: cancellationToken)
            //    .ConfigureAwait(false);
        }

        public bool CreateItem(string containerId, JsonNode document, IUpdateEntry entry)
        {
            throw new NotImplementedException();
        }

        public bool ReplaceItem(string collectionId, string documentId, JsonNode document, IUpdateEntry entry)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CreateItemAsync(string containerId, JsonNode document, IUpdateEntry updateEntry, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ReplaceItemAsync(string collectionId, string documentId, JsonNode document, IUpdateEntry updateEntry, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        JsonNode IBrightChainClientWrapper.ExecuteReadItem(string containerId, string partitionKey, string resourceId)
        {
            throw new NotImplementedException();
        }

        Task<JsonNode> IBrightChainClientWrapper.ExecuteReadItemAsync(string containerId, string partitionKey, string resourceId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        IEnumerable<JsonNode> IBrightChainClientWrapper.ExecuteSqlQuery(string containerId, string partitionKey, BrightChainSqlQuery query)
        {
            throw new NotImplementedException();
        }

        IAsyncEnumerable<JsonNode> IBrightChainClientWrapper.ExecuteSqlQueryAsync(string containerId, string partitionKey, BrightChainSqlQuery query)
        {
            throw new NotImplementedException();
        }

        private sealed class DocumentEnumerable : IEnumerable<JsonDocument>
        {
            private readonly IBrightChainClientWrapper _brightChainClient;
            private readonly string _containerId;
            private readonly string? _partitionKey;
            private readonly BrightChainSqlQuery _brightChainSqlQuery;

            public DocumentEnumerable(
                IBrightChainClientWrapper brightChainClient,
                string containerId,
                string? partitionKey,
                BrightChainSqlQuery brightChainSqlQuery)
            {
                _brightChainClient = brightChainClient;
                _containerId = containerId;
                _partitionKey = partitionKey;
                _brightChainSqlQuery = brightChainSqlQuery;
            }

            public IEnumerator<JsonDocument> GetEnumerator()
            {
                return new Enumerator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private sealed class Enumerator : IEnumerator<JsonDocument>
            {
                private readonly IBrightChainClientWrapper _brightChainClientWrapper;
                private readonly string _containerId;
                private readonly string? _partitionKey;
                private readonly BrightChainSqlQuery _brightChainSqlQuery;

                private JsonDocument? _current;
                private HttpWebResponse? _responseMessage;
                private Stream? _responseStream;
                private StreamReader? _reader;

                public Enumerator(DocumentEnumerable documentEnumerable)
                {
                    _brightChainClientWrapper = documentEnumerable._brightChainClient;
                    _containerId = documentEnumerable._containerId;
                    _partitionKey = documentEnumerable._partitionKey;
                    _brightChainSqlQuery = documentEnumerable._brightChainSqlQuery;
                }

                public JsonDocument Current => _current ?? throw new InvalidOperationException();

                object IEnumerator.Current
                    => Current;

                private void ResetRead()
                {
                    _reader?.Dispose();
                    _reader = null;
                    _responseStream?.Dispose();
                    _responseStream = null;
                }

                public void Dispose()
                {
                    ResetRead();

                    _responseMessage?.Dispose();
                    _responseMessage = null;
                }

                public void Reset()
                {
                    throw new NotSupportedException(CoreStrings.EnumerableResetNotSupported);
                }

                public bool MoveNext()
                {
                    throw new NotImplementedException();
                }
            }
        }

        private sealed class DocumentAsyncEnumerable : IAsyncEnumerable<JsonDocument>
        {
            private readonly IBrightChainClientWrapper _brightChainClient;
            private readonly string _containerId;
            private readonly string? _partitionKey;
            private readonly BrightChainSqlQuery _brightChainSqlQuery;

            public DocumentAsyncEnumerable(
                IBrightChainClientWrapper brightChainClient,
                string containerId,
                string? partitionKey,
                BrightChainSqlQuery brightChainSqlQuery)
            {
                _brightChainClient = brightChainClient;
                _containerId = containerId;
                _partitionKey = partitionKey;
                _brightChainSqlQuery = brightChainSqlQuery;
            }

            public IAsyncEnumerator<JsonDocument> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new AsyncEnumerator(this, cancellationToken);
            }

            private sealed class AsyncEnumerator : IAsyncEnumerator<JsonDocument>
            {
                private readonly IBrightChainClientWrapper _brightChainClientWrapper;
                private readonly string _containerId;
                private readonly string? _partitionKey;
                private readonly BrightChainSqlQuery _brightChainSqlQuery;
                private readonly CancellationToken _cancellationToken;

                private JsonDocument? _current;
                private HttpWebResponse? _responseMessage;
                private Stream? _responseStream;
                private StreamReader? _reader;

                public JsonDocument Current => _current ?? throw new InvalidOperationException();

                public AsyncEnumerator(DocumentAsyncEnumerable documentEnumerable, CancellationToken cancellationToken)
                {
                    _brightChainClientWrapper = documentEnumerable._brightChainClient;
                    _containerId = documentEnumerable._containerId;
                    _partitionKey = documentEnumerable._partitionKey;
                    _brightChainSqlQuery = documentEnumerable._brightChainSqlQuery;
                    _cancellationToken = cancellationToken;
                }

                private async Task ResetReadAsync()
                {
                    await _reader.DisposeAsyncIfAvailable().ConfigureAwait(false);
                    _reader = null;
                    await _responseStream.DisposeAsyncIfAvailable().ConfigureAwait(false);
                    _responseStream = null;
                }

                public async ValueTask DisposeAsync()
                {
                    await ResetReadAsync().ConfigureAwait(false);

                    await _responseMessage.DisposeAsyncIfAvailable().ConfigureAwait(false);
                    _responseMessage = null;
                }

                public ValueTask<bool> MoveNextAsync()
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
