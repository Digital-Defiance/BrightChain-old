using BrightChain.Engine.Enumerations;

namespace BrightChain.Engine.Models.Blocks.Tags
{
    public struct BrightTag : IFormattable
    {
        public readonly Guid Id;
        public readonly string Tag;
        public readonly BrightTagType Type;

        public BrightTag(string tag, BrightTagType type = BrightTagType.UserAssigned)
        {
            this.Id = Guid.NewGuid();
            this.Tag = tag;
            this.Type = type;
        }

        private string uniqueIdentifier =>
            string.Format(
                format: "{0}:{1}",
                this.Type.ToString(),
                this.Tag);

        public ReadOnlyMemory<byte> Bytes =>
            System.Text.Encoding.ASCII.GetBytes(this.Tag);

        public ReadOnlyMemory<byte> IdentifierBytes =>
            System.Text.Encoding.ASCII.GetBytes(this.uniqueIdentifier);

        public uint Crc32 =>
            Helpers.Crc32.ComputeNewChecksum(this.Bytes.ToArray());

        public ulong Crc64 =>
            DamienG.Security.Cryptography.Crc64Iso.Compute(this.Bytes.ToArray());

        public string ToString(string _, IFormatProvider formatProvider)
        {
            return this.Tag.ToString(provider: formatProvider);
        }

        public string ToString()
        {
            return this.Tag.ToString();
        }
    }
}
