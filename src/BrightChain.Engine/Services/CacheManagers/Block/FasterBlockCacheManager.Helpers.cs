namespace BrightChain.Engine.Faster.CacheManager
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Faster.Enumerations;
    using BrightChain.Engine.Faster.Serializers;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Hashes;
    using FASTER.core;

    public partial class FasterBlockCacheManager
    {
        private static LogSettings NewLogSettings(Dictionary<CacheDeviceType, IDevice> storeDevices, bool useReadCache)
        {
            return new LogSettings
            {
                LogDevice = storeDevices[CacheDeviceType.Log],
                ObjectLogDevice = storeDevices[CacheDeviceType.Data],
                ReadCacheSettings = useReadCache ? new ReadCacheSettings() : null,
            };
        }

        private static CheckpointSettings NewCheckpointSettings(string cacheDir)
        {
            return new CheckpointSettings
            {
                CheckpointDir = cacheDir,
            };
        }

        private DirectoryInfo EnsuredDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                return Directory.CreateDirectory(dir);
            }

            return new DirectoryInfo(dir);
        }

        protected DirectoryInfo GetDiskCacheDirectory()
        {
            return Directory.CreateDirectory(
                Path.Combine(
                    path1: this.baseDirectory.FullName,
                    path2: "BrightChain",
                    path3: this.DatabaseName));
        }

        protected string GetDevicePath(string nameSpace, out DirectoryInfo cacheDirectoryInfo)
        {
            cacheDirectoryInfo = this.GetDiskCacheDirectory();

            return Path.Combine(
                cacheDirectoryInfo.FullName,
                string.Format(
                    provider: System.Globalization.CultureInfo.InvariantCulture,
                    format: "{0}-{1}.log",
                    this.DatabaseName,
                    nameSpace));
        }

        protected IDevice CreateLogDevice(string nameSpace)
        {
            var devicePath = this.GetDevicePath(nameSpace, out DirectoryInfo _);

            return Devices.CreateLogDevice(
                logPath: devicePath);
        }

        private
            (Dictionary<CacheDeviceType, IDevice> Devices,
            FasterBase Store)
            InitFaster()
        {
            var cacheDir = this.GetDiskCacheDirectory().FullName;
            var kv = new FasterBase();
            var devices = new Dictionary<CacheDeviceType, IDevice>();
            var logDevicesByType = new Dictionary<CacheDeviceType, IDevice>();
            foreach (CacheDeviceType deviceType in Enum.GetValues(enumType: typeof(CacheDeviceType)))
            {
                var device = this.CreateLogDevice(deviceType.ToString());
                logDevicesByType.Add(deviceType, device);
            }

            var blockDataSerializerSettings = new SerializerSettings<BlockHash, BlockData>
            {
                keySerializer = () => new FasterBlockHashSerializer(),
                valueSerializer = () => new DataContractObjectSerializer<BlockData>(),
            };

            var newStore = new FasterKV<BlockHash, BlockData>(
                size: HashTableBuckets,
                logSettings: NewLogSettings(devices, this.useReadCache),
                checkpointSettings: NewCheckpointSettings(cacheDir),
                serializerSettings: blockDataSerializerSettings,
                comparer: BlockSizeMap.ZeroVectorHash(BlockSize.Micro)); // gets an arbitrary BlockHash object which has the IFasterEqualityComparer on the class.

            return (devices, newStore);
        }
    }
}
