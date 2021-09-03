namespace BrightChain.Engine.Faster.Indices
{
    using BrightChain.Engine.Faster.Serializers;
    using BrightChain.Engine.Models.Hashes;
    using ProtoBuf;

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
            this.ExpiringHashes = InternalDeserialize(data).ExpiringHashes;
        }

        internal static ReadOnlyMemory<byte> InternalSerialize(IEnumerable<BlockHash> data)
        {
            var memory = new MemoryStream();
            memory.Write(BitConverter.GetBytes(data.Count()));
            var tmp = new FasterBlockHashSerializer();
            tmp.BeginSerialize(memory);
            foreach (var item in data)
            {
                var refItem = item;
                tmp.Serialize(ref refItem);
            }

            memory.Position = 0;
            var retval = new ReadOnlyMemory<byte>(memory.GetBuffer());
            tmp.EndSerialize();
            return retval;
        }

        internal static BlockExpirationIndexValue InternalDeserialize(ReadOnlyMemory<byte> data)
        {
            var tmp = new FasterBlockHashSerializer();
            MemoryStream s = new MemoryStream(data.ToArray());
            tmp.BeginDeserialize(s);

            byte[] iBytes = new byte[sizeof(int)];
            s.Read(iBytes, 0, sizeof(int));
            var count = BitConverter.ToInt32(new ReadOnlySpan<byte>(array: iBytes));
            var hashes = new BlockHash[count];
            for (int i = 0; i < count; i++)
            {
                tmp.Deserialize(out hashes[i]);
            }
            tmp.EndDeserialize();
            return new BlockExpirationIndexValue(hashes: hashes);
        }
    }
}
