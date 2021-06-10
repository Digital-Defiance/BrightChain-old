using System;
using System.Collections.Generic;

namespace BrightChain.Models.Events
{
    public class CacheEventArgs<Tkey, Tvalue> : EventArgs
    {
        public KeyValuePair<Tkey, Tvalue> KeyValue;
    }
}
