using System;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Models.Blocks.DataObjects;
using FASTER.core;

namespace BrightChain.Engine.Helpers;

public class BlockDataSerializer : BinaryObjectSerializer<BlockData>
{
    private void DeserializePiBlock(out PiBlockData obj)
    {
        var offset = this.reader.ReadInt64();
        var size = this.reader.ReadInt32();
        obj = new PiBlockData(nOffset: offset,
            blockSize: size);
    }

    private void SerializePiBlock(ref PiBlockData obj)
    {
        this.writer.Write(value: BlockDataType.Pi.ToString());
        this.writer.Write(value: obj.PiOffset);
        this.writer.Write(value: obj.BlockSize);
    }

    private void DeserializeStoredBlock(out StoredBlockData obj)
    {
        var sizet = this.reader.ReadInt32();
        var bytes = new byte[sizet];
        this.reader.Read(buffer: bytes,
            index: 0,
            count: sizet);
        obj = new StoredBlockData(data: new ReadOnlyMemory<byte>(array: bytes));
    }

    private void SerializeStoredBlock(ref StoredBlockData obj)
    {
        this.writer.Write(value: BlockDataType.Stored.ToString());
        this.writer.Write(value: obj.Bytes.Length);
        this.writer.BaseStream.Write(buffer: obj.Bytes.ToArray(),
            offset: 0,
            count: obj.Bytes.Length);
    }

    public override void Deserialize(out BlockData obj)
    {
        var type = this.reader.ReadString();
        var storedBlockType = Enum.Parse(
            enumType: typeof(BlockDataType),
            value: type);

        switch (storedBlockType)
        {
            case BlockDataType.Stored:
                this.DeserializeStoredBlock(obj: out var storedObj);
                obj = storedObj;
                return;
            case BlockDataType.Pi:
                this.DeserializePiBlock(obj: out var piObj);
                obj = piObj;
                return;
            default:
                throw new NotImplementedException();
        }
    }

    public override void Serialize(ref BlockData obj)
    {
        if (obj is StoredBlockData storedObj)
        {
            this.SerializeStoredBlock(obj: ref storedObj);
            return;
        }

        if (obj is PiBlockData piObj)
        {
            this.SerializePiBlock(obj: ref piObj);
            return;
        }

        throw new NotImplementedException();
    }
}
