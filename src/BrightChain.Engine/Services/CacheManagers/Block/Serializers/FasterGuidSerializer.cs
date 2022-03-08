using System;
using FASTER.core;

namespace BrightChain.Engine.Faster.Serializers;

/// <summary>
///     Serializer for CacheKey - used if CacheKey is changed from struct to class
/// </summary>
public class FasterGuidSerializer
    : BinaryObjectSerializer<Guid>
{
    public override void Deserialize(out Guid obj)
    {
        obj = Guid.Parse(input: this.reader.ReadString());
    }

    public override void Serialize(ref Guid obj)
    {
        this.writer.Write(value: obj.ToString());
    }
}
