namespace BrightChain.Engine.Faster.Serializers
{
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Hashes;
    using FASTER.core;
    using ProtoBuf;

    /// <summary>
    /// Serializer for CacheKey - used if CacheKey is changed from struct to class
    /// </summary>
    public class FasterBlockHashSerializer<T>
        : BinaryObjectSerializer<BlockHash>
        where T : IBlock
    {

        public FasterBlockHashSerializer()
        {
        }

        public override void Deserialize(out BlockHash obj)
        {
            obj = Serializer.Deserialize<BlockHash>(source: this.reader.BaseStream);
        }

        public override void Serialize(ref BlockHash obj)
        {
            Serializer.Serialize(destination: this.writer.BaseStream, instance: obj);
        }
    }
}
