namespace BrightChain.Engine.Faster.Serializers
{
    using System;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Faster.Indices;
    using FASTER.core;

    /// <summary>
    /// Serializer for CacheKey - used if CacheKey is changed from struct to class
    /// </summary>
    public class FasterBrightChainIndexValueSerializer
        : BinaryObjectSerializer<BrightChainIndexValue>
    {

        public FasterBrightChainIndexValueSerializer()
        {
        }

        public override void Deserialize(out BrightChainIndexValue obj)
        {
            int length = this.reader.ReadInt32();
            Type type = Type.GetType(typeName: this.reader.ReadString());
            var data = this.reader.ReadBytes(length);

            if (type.Equals(typeof(BlockExpirationIndexValue)))
            {
                obj = new BlockExpirationIndexValue(new ReadOnlyMemory<byte>(data));
            }
            else if (type.Equals(typeof(CBLDataHashIndexValue)))
            {
                obj = new CBLDataHashIndexValue(new ReadOnlyMemory<byte>(data));
            }
            else if (type.Equals(typeof(CBLTagIndexValue)))
            {
                obj = new CBLTagIndexValue(new ReadOnlyMemory<byte>(data));
            }
            else
            {
                throw new BrightChainException("Unexpected type");
            }
        }

        public override void Serialize(ref BrightChainIndexValue obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            this.writer.Write(obj.Data.Length);
            this.writer.Write(obj.GetType().AssemblyQualifiedName);
            this.writer.Write(obj.Data.ToArray());
        }
    }
}
