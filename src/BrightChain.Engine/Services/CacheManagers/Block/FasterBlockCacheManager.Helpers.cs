using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Faster.Enumerations;
using BrightChain.Engine.Faster.Serializers;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Models.Hashes;
using FASTER.core;

namespace BrightChain.Engine.Faster.CacheManager;

public partial class FasterBlockCacheManager
{
    private static LogSettings NewLogSettings(Dictionary<CacheDeviceType, IDevice> storeDevices, bool useReadCache)
    {
        return new LogSettings
        {
            LogDevice = storeDevices[key: CacheDeviceType.Log],
            ObjectLogDevice = storeDevices[key: CacheDeviceType.Data],
            ReadCacheSettings = useReadCache ? new ReadCacheSettings() : null,
        };
    }

    private static CheckpointSettings NewCheckpointSettings(string cacheDir)
    {
        return new CheckpointSettings {CheckpointDir = cacheDir};
    }

    private DirectoryInfo EnsuredDirectory(string dir)
    {
        if (!Directory.Exists(path: dir))
        {
            return Directory.CreateDirectory(path: dir);
        }

        return new DirectoryInfo(path: dir);
    }

    protected DirectoryInfo GetDiskCacheDirectory()
    {
        return Directory.CreateDirectory(
            path: Path.Combine(
                path1: this.baseDirectory.FullName,
                path2: "BrightChain",
                path3: this.DatabaseName));
    }

    protected string GetDevicePath(string nameSpace, out DirectoryInfo cacheDirectoryInfo)
    {
        cacheDirectoryInfo = this.GetDiskCacheDirectory();

        return Path.Combine(
            path1: cacheDirectoryInfo.FullName,
            path2: string.Format(
                provider: CultureInfo.InvariantCulture,
                format: "{0}-{1}.log",
                arg0: this.DatabaseName,
                arg1: nameSpace));
    }

    protected IDevice CreateLogDevice(string nameSpace)
    {
        var devicePath = this.GetDevicePath(nameSpace: nameSpace,
            cacheDirectoryInfo: out var _);

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
            var device = this.CreateLogDevice(nameSpace: deviceType.ToString());
            logDevicesByType.Add(key: deviceType,
                value: device);
        }

        var blockDataSerializerSettings = new SerializerSettings<BlockHash, BlockData>
        {
            keySerializer = () => new FasterBlockHashSerializer(),
            valueSerializer = () => new DataContractObjectSerializer<BlockData>(),
        };

        var newStore = new FasterKV<BlockHash, BlockData>(
            size: HashTableBuckets,
            logSettings: NewLogSettings(storeDevices: devices,
                useReadCache: this.useReadCache),
            checkpointSettings: NewCheckpointSettings(cacheDir: cacheDir),
            serializerSettings: blockDataSerializerSettings,
            comparer: BlockSizeMap.ZeroVectorHash(blockSize: BlockSize
                .Micro)); // gets an arbitrary BlockHash object which has the IFasterEqualityComparer on the class.

        return (devices, newStore);
    }
}
