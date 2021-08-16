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
            var assumedByteLength = (int)this.reader.BaseStream.Length;
            byte[] buffer = new byte[assumedByteLength];
            this.reader.BaseStream.Read(buffer: buffer, offset: 0, count: assumedByteLength);
            obj = new BlockData(new ReadOnlyMemory<byte>(buffer));
        }

        public override void Serialize(ref BlockData obj)
        {
            this.writer.BaseStream.Write(obj.Bytes.ToArray(), 0, obj.Bytes.Length);
        }
    }
}
