using System;
using FASTER.core;
using NeuralFabric.Models.Hashes;

namespace BrightChain.Engine.Faster.Serializers;

/// <summary>
///     Serializer for CacheKey - used if CacheKey is changed from struct to class
/// </summary>
public class FasterDataHashSerializer
    : BinaryObjectSerializer<DataHash>
{
    public override void Deserialize(out DataHash obj)
    {
        var hashSize = this.reader.ReadInt32();
        var sourceLength = this.reader.ReadInt64();
        var dataBytes = this.reader.ReadBytes(count: hashSize);
        var computed = this.reader.ReadBoolean();

        obj = new DataHash(
            providedHashBytes: dataBytes,
            sourceDataLength: sourceLength,
            computed: computed);
    }

    public override void Serialize(ref DataHash obj)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(paramName: nameof(obj));
        }

        this.writer.Write(value: DataHash.HashSizeBytes);
        this.writer.Write(value: obj.SourceDataLength);
        this.writer.Write(buffer: obj.HashBytes.ToArray());
        this.writer.Write(value: obj.Computed);
    }
}
