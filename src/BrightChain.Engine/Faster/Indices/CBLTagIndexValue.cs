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
            this.CorrelationIds = InternalDeserialize<IEnumerable<Guid>>(data);
        }
    }
}
