namespace BrightChain.Engine.Models.Blocks.Tags
{
    public struct BrightTag : IFormattable
    {
        public readonly string Tag;

        public BrightTag(string tag)
        {
            this.Tag = tag;
        }

        public ReadOnlyMemory<byte> Bytes =>
            System.Text.Encoding.ASCII.GetBytes(this.Tag);

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
