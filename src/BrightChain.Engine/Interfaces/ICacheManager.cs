namespace BrightChain.Engine.Interfaces
{
    using System;
    using BrightChain.Engine.Models.Events;

    /// <summary>
    /// Basic guaranteed members of the cache system. Notably the system is heavily dependent on the BPlusTree caches which have transaction support.
    /// </summary>
    /// <typeparam name="Tkey"></typeparam>
    /// <typeparam name="Tvalue"></typeparam>
    public interface ICacheManager<Tkey, Tvalue>
        where Tkey : IComparable<Tkey>
    {
        /// <summary>
        /// Retrieves an object from the cache if it is present
        /// </summary>
        /// <param name="blockHash">key to retrieve</param>
        /// <returns>returns requested block or throws</returns>
        Tvalue Get(Tkey blockHash);

        /// <summary>
        /// Adds a key to the cache if it is not already present
        /// </summary>
        /// <param name="key">key to palce in the cache</param>
        void Set(Tkey key, Tvalue value);

        /// <summary>
        /// 
        /// Returns whether the cache manager has the given key and it is not expired
        /// </summary>
        /// <param name="key">key to check the collection for</param>
        /// <returns>boolean with whether key is present</returns>
        bool Contains(Tkey key);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="noCheckContains"></param>
        /// <returns></returns>
        bool Drop(Tkey key, bool noCheckContains = false);

        delegate void KeyAddedEventHandler(object sender, CacheEventArgs<Tkey, Tvalue> cacheEventArgs);

        delegate void KeyExpiredEventHandler(object sender, CacheEventArgs<Tkey, Tvalue> cacheEventArgs);

        delegate void KeyRemovedEventHandler(object sender, CacheEventArgs<Tkey, Tvalue> cacheEventArgs);

        delegate void CacheMissEventHandler(object sender, CacheEventArgs<Tkey, Tvalue> cacheEventArgs);

        event KeyAddedEventHandler KeyAdded;
        event KeyExpiredEventHandler KeyExpired;
        event KeyRemovedEventHandler KeyRemoved;
        event CacheMissEventHandler CacheMiss;
    }
}
