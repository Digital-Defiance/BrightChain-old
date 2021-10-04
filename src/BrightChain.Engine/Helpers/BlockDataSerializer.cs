namespace BrightChain.Engine.Helpers
{
    using System;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using FASTER.core;

    public class BlockDataSerializer : BinaryObjectSerializer<BlockData>
    {
        private void DeserializePiBlock(out PiBlockData obj)
        {
            var offset = this.reader.ReadInt64();
            var size = this.reader.ReadInt32();
            obj = new PiBlockData(nOffset: offset, blockSize: size);
        }

        private void SerializePiBlock(ref PiBlockData obj)
        {
            this.writer.Write(BlockDataType.Pi.ToString());
            this.writer.Write(obj.PiOffset);
            this.writer.Write(obj.BlockSize);
        }

        private void DeserializeStoredBlock(out StoredBlockData obj)
        {
            var sizet = this.reader.ReadInt32();
            var bytes = new byte[sizet];
            this.reader.Read(bytes, 0, sizet);
            obj = new StoredBlockData(new ReadOnlyMemory<byte>(bytes));
        }

        private void SerializeStoredBlock(ref StoredBlockData obj)
        {
            this.writer.Write(BlockDataType.Stored.ToString());
            this.writer.Write(obj.Bytes.Length);
            this.writer.BaseStream.Write(obj.Bytes.ToArray(), 0, obj.Bytes.Length);
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
                    this.DeserializeStoredBlock(out StoredBlockData storedObj);
                    obj = storedObj;
                    return;
                case BlockDataType.Pi:
                    this.DeserializePiBlock(out PiBlockData piObj);
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
                this.SerializeStoredBlock(ref storedObj);
                return;
            }
            else if (obj is PiBlockData piObj)
            {
                this.SerializePiBlock(ref piObj);
                return;
            }

            throw new NotImplementedException();
        }
    }
}
