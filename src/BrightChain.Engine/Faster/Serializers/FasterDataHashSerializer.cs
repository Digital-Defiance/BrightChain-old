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
            var hashSize = this.reader.ReadInt32();
            var sourceLength = this.reader.ReadInt64();
            var dataBytes = this.reader.ReadBytes(hashSize);
            var computed = this.reader.ReadBoolean();

            obj = new DataHash(
                providedHashBytes: dataBytes,
                sourceDataLength: sourceLength,
                computed: computed);
        }

        public override void Serialize(ref DataHash obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            this.writer.Write(BlockHash.HashSizeBytes);
            this.writer.Write(obj.SourceDataLength);
            this.writer.Write(obj.HashBytes.ToArray());
            this.writer.Write(obj.Computed);
        }
    }
}
