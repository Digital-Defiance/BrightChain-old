namespace BrightChain.Engine.Faster.Serializers
{
    using BrightChain.Engine.Models.Blocks;
    using FASTER.core;
    using ProtoBuf;

    /// <summary>
    /// Serializer for CacheValue - used if CacheValue is changed from struct to class.
    /// </summary>
    public class FasterBlockSerializer : BinaryObjectSerializer<TransactableBlock>
    {
        public FasterBlockSerializer()
        {
        }

        public override void Deserialize(out TransactableBlock obj)
        {
            obj = Serializer.Deserialize<TransactableBlock>(source: this.reader.BaseStream);
        }

        public override void Serialize(ref TransactableBlock obj)
        {
            Serializer.Serialize(destination: this.writer.BaseStream, instance: obj);
        }
    }
}
