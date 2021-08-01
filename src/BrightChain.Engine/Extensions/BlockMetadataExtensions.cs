using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using BrightChain.Engine.Attributes;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Factories;
using BrightChain.Engine.Interfaces;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Contracts;

namespace BrightChain.Engine.Extensions
{
    /// <summary>
    /// Extension class that gives blocks ability to export and import their metadata from any attribute tagged with [BrightChainMetadata].
    /// TODO: Right now we are generating really verbose JSON.
    /// Suggest Bson, or otherwise using short keys or stripping keys after ordering by key.
    /// Would then have to check the _t attribute on the block for the type of the original block to get the right attributes back for restore.
    /// Otherwise have been thinking about just gzipping the JSON and using the short keys.
    /// Also thinking about length encoding the metadata byte portion. {int-length}{metadata}{data} rather than {metadata}{0}{data}
    /// </summary>
    public static class BlockMetadataExtensions
    {
        public static JsonSerializerOptions NewSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = false,
                Converters =
                {
                    new HashJsonFactory(),
                },
            };
        }

        /// <summary>
        /// Emits a json binary blob from the metadata properties 
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static ReadOnlyMemory<byte> MetadataBytes(this IBlock block)
        {
            Dictionary<string, object> metadataDictionary = new Dictionary<string, object>();
            foreach (PropertyInfo prop in block.GetType().GetProperties())
            {
                foreach (object attr in prop.GetCustomAttributes(true))
                {
                    if (attr is BrightChainMetadataAttribute)
                    {
                        metadataDictionary.Add(prop.Name, prop.GetValue(block));
                    }
                }
            }

            // add block type
            metadataDictionary.Add("_t", block.OriginalType);

            // add assembly version
            metadataDictionary.Add("_v", block.AssemblyVersion);

            string jsonData = JsonSerializer.Serialize(metadataDictionary, NewSerializerOptions());
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
                        if (value is StorageContract storageContract)
                        {
                            block.StorageContract = storageContract;
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
                object metaDataObject = JsonSerializer.Deserialize(jsonString, typeof(Dictionary<string, object>), NewSerializerOptions());

                Dictionary<string, object> metadataDictionary = (Dictionary<string, object>)metaDataObject;
                foreach (string key in metadataDictionary.Keys)
                {
                    if (key == "_t")
                    {
                        block.OriginalType = ((JsonElement)metadataDictionary[key]).ToObject<string>();
                        // TODO: validate compatible types and assembly versions
                    }
                    else if (key == "_v")
                    {
                        block.AssemblyVersion = ((JsonElement)metadataDictionary[key]).ToObject<string>();
                        // TODO: validate compatible types and assembly versions
                    }
                    else if (!key.StartsWith("_", false, culture: System.Globalization.CultureInfo.InvariantCulture))
                    {
                        var keyProperty = block.GetType().GetProperty(key);
                        var valueObject = metadataDictionary[key];
                        var keyValue = (valueObject is null) ? null : ((JsonElement)valueObject).ToObject(keyProperty.PropertyType, NewSerializerOptions());
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
            catch (Exception _)
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
        public static bool TryRestoreMetadataFromBytesAndValidate(this Block block, ReadOnlyMemory<byte> metadataBytes)
        {
            // save metadata key -> values to be affected
            var savedMetadata = block.Metadata;
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
