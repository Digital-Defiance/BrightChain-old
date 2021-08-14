namespace BrightChain.Engine.Faster.Serializers
{
    using BrightChain.Engine.Models.Hashes;
    using FASTER.core;
    using ProtoBuf;

    /// <summary>
    /// Serializer for CacheKey - used if CacheKey is changed from struct to class
    /// </summary>
    public class FasterDataHashSerializer
        : BinaryObjectSerializer<DataHash>
    {

        public FasterDataHashSerializer()
        {
        }

        public override void Deserialize(out DataHash obj)
        {
            obj = Serializer.Deserialize<DataHash>(source: this.reader.BaseStream);
        }

        public override void Serialize(ref DataHash obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            Serializer.Serialize(destination: this.writer.BaseStream, instance: obj);
        }
    }
}
