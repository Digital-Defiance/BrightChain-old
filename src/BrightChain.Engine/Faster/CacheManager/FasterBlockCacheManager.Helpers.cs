namespace BrightChain.Engine.Faster.CacheManager
{
    using System;
    using System.Globalization;
    using System.IO;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Faster.Enumerations;
    using BrightChain.Engine.Faster.Indices;
    using BrightChain.Engine.Faster.Serializers;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Hashes;
    using FASTER.core;

    public partial class FasterBlockCacheManager
    {
        private static LogSettings NewLogSettings(Dictionary<CacheDeviceType, IDevice> storeDevices, bool useReadCache) =>
            new LogSettings
            {
                LogDevice = storeDevices[CacheDeviceType.Log],
                ObjectLogDevice = storeDevices[CacheDeviceType.Data],
                ReadCacheSettings = useReadCache ? new ReadCacheSettings() : null,
            };

        private static CheckpointSettings NewCheckpointSettings(string cacheDir) =>
            new CheckpointSettings
            {
                CheckpointDir = cacheDir,
            };

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
            (Dictionary<CacheStoreType, Dictionary<CacheDeviceType, IDevice>> DevicesByStoreType,
            Dictionary<CacheStoreType, FasterBase> StoresByStoreType)
            InitFaster()
        {
            var cacheDir = this.GetDiskCacheDirectory().FullName;
            var kvs = new Dictionary<CacheStoreType, FasterBase>();
            var devices = new Dictionary<CacheStoreType, Dictionary<CacheDeviceType, IDevice>>();
            foreach (CacheStoreType storeType in Enum.GetValues(enumType: typeof(CacheStoreType)))
            {
                var logDevicesByType = new Dictionary<CacheDeviceType, IDevice>();
                foreach (CacheDeviceType deviceType in Enum.GetValues(enumType: typeof(CacheDeviceType)))
                {
                    var device = this.CreateLogDevice(string.Format("{0}-{1}", storeType.ToString(), deviceType.ToString()));
                    logDevicesByType.Add(deviceType, device);
                }

                devices.Add(storeType, logDevicesByType);
                var newKv = this.newKVFuncs[storeType](logDevicesByType, this.useReadCache, cacheDir);
                kvs.Add(storeType, newKv);
            }

            return (devices, kvs);
        }
    }
}
