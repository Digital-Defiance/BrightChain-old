namespace BrightChain.Engine.Faster.Indices
{
    using BrightChain.Engine.Models.Hashes;

    public class CBLDataHashIndexValue : BrightChainIndexValue
    {
        public readonly DataHash DataHash;

        public CBLDataHashIndexValue(DataHash dataHash)
            : base(data: InternalSerialize(dataHash))
        {
            this.DataHash = dataHash;
        }

        public CBLDataHashIndexValue(ReadOnlyMemory<byte> data)
            : base(data)
        {
            this.DataHash = InternalDeserialize<DataHash>(data);
        }
    }
}
