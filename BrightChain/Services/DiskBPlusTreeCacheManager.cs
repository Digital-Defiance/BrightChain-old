using CSharpTest.Net.Collections;
using CSharpTest.Net.IO;
using CSharpTest.Net.Serialization;
using Microsoft.Extensions.Logging;
using System;

namespace BrightChain.Services
{
    /// <summary>
    /// Disk backed version of BPlus tree
    /// </summary>
    public class DiskBPlusTreeCacheManager<Tkey, Tvalue, TkeySerializer, TvalueSerializer> : BPlusTreeCacheManager<Tkey, Tvalue, TkeySerializer, TvalueSerializer>
        where Tkey : IComparable<Tkey>
        where Tvalue : IComparable<Tvalue>
        where TkeySerializer : ISerializer<Tkey>, new()
        where TvalueSerializer : ISerializer<Tvalue>, new()
    {
        private TempFile backingFile = new TempFile();

        public new BPlusTree<Tkey, Tvalue>.OptionsV2 DefaultOptions(BPlusTree<Tkey, Tvalue>.OptionsV2 options)
        {
            options.CalcBTreeOrder(16, 24);
            options.CreateFile = CreatePolicy.Always;
            options.FileName = this.backingFile.TempPath;
            return options;
        }

        public DiskBPlusTreeCacheManager(ILogger logger, BPlusTree<Tkey, Tvalue>.OptionsV2 optionsV2) :
            base(
                logger: logger,
                tree: new BPlusTree<Tkey, Tvalue>(optionsV2))
        { }

        public DiskBPlusTreeCacheManager(BPlusTreeCacheManager<Tkey, Tvalue, TkeySerializer, TvalueSerializer> other) : base(other) { }
    }
}
