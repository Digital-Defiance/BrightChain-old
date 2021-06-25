using BrightChain.Attributes;
using BrightChain.Models.Blocks;
using BrightChain.Models.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                        bool wasSet = false;
                        Exception reloadException = null;
                        switch (key)
                        {
                            case "DurationContract":
                                var durationContract = (metadataDictionary["DurationContract"] as JObject).ToObject<StorageDurationContract>();
                                wasSet = block.reloadMetadata(key, durationContract, out reloadException);
                                break;

                            case "RedundancyContract":
                                var redundancyContract = (metadataDictionary["RedundancyContract"] as JObject).ToObject<RedundancyContract>();
                                wasSet = block.reloadMetadata(key, redundancyContract, out reloadException);
                                break;

                            default:
                                wasSet = block.reloadMetadata(key, metadataDictionary[key], out reloadException);
                                break;
                        }
                        if (reloadException != null)
                            throw reloadException;
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
