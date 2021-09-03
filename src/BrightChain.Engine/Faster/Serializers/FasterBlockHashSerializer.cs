namespace BrightChain.Engine.Faster.Serializers
{
    using BrightChain.Engine.Enumerations;
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
            var hashSize = this.reader.ReadInt32();
            var blockSizeString = this.reader.ReadString();
            var blockSize = (BlockSize)Enum.Parse(typeof(BlockSize), blockSizeString);
            var blockBytes = this.reader.ReadBytes(hashSize);
            var blockType = Type.GetType(this.reader.ReadString());
            var computed = this.reader.ReadBoolean();

            obj = new BlockHash(
                blockType: blockType,
                originalBlockSize: blockSize,
                providedHashBytes: blockBytes,
                computed: computed);
        }

        public override void Serialize(ref BlockHash obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            this.writer.Write(BlockHash.HashSizeBytes);
            this.writer.Write(obj.BlockSize.ToString());
            this.writer.Write(obj.HashBytes.ToArray());
            this.writer.Write(obj.BlockType.AssemblyQualifiedName);
            this.writer.Write(obj.Computed);
        }
    }
}
