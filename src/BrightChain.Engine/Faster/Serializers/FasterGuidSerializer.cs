namespace BrightChain.Engine.Faster.Serializers
{
    using FASTER.core;
    /// <summary>
    /// Serializer for CacheKey - used if CacheKey is changed from struct to class
    /// </summary>
    public class FasterGuidSerializer
        : BinaryObjectSerializer<Guid>
    {
        public FasterGuidSerializer()
        {
        }

        public override void Deserialize(out Guid obj)
        {
            obj = Guid.Parse(this.reader.ReadString());
        }

        public override void Serialize(ref Guid obj)
        {

            this.writer.Write(obj.ToString());
        }
    }
}
