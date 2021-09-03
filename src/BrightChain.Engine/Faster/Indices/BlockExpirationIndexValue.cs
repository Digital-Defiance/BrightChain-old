namespace BrightChain.Engine.Faster.Indices
{
    using BrightChain.Engine.Models.Hashes;

    public class BlockExpirationIndexValue : BrightChainIndexValue
    {
        public readonly IEnumerable<BlockHash> ExpiringHashes;

        public BlockExpirationIndexValue(IEnumerable<BlockHash> hashes)
            : base(data: InternalSerialize(hashes))
        {
            this.ExpiringHashes = hashes;
        }

        public BlockExpirationIndexValue(ReadOnlyMemory<byte> data)
            : base(data)
        {
            this.ExpiringHashes = InternalDeserialize<IEnumerable<BlockHash>>(data);
        }
    }
}
