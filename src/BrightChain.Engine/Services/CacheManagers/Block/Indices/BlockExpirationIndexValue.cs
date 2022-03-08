using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightChain.Engine.Faster.Serializers;
using BrightChain.Engine.Models.Hashes;

namespace BrightChain.Engine.Faster.Indices;

public class BlockExpirationIndexValue : BrightChainIndexValue
{
    public readonly IEnumerable<BlockHash> ExpiringHashes;

    public BlockExpirationIndexValue(IEnumerable<BlockHash> hashes)
        : base(data: InternalSerialize(data: hashes))
    {
        this.ExpiringHashes = hashes;
    }

    public BlockExpirationIndexValue(ReadOnlyMemory<byte> data)
        : base(data: data)
    {
        this.ExpiringHashes = InternalDeserialize(data: data).ExpiringHashes;
    }

    private static ReadOnlyMemory<byte> InternalSerialize(IEnumerable<BlockHash> data)
    {
        var serializer = new FasterBlockHashSerializer();
        var memory = new MemoryStream();

        memory.Write(buffer: BitConverter.GetBytes(value: data.Count()));
        serializer.BeginSerialize(stream: memory);
        foreach (var item in data)
        {
            var refItem = item;
            serializer.Serialize(obj: ref refItem);
        }

        var retval = new ReadOnlyMemory<byte>(array: memory.ToArray());
        serializer.EndSerialize();
        return retval;
    }

    private static BlockExpirationIndexValue InternalDeserialize(ReadOnlyMemory<byte> data)
    {
        var deserializer = new FasterBlockHashSerializer();
        var s = new MemoryStream(buffer: data.ToArray());
        deserializer.BeginDeserialize(stream: s);

        var iBytes = new byte[sizeof(int)];
        s.Read(buffer: iBytes,
            offset: 0,
            count: sizeof(int));
        var count = BitConverter.ToInt32(value: new ReadOnlySpan<byte>(array: iBytes));
        var hashes = new BlockHash[count];
        for (var i = 0; i < count; i++)
        {
            deserializer.Deserialize(obj: out hashes[i]);
        }

        deserializer.EndDeserialize();
        return new BlockExpirationIndexValue(hashes: hashes);
    }
}
