using BrightChain.Attributes;
using BrightChain.Models.Blocks;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BrightChain.Extensions
{
    public static class BlockMetadataExtensions
    {
        public static ReadOnlyMemory<byte> MetaDataBytes(this Block block)
        {
            Dictionary<string, object> _dict = new Dictionary<string, object>();

            PropertyInfo[] props = typeof(Block).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    BrightChainMetadataAttribute metadataAttr = attr as BrightChainMetadataAttribute;
                    if (metadataAttr != null)
                    {
                        string propName = prop.Name;
                        object propValue = prop.GetValue(block);

                        _dict.Add(propName, propValue);
                    }
                }
            }
            // alphabetize by key

            return new ReadOnlyMemory<byte>();
        }
    }
}
