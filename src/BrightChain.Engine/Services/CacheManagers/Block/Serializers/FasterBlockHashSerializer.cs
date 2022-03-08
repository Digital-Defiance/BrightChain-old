using System;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Models.Hashes;
using FASTER.core;
using NeuralFabric.Models.Hashes;

namespace BrightChain.Engine.Faster.Serializers;

/// <summary>
///     Serializer for CacheKey - used if CacheKey is changed from struct to class
/// </summary>
public class FasterBlockHashSerializer
    : BinaryObjectSerializer<BlockHash>
{
    public override void Deserialize(out BlockHash obj)
    {
        var hashSize = this.reader.ReadInt32();
        var blockSizeString = this.reader.ReadString();
        var blockSize = (BlockSize)Enum.Parse(enumType: typeof(BlockSize),
            value: blockSizeString);
        var blockBytes = this.reader.ReadBytes(count: hashSize);
        var blockType = Type.GetType(typeName: this.reader.ReadString());
        var computed = this.reader.ReadBoolean();

        obj = new BlockHash(
            blockType: blockType,
            originalBlockSize: blockSize,
            providedHashBytes: blockBytes,
            computed: computed);
    }

    public override void Serialize(ref BlockHash obj)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(paramName: nameof(obj));
        }

        this.writer.Write(value: DataHash.HashSizeBytes);
        this.writer.Write(value: obj.BlockSize.ToString());
        this.writer.Write(buffer: obj.HashBytes.ToArray());
        this.writer.Write(value: obj.BlockType.AssemblyQualifiedName);
        this.writer.Write(value: obj.Computed);
    }
}
