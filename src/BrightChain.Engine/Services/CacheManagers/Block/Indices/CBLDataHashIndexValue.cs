using System;
using System.IO;
using NeuralFabric.Models.Hashes;
using ProtoBuf;

namespace BrightChain.Engine.Faster.Indices;

public class CBLDataHashIndexValue : BrightChainIndexValue
{
    public readonly DataHash DataHash;

    public CBLDataHashIndexValue(DataHash dataHash)
        : base(data: InternalSerialize(data: dataHash))
    {
        this.DataHash = dataHash;
    }

    public CBLDataHashIndexValue(ReadOnlyMemory<byte> data)
        : base(data: data)
    {
        this.DataHash = InternalDeserialize<DataHash>(data: data);
    }

    internal static ReadOnlyMemory<byte> InternalSerialize<T>(T data)
    {
        var s = new MemoryStream();
        Serializer.Serialize(destination: s,
            instance: data);
        return new ReadOnlyMemory<byte>(array: s.ToArray());
    }

    internal static T InternalDeserialize<T>(ReadOnlyMemory<byte> data)
    {
        var s = new MemoryStream(buffer: data.ToArray());
        return Serializer.Deserialize<T>(source: s);
    }
}
