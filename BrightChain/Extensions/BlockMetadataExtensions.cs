using BrightChain.Attributes;
using BrightChain.Models.Blocks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BrightChain.Extensions
{
    public static class BlockMetadataExtensions
    {
        public static ReadOnlyMemory<byte> MetaDataBytes(this Block block)
        {
            Dictionary<string, object> metadataDictionary = new Dictionary<string, object>();
            foreach (PropertyInfo prop in typeof(Block).GetProperties())
                foreach (object attr in prop.GetCustomAttributes(true))
                    if (attr is BrightChainMetadataAttribute)
                        metadataDictionary.Add(prop.Name, prop.GetValue(block));

            // get assembly version
            Assembly assembly = Assembly.GetEntryAssembly();
            AssemblyInformationalVersionAttribute versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            string assemblyVersion = versionAttribute.InformationalVersion;

            metadataDictionary.Add("_t", block.GetType().Name);
            metadataDictionary.Add("_v", assemblyVersion);

            string jsonData = JsonConvert.SerializeObject(metadataDictionary);
            var readonlyChars = jsonData.AsMemory();
            return new ReadOnlyMemory<byte>(readonlyChars.ToArray().Select(c => (byte)c).ToArray());
        }

        public static bool RestoreMetaDataFromBytes(this Block block, ReadOnlyMemory<byte> metaDataBytes)
        {
            try
            {
                object metaDataObject = JsonConvert.DeserializeObject(
                    System.Text.Encoding.ASCII.GetString(
                        metaDataBytes.ToArray()), typeof(Dictionary<string, object>));

                // TODO: validate compatible types and assembly versions
                // TODO: use BlockFactory to get the right type

                Dictionary<string, object> metadataDictionary = (Dictionary<string, object>)metaDataObject;
                foreach (string key in metadataDictionary.Keys)
                    if (!key.StartsWith("_"))
                    {
                        var prop = typeof(Block).GetProperty(key);
                        if (prop != null)
                            prop.SetValue(block, metadataDictionary[key]);
                    }
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }
    }
}
