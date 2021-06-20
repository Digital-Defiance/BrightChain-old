using CSharpTest.Net.Collections;
using BrightChain.Interfaces;
using BrightChain.Models;
using BrightChain.Models.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrightChain.Services
{
    public abstract class BPlusTreeCacheManager<Tkey, Tvalue> : ICacheManager<Tkey, Tvalue>
    {
        static readonly ManualResetEvent mreStop = new ManualResetEvent(false);

        public static bool EnableCount { get; } = false;

        internal readonly BPlusTree<Tkey, Tvalue> tree;
        protected readonly BlockCacheExpirationCache<Tkey, Tvalue> expirationCache;
        protected readonly ILogger logger;

        public event ICacheManager<Tkey, Tvalue>.KeyAddedEventHandler KeyAdded;
        public event ICacheManager<Tkey, Tvalue>.KeyRemovedEventHandler KeyRemoved;
        public event ICacheManager<Tkey, Tvalue>.CacheMissEventHandler CacheMiss;

        public BPlusTreeCacheManager(ILogger logger, BPlusTree<Tkey, Tvalue> tree)
        {
            this.logger = logger;
            this.logger.LogInformation(String.Format("<%s>: initalizing", nameof(BPlusTreeCacheManager<Tkey, Tvalue>)));
            this.tree = tree;
            ApplyTreeActions(new Action<BPlusTree<Tkey, Tvalue>>[] {
                CacheGarbageCollectionAction<Tkey, Tvalue>.GarbageCollect
            });
        }

        public void ApplyTreeActions(Action<BPlusTree<Tkey, Tvalue>>[] actions)
        {
            mreStop.Reset();
            if (EnableCount) this.tree.EnableCount();

            var result = actions.Select(t =>
                Task.Run(() => t(this.tree)))
                    .ToArray();

            mreStop.Set();
            Task.WaitAll(result);

            Trace.TraceInformation("Dictionary.Count = {0}", this.tree.Count);
        }

        public Tvalue Get(Tkey key)
        {
            Tvalue value = default;
            var contains = this.tree.TryGetValue(key, out value);
            if (!contains)
            {
                var eventArgs = new CacheEventArgs<Tkey, Tvalue>();
                eventArgs.KeyValue = new KeyValuePair<Tkey, Tvalue>(key, value);
                CacheMiss?.Invoke(this, eventArgs);
                throw new IndexOutOfRangeException(message: nameof(key));
            }
            return value;
        }

        public void Set(Tkey key, Tvalue value)
        {
            this.tree.Add(
                key: key,
                value: value); // TODO: Expiration in btree - look at having an expiration tree grouped by blocks expiring that second?
            var eventArgs = new CacheEventArgs<Tkey, Tvalue>();
            eventArgs.KeyValue = new KeyValuePair<Tkey, Tvalue>(key, value);
            KeyAdded?.Invoke(this, eventArgs);
        }

        public bool Contains(Tkey key) =>
            this.tree.ContainsKey(key);

        public bool Drop(Tkey key, bool noCheckContains = true)
        {
            if (!noCheckContains && !Contains(key)) return false;
            var eventArgs = new CacheEventArgs<Tkey, Tvalue>();
            eventArgs.KeyValue = new KeyValuePair<Tkey, Tvalue>(key, this.tree[key]);
            this.tree.Remove(key);
            KeyRemoved?.Invoke(this, eventArgs);
            return true;
        }
    }
}
