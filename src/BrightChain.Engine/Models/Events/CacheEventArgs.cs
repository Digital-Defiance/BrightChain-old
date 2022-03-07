using System;
using System.Collections.Generic;

namespace BrightChain.Engine.Models.Events
{
    /// <summary>
    /// Any cache event will have these args
    /// </summary>
    /// <typeparam name="Tkey"></typeparam>
    /// <typeparam name="Tvalue"></typeparam>
    public class CacheEventArgs<Tkey, Tvalue> : EventArgs
    {
        public KeyValuePair<Tkey, Tvalue> KeyValue;
    }
}
