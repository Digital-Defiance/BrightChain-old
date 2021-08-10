namespace BrightChain.Engine.Faster.Serializers
{
    using BrightChain.Engine.Helpers;
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Blocks;
    using FASTER.core;

    /// <summary>
    /// Serializer for CacheKey - used if CacheKey is changed from struct to class
    /// </summary>
    public class FasterBlockHashSerializer<T>
        : BinaryObjectSerializer<BlockHash>
        where T : IBlock
    {
        private readonly BlockHashSerializer<T> internalSerializer;

        public FasterBlockHashSerializer()
        {
            this.internalSerializer = new BlockHashSerializer<T>();
        }

        public override void Deserialize(out BlockHash obj)
        {
            obj = this.internalSerializer.ReadFrom(this.reader.BaseStream);
        }

        public override void Serialize(ref BlockHash obj)
        {
            this.internalSerializer.WriteTo(obj, this.writer.BaseStream);
        }
    }
}
