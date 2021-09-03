namespace BrightChain.Engine.Faster.Indices
{
    using BrightChain.Engine.Models.Blocks;
    using FASTER.core;

    public class BlockMetadataIndexValue : BrightChainIndexValue
    {
        public readonly BrightenedBlock Block;

        public BlockMetadataIndexValue(BrightenedBlock block)
            : base(data: InternalSerialize(block))
        {
            this.Block = block;
        }

        public BlockMetadataIndexValue(ReadOnlyMemory<byte> data)
            : base(data)
        {
            this.Block = InternalDeserialize(data).Block;
        }

        private static ReadOnlyMemory<byte> InternalSerialize(BrightenedBlock data)
        {
            var serializer = new DataContractObjectSerializer<BrightenedBlock>();
            var memory = new MemoryStream();
            serializer.BeginSerialize(memory);
            serializer.Serialize(ref data);
            var bytes = memory.ToArray();
            serializer.EndSerialize();

            var retval = new ReadOnlyMemory<byte>(bytes);
            return retval;
        }

        private static BlockMetadataIndexValue InternalDeserialize(ReadOnlyMemory<byte> data)
        {
            var deserializer = new DataContractObjectSerializer<BrightenedBlock>();
            MemoryStream s = new MemoryStream(data.ToArray());
            deserializer.BeginDeserialize(s);
            deserializer.Deserialize(out BrightenedBlock block);
            deserializer.EndDeserialize();
            return new BlockMetadataIndexValue(block: block);
        }
    }
}
