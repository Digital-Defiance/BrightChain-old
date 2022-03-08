using System;
using System.IO;
using BrightChain.Engine.Models.Blocks;
using FASTER.core;

namespace BrightChain.Engine.Faster.Indices;

public class BlockMetadataIndexValue : BrightChainIndexValue
{
    public readonly BrightenedBlock Block;

    public BlockMetadataIndexValue(BrightenedBlock block)
        : base(data: InternalSerialize(data: block))
    {
        this.Block = block;
    }

    public BlockMetadataIndexValue(ReadOnlyMemory<byte> data)
        : base(data: data)
    {
        this.Block = InternalDeserialize(data: data).Block;
    }

    private static ReadOnlyMemory<byte> InternalSerialize(BrightenedBlock data)
    {
        var serializer = new DataContractObjectSerializer<BrightenedBlock>();
        var memory = new MemoryStream();
        serializer.BeginSerialize(stream: memory);
        serializer.Serialize(obj: ref data);
        var bytes = memory.ToArray();
        serializer.EndSerialize();

        var retval = new ReadOnlyMemory<byte>(array: bytes);
        return retval;
    }

    private static BlockMetadataIndexValue InternalDeserialize(ReadOnlyMemory<byte> data)
    {
        var deserializer = new DataContractObjectSerializer<BrightenedBlock>();
        var s = new MemoryStream(buffer: data.ToArray());
        deserializer.BeginDeserialize(stream: s);
        deserializer.Deserialize(obj: out var block);
        deserializer.EndDeserialize();
        return new BlockMetadataIndexValue(block: block);
    }
}
