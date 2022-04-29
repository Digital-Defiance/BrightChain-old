using System;
using BrightChain.Engine.Interfaces;
using FASTER.core;

namespace BrightChain.Engine.Services.CacheManagers.Quorum;

public class QuorumCacheManager<Tkey, Tvalue, TkeySerializer, TvalueSerializer>
    : ICacheManager<Tkey, Tvalue>, IDisposable
    where Tkey : IComparable<Tkey>
    where TkeySerializer : BinaryObjectSerializer<Tkey>, new()
    where TvalueSerializer : BinaryObjectSerializer<Tvalue>, new()
{
    public Tvalue Get(Tkey blockHash)
    {
        throw new NotImplementedException();
    }

    public void Set(Tkey key, Tvalue value)
    {
        throw new NotImplementedException();
    }

    public bool Contains(Tkey key)
    {
        throw new NotImplementedException();
    }

    public bool Drop(Tkey key, bool noCheckContains = false)
    {
        throw new NotImplementedException();
    }

    public event ICacheManager<Tkey, Tvalue>.KeyAddedEventHandler KeyAdded;
    public event ICacheManager<Tkey, Tvalue>.KeyExpiredEventHandler KeyExpired;
    public event ICacheManager<Tkey, Tvalue>.KeyRemovedEventHandler KeyRemoved;
    public event ICacheManager<Tkey, Tvalue>.CacheMissEventHandler CacheMiss;
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
