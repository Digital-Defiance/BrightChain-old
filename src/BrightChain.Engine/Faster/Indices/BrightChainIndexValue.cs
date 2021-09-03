namespace BrightChain.Engine.Faster.Indices
{
    using ProtoBuf;

    public abstract class BrightChainIndexValue
    {
        public readonly ReadOnlyMemory<byte> Data;

        public BrightChainIndexValue(ReadOnlyMemory<byte> data)
        {
            this.Data = data;
        }

        protected static ReadOnlyMemory<byte> InternalSerialize<T>(T data)
        {
            MemoryStream s = new MemoryStream();
            Serializer.Serialize(s, data);
            s.Position = 0;
            return new ReadOnlyMemory<byte>(s.GetBuffer());
        }

        protected static T InternalDeserialize<T>(ReadOnlyMemory<byte> data)
        {
            MemoryStream s = new MemoryStream(data.ToArray());
            return Serializer.Deserialize<T>(s);
        }

        public BrightChainIndexValue AsIndex => this;
    }
}
