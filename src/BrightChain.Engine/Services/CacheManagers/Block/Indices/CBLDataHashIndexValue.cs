namespace BrightChain.Engine.Faster.Indices
{
    using System;
    using System.IO;
    using BrightChain.Engine.Models.Hashes;
    using ProtoBuf;

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

        internal static ReadOnlyMemory<byte> InternalSerialize<T>(T data)
        {
            MemoryStream s = new MemoryStream();
            Serializer.Serialize(s, data);
            return new ReadOnlyMemory<byte>(s.ToArray());
        }

        internal static T InternalDeserialize<T>(ReadOnlyMemory<byte> data)
        {
            MemoryStream s = new MemoryStream(data.ToArray());
            return Serializer.Deserialize<T>(s);
        }
    }
}
