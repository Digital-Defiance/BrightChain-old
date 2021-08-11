namespace BrightChain.Engine.Interfaces
{
    public interface IDataSignature : IFormattable
    {
        /// <summary>`
        /// raw bytes of the hash value
        /// </summary>
        ReadOnlyMemory<byte> SignatureHashBytes { get; }
    }
}
