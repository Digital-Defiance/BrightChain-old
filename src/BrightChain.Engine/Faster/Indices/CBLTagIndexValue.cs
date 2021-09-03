namespace BrightChain.Engine.Faster.Indices
{
    public class CBLTagIndexValue : BrightChainIndexValue
    {
        public readonly List<Guid> CorrelationIds;

        public CBLTagIndexValue(List<Guid> guids)
            : base(data: InternalSerialize(guids))
        {
            this.CorrelationIds = guids;
        }

        public CBLTagIndexValue(ReadOnlyMemory<byte> data)
            : base(data)
        {
            this.CorrelationIds = InternalDeserialize<List<Guid>>(data);
        }
    }
}
