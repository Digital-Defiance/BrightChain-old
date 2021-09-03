using ProtoBuf;

namespace BrightChain.Engine.Faster.Indices
{
    public abstract class BrightChainIndexValue
    {
        public readonly ReadOnlyMemory<byte> Data;

        public BrightChainIndexValue(ReadOnlyMemory<byte> data)
        {
            this.Data = data;
        }

        protected static ReadOnlyMemory<byte> Serialize<T>(T data)
        {
            MemoryStream s = new MemoryStream();
            Serializer.Serialize(s, data);
            s.Position = 0;
            return new ReadOnlyMemory<byte>(s.GetBuffer());
        }

        protected static T Deserialize<T>(ReadOnlyMemory<byte> data)
        {
            MemoryStream s = new MemoryStream(data.ToArray());
            return Serializer.Deserialize<T>(s);
        }

        public BrightChainIndexValue AsIndex => this;
    }
}
