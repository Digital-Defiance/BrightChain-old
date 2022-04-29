using System;
using BrightChain.Engine.Interfaces;
using FASTER.core;

namespace BrightChain.Engine.Services.CacheManagers.Quorum;

/// <summary>
/// 
/// </summary>
/// <typeparam name="Tkey"></typeparam>
/// <typeparam name="Tvalue"></typeparam>
/// <typeparam name="TkeySerializer"></typeparam>
/// <typeparam name="TvalueSerializer"></typeparam>
public class QuorumCacheManager<Tkey, Tvalue, TkeySerializer, TvalueSerializer>
    : ICacheManager<Tkey, Tvalue>, IDisposable
    where Tkey : IComparable<Tkey>
    where TkeySerializer : BinaryObjectSerializer<Tkey>, new()
    where TvalueSerializer : BinaryObjectSerializer<Tvalue>, new()
{
    /// <summary>
    /// KeyAdded event
    /// </summary>
    public event ICacheManager<Tkey, Tvalue>.KeyAddedEventHandler KeyAdded;

    /// <summary>
    /// KeyExpired event
    /// </summary>
    public event ICacheManager<Tkey, Tvalue>.KeyExpiredEventHandler KeyExpired;

    /// <summary>
    /// KeyRemoved event
    /// </summary>
    public event ICacheManager<Tkey, Tvalue>.KeyRemovedEventHandler KeyRemoved;

    /// <summary>
    /// Cache miss event
    /// </summary>
    public event ICacheManager<Tkey, Tvalue>.CacheMissEventHandler CacheMiss;

    /// <summary>
    ///  Get data from the quorum
    /// </summary>
    /// <param name="blockHash"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Tvalue Get(Tkey blockHash)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Set quorum data
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Set(Tkey key, Tvalue value)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Whether the quorum contains the key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public bool Contains(Tkey key)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Drop the key if it exists
    /// </summary>
    /// <param name="key"></param>
    /// <param name="noCheckContains"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public bool Drop(Tkey key, bool noCheckContains = false)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Standard dispose pattern
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
