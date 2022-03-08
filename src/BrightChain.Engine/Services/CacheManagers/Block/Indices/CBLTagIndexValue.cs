using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;

namespace BrightChain.Engine.Faster.Indices;

public class CBLTagIndexValue : BrightChainIndexValue
{
    public readonly IEnumerable<Guid> CorrelationIds;

    public CBLTagIndexValue(IEnumerable<Guid> guids)
        : base(data: InternalSerialize(data: guids))
    {
        this.CorrelationIds = guids;
    }

    public CBLTagIndexValue(ReadOnlyMemory<byte> data)
        : base(data: data)
    {
        this.CorrelationIds = InternalDeserialize(data: data);
    }

    internal static ReadOnlyMemory<byte> InternalSerialize(IEnumerable<Guid> data)
    {
        var s = new MemoryStream();
        Serializer.Serialize(destination: s,
            instance: data);
        return new ReadOnlyMemory<byte>(array: s.ToArray());
    }

    internal static IEnumerable<Guid> InternalDeserialize(ReadOnlyMemory<byte> data)
    {
        var s = new MemoryStream(buffer: data.ToArray());
        return Serializer.Deserialize<IEnumerable<Guid>>(source: s);
    }
}
