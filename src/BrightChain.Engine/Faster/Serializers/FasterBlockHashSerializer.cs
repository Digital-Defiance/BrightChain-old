﻿namespace BrightChain.Engine.Faster.Serializers
{
    using BrightChain.Engine.Models.Hashes;
    using FASTER.core;
    using ProtoBuf;

    /// <summary>
    /// Serializer for CacheKey - used if CacheKey is changed from struct to class
    /// </summary>
    public class FasterBlockHashSerializer
        : BinaryObjectSerializer<BlockHash>
    {

        public FasterBlockHashSerializer()
        {
        }

        public override void Deserialize(out BlockHash obj)
        {
            obj = Serializer.Deserialize<BlockHash>(source: this.reader.BaseStream);
        }

        public override void Serialize(ref BlockHash obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            Serializer.Serialize(destination: this.writer.BaseStream, instance: obj);
        }
    }
}
