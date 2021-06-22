using BrightChain.Models.Events;

namespace BrightChain.Interfaces
{
    /// <summary>
    /// Basic guaranteed members of the cache system. Notably the system is heavily dependent on the BPlusTree caches which have transaction support.
    /// </summary>
    /// <typeparam name="Tkey"></typeparam>
    /// <typeparam name="Tvalue"></typeparam>
    public interface ICacheManager<Tkey, Tvalue>
    {
        Tvalue Get(Tkey key);
        void Set(Tkey key, Tvalue value);
        bool Contains(Tkey key);
        bool Drop(Tkey key, bool noCheckContains = false);

        delegate void KeyAddedEventHandler(object sender, CacheEventArgs<Tkey, Tvalue> cacheEventArgs);
        delegate void KeyRemovedEventHandler(object sender, CacheEventArgs<Tkey, Tvalue> cacheEventArgs);
        delegate void CacheMissEventHandler(object sender, CacheEventArgs<Tkey, Tvalue> cacheEventArgs);

        event KeyAddedEventHandler KeyAdded;
        event KeyRemovedEventHandler KeyRemoved;
        event CacheMissEventHandler CacheMiss;
    }
}