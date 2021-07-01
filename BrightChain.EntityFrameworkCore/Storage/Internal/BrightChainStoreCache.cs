// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Infrastructure.Internal;
using System.Collections.Concurrent;
using System.Threading;

namespace BrightChain.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class BrightChainStoreCache : IBrightChainStoreCache
    {
        private readonly IBrightChainTableFactory _tableFactory;
        private readonly bool _useNameMatching;
        private readonly ConcurrentDictionary<string, IBrightChainStore> _namedStores;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public BrightChainStoreCache(
            IBrightChainTableFactory tableFactory,
            IBrightChainSingletonOptions? options)
        {
            this._tableFactory = tableFactory;

            if (options?.DatabaseRoot != null)
            {
                this._useNameMatching = true;

                LazyInitializer.EnsureInitialized(
                    ref options.DatabaseRoot.Instance,
                    () => new ConcurrentDictionary<string, IBrightChainStore>());

                this._namedStores = (ConcurrentDictionary<string, IBrightChainStore>)options.DatabaseRoot.Instance;
            }
            else
            {
                this._namedStores = new ConcurrentDictionary<string, IBrightChainStore>();
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IBrightChainStore GetStore(string name)
            => this._namedStores.GetOrAdd(name, _ => new BrightChainStore(this._tableFactory, this._useNameMatching));
    }
}
