namespace BrightChain.Engine.Faster.Indices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using BrightChain.Engine.Faster.Serializers;
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
            this.ExpiringHashes = InternalDeserialize(data).ExpiringHashes;
        }

        private static ReadOnlyMemory<byte> InternalSerialize(IEnumerable<BlockHash> data)
        {
            var serializer = new FasterBlockHashSerializer();
            var memory = new MemoryStream();

            memory.Write(BitConverter.GetBytes(data.Count()));
            serializer.BeginSerialize(memory);
            foreach (var item in data)
            {
                var refItem = item;
                serializer.Serialize(ref refItem);
            }

            var retval = new ReadOnlyMemory<byte>(memory.ToArray());
            serializer.EndSerialize();
            return retval;
        }

        private static BlockExpirationIndexValue InternalDeserialize(ReadOnlyMemory<byte> data)
        {
            var deserializer = new FasterBlockHashSerializer();
            MemoryStream s = new MemoryStream(data.ToArray());
            deserializer.BeginDeserialize(s);

            byte[] iBytes = new byte[sizeof(int)];
            s.Read(iBytes, 0, sizeof(int));
            var count = BitConverter.ToInt32(new ReadOnlySpan<byte>(array: iBytes));
            var hashes = new BlockHash[count];
            for (int i = 0; i < count; i++)
            {
                deserializer.Deserialize(out hashes[i]);
            }

            deserializer.EndDeserialize();
            return new BlockExpirationIndexValue(hashes: hashes);
        }
    }
}
