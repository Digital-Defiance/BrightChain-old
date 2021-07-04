using BrightChain.Attributes;
using BrightChain.Exceptions;
using BrightChain.Interfaces;
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
        /// <summary>
        /// Emits a json binary blob from the metadata properties 
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static ReadOnlyMemory<byte> MetadataBytes(this IBlock block)
        {
            Dictionary<string, object> metadataDictionary = new Dictionary<string, object>();
            foreach (PropertyInfo prop in typeof(Block).GetProperties())
            {
                foreach (object attr in prop.GetCustomAttributes(true))
                {
                    if (attr is BrightChainMetadataAttribute)
                    {
                        metadataDictionary.Add(prop.Name, prop.GetValue(block));
                    }
                }
            }

            // get assembly version
            Assembly assembly = Assembly.GetEntryAssembly();
            AssemblyInformationalVersionAttribute versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            string assemblyVersion = versionAttribute.InformationalVersion;

            // add block type
            metadataDictionary.Add("_t", block.GetType().Name);
            // add assembly version
            metadataDictionary.Add("_v", assemblyVersion);

            string jsonData = JsonConvert.SerializeObject(metadataDictionary);
            var readonlyChars = jsonData.AsMemory();
            return new ReadOnlyMemory<byte>(readonlyChars.ToArray().Select(c => (byte)c).ToArray());
        }

        private static bool ReloadMetadata(this IBlock block, string key, object value, out Exception exception)
        {
            var prop = block.GetType().GetProperty(key);
            try
            {
                foreach (object attr in prop.GetCustomAttributes(true))
                {
                    if (attr is BrightChainMetadataAttribute)
                    {
                        prop.SetValue(block, value);
                        if (value is RedundancyContract redundancyContract)
                        {
                            block.StorageContract = redundancyContract.StorageContract;
                        }

                        exception = null;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }

            // not settable attribute
            exception = new BrightChainException("Invalid Metadata attribute");
            return false;
        }

        /// <summary>
        /// Takes a json blob with the serialized metadata and restores it to the block.
        /// Metadata by definition are not part of the block hash.
        /// The size must match, but this will technically alter the contracts on existing blocks until we add contract signatures.
        /// TODO: contract signatures
        /// </summary>
        /// <param name="block"></param>
        /// <param name="metadataBytes"></param>
        /// <returns></returns>
        private static bool RestoreMetadataFromBytes(this Block block, ReadOnlyMemory<byte> metadataBytes)
        {
            var jsonString = new string(metadataBytes.ToArray().Select(c => (char)c).ToArray());
            try
            {
                object metaDataObject = JsonConvert.DeserializeObject(jsonString, typeof(Dictionary<string, object>));

                Dictionary<string, object> metadataDictionary = (Dictionary<string, object>)metaDataObject;
                foreach (string key in metadataDictionary.Keys)
                {
                    if (key == "_t")
                    {
                        // TODO: validate compatible types and assembly versions
                    }
                    else if (key == "_v")
                    {
                        // TODO: validate compatible types and assembly versions

                    }
                    else if (!key.StartsWith("_"))
                    {
                        var keyProperty = block.GetType().GetProperty(key);
                        var keyValue = (metadataDictionary[key] as JObject).ToObject(keyProperty.PropertyType);
                        Exception reloadException = null;
                        bool wasSet = block.ReloadMetadata(key, keyValue, out reloadException);

                        if (reloadException != null)
                        {
                            throw reloadException;
                        }
                        else if (!wasSet)
                        {
                            return false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Attempt to restore the block's original metadata if the import fails.
        /// </summary>
        /// <param name="block"></param>
        /// <param name="metadataBytes"></param>
        /// <returns></returns>
        public static bool TryRestoreMetadataFromBytes(this Block block, ReadOnlyMemory<byte> metadataBytes)
        {
            // save metadata key -> values to be affected
            ReadOnlyMemory<byte> savedMetadata = block.Metadata;
            // ReloadMetadataFromBytes
            var restored = block.RestoreMetadataFromBytes(metadataBytes);
            // if true validate the block
            var validated = restored && block.Validate();
            // if either is false, restore the saved metadata
            if (!validated)
            {
                block.RestoreMetadataFromBytes(savedMetadata);
            }

            return validated;
        }
    }
}
