namespace BrightChain.Engine.Helpers
{
    using System;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using FASTER.core;

    /// <summary>
    /// Serializer for CacheValue - used if CacheValue is changed from struct to class.
    /// </summary>
    public class BlockDataSerializer : BinaryObjectSerializer<BlockData>
    {
        public BlockDataSerializer()
        {
        }

        public override void Deserialize(out BlockData obj)
        {
            var bytesr = new byte[4];
            this.reader.Read(bytesr, 0, 4);
            var sizet = BitConverter.ToInt32(bytesr);
            var bytes = new byte[sizet];
            this.reader.Read(bytes, 0, sizet);
            obj = new BlockData(new ReadOnlyMemory<byte>(bytes));
        }

        public override void Serialize(ref BlockData obj)
        {
            var sizet = BitConverter.GetBytes(obj.Bytes.Length);
            this.writer.BaseStream.Write(sizet, 0, sizet.Length);
            this.writer.BaseStream.Write(obj.Bytes.ToArray(), 0, obj.Bytes.Length);
        }
    }
}
