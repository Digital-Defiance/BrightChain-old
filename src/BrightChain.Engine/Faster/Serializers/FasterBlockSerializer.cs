namespace BrightChain.Engine.Faster.Serializers
{
    using BrightChain.Engine.Helpers;
    using BrightChain.Engine.Interfaces;
    using FASTER.core;

    /// <summary>
    /// Serializer for CacheValue - used if CacheValue is changed from struct to class.
    /// </summary>
    public class FasterBlockSerializer : BinaryObjectSerializer<IBlock>
    {
        private readonly BlockSerializer internalSerializer;

        public FasterBlockSerializer()
        {
            this.internalSerializer = new BlockSerializer();
        }

        public override void Deserialize(out IBlock obj)
        {
            obj = this.internalSerializer.ReadFrom(this.reader.BaseStream);
        }

        public override void Serialize(ref IBlock obj)
        {
            this.internalSerializer.WriteTo(obj, this.writer.BaseStream);
        }
    }
}
