using System;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Faster.Indices;
using FASTER.core;

namespace BrightChain.Engine.Faster.Serializers;

/// <summary>
///     Serializer for CacheKey - used if CacheKey is changed from struct to class
/// </summary>
public class FasterBrightChainIndexValueSerializer
    : BinaryObjectSerializer<BrightChainIndexValue>
{
    public override void Deserialize(out BrightChainIndexValue obj)
    {
        var length = this.reader.ReadInt32();
        var type = Type.GetType(typeName: this.reader.ReadString());
        var data = this.reader.ReadBytes(count: length);

        if (type.Equals(o: typeof(BlockExpirationIndexValue)))
        {
            obj = new BlockExpirationIndexValue(data: new ReadOnlyMemory<byte>(array: data));
        }
        else if (type.Equals(o: typeof(CBLDataHashIndexValue)))
        {
            obj = new CBLDataHashIndexValue(data: new ReadOnlyMemory<byte>(array: data));
        }
        else if (type.Equals(o: typeof(CBLTagIndexValue)))
        {
            obj = new CBLTagIndexValue(data: new ReadOnlyMemory<byte>(array: data));
        }
        else
        {
            throw new BrightChainException(message: "Unexpected type");
        }
    }

    public override void Serialize(ref BrightChainIndexValue obj)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(paramName: nameof(obj));
        }

        this.writer.Write(value: obj.Data.Length);
        this.writer.Write(value: obj.GetType().AssemblyQualifiedName);
        this.writer.Write(buffer: obj.Data.ToArray());
    }
}
