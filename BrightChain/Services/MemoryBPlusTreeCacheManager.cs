using CSharpTest.Net.Collections;
using CSharpTest.Net.Serialization;
using Microsoft.Extensions.Logging;
using System;

namespace BrightChain.Services
{
    /// <summary>
    /// Thin wrapper to parallel DiskCacheManager and provide appropriate BTree options
    /// </summary>
    public class MemoryBPlusTreeCacheManager<Tkey, Tvalue, TkeySerializer, TvalueSerializer> : BPlusTreeCacheManager<Tkey, Tvalue, TkeySerializer, TvalueSerializer>
        where Tkey : IComparable<Tkey>
        where Tvalue : IComparable<Tvalue>
        where TkeySerializer : ISerializer<Tkey>, new()
        where TvalueSerializer : ISerializer<Tvalue>, new()
    {
        public MemoryBPlusTreeCacheManager(ILogger logger, BPlusTree<Tkey, Tvalue>.OptionsV2 optionsV2 = null) :
            base(
                logger: logger,
                tree: optionsV2 is null ? new BPlusTree<Tkey, Tvalue>() : new BPlusTree<Tkey, Tvalue>(optionsV2: optionsV2))
        { }

        public MemoryBPlusTreeCacheManager(BPlusTreeCacheManager<Tkey, Tvalue, TkeySerializer, TvalueSerializer> other) : base(other) { }
    }
}
