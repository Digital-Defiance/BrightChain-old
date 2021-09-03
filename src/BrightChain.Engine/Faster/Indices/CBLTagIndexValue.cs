using ProtoBuf;

namespace BrightChain.Engine.Faster.Indices
{
    public class CBLTagIndexValue : BrightChainIndexValue
    {
        public readonly IEnumerable<Guid> CorrelationIds;

        public CBLTagIndexValue(IEnumerable<Guid> guids)
            : base(data: InternalSerialize(guids))
        {
            this.CorrelationIds = guids;
        }

        public CBLTagIndexValue(ReadOnlyMemory<byte> data)
            : base(data)
        {
            this.CorrelationIds = InternalDeserialize(data);
        }

        internal static ReadOnlyMemory<byte> InternalSerialize(IEnumerable<Guid> data)
        {
            MemoryStream s = new MemoryStream();
            Serializer.Serialize(s, data);
            return new ReadOnlyMemory<byte>(s.ToArray());
        }

        internal static IEnumerable<Guid> InternalDeserialize(ReadOnlyMemory<byte> data)
        {
            MemoryStream s = new MemoryStream(data.ToArray());
            return Serializer.Deserialize<IEnumerable<Guid>>(s);
        }
    }
}
