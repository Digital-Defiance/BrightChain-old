using BrightChain.Interfaces;
using BrightChain.Models.Events;
using CSharpTest.Net.Collections;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrightChain.Services
{
    /// <summary>
    /// Special intance of CacheManager that is BTree backed. Works for disk and memory and has transaction support.
    /// </summary>
    /// <typeparam name="Tkey"></typeparam>
    /// <typeparam name="Tvalue"></typeparam>
    public abstract class BPlusTreeCacheManager<Tkey, Tvalue> : ICacheManager<Tkey, Tvalue>, IBPlusTreeCacheManager<Tkey, Tvalue>
        where Tvalue : new()
    {
        static readonly ManualResetEvent mreStop = new ManualResetEvent(false);

        public static bool EnableCount { get; } = false;

        protected readonly ILogger logger;
        internal readonly BPlusTree<Tkey, Tvalue> tree;
        protected readonly BlockCacheExpirationCache<Tkey, Tvalue> expirationCache;

        public event ICacheManager<Tkey, Tvalue>.KeyAddedEventHandler KeyAdded;
        public event ICacheManager<Tkey, Tvalue>.KeyRemovedEventHandler KeyRemoved;
        public event ICacheManager<Tkey, Tvalue>.CacheMissEventHandler CacheMiss;

        public BPlusTreeCacheManager(ILogger logger, BPlusTree<Tkey, Tvalue> tree)
        {
            this.logger = logger;
            this.logger.LogInformation(String.Format("<%s>: initalizing", nameof(BPlusTreeCacheManager<Tkey, Tvalue>)));
            this.tree = tree;
            ApplyTreeActions(new Action<BPlusTree<Tkey, Tvalue>>[] {
                GarbageCollect
            });
        }

        public BPlusTreeCacheManager(BPlusTreeCacheManager<Tkey, Tvalue> other)
        {
            this.logger = other.logger;
            this.tree = other.tree;
            this.expirationCache = other.expirationCache;
        }

        internal void GarbageCollect(BPlusTree<Tkey, Tvalue> tree)
        {
            // TODO: get (keep and store elsewhere?) list of keys expiring this second in the given tree (and which haven't been required longer by renewed/extended contracts)
            // remove expired entries
            this.logger.LogInformation("GarbageCollect");
        }

        public void ApplyTreeActions(Action<BPlusTree<Tkey, Tvalue>>[] actions)
        {
            mreStop.Reset();
            if (EnableCount) this.tree.EnableCount();

            var result = actions.Select(t =>
                Task.Run(() => t(this.tree))
            ).ToArray();

            mreStop.Set();
            Task.WaitAll(result);

            Trace.TraceInformation("Dictionary.Count = {0}", this.tree.Count);
        }

        public Tvalue Get(Tkey key)
        {
            this.logger.LogInformation(String.Format("Get({0})", key));
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
            this.logger.LogInformation(String.Format("Set({0})", key));
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
            this.logger.LogInformation(String.Format("Drop({0})", key));
            if (!noCheckContains && !Contains(key)) return false;
            var eventArgs = new CacheEventArgs<Tkey, Tvalue>();
            eventArgs.KeyValue = new KeyValuePair<Tkey, Tvalue>(key, this.tree[key]);
            this.tree.Remove(key);
            KeyRemoved?.Invoke(this, eventArgs);
            return true;
        }

        public bool TreeIsEqual(BPlusTree<Tkey, Tvalue> other) =>
            this.tree is null ? false : this.tree.Equals(other);

        public bool TreeIsSame(BPlusTree<Tkey, Tvalue> other) =>
            this.tree is null ? false : object.ReferenceEquals(this.tree, other);

        public void Commit()
        {
            if (this.tree is null)
                throw new NullReferenceException(nameof(this.tree));

            this.tree.Commit();
        }

        public void Rollback()
        {
            if (this.tree is null)
                throw new NullReferenceException(nameof(this.tree));

            this.tree.Rollback();
        }
    }
}
