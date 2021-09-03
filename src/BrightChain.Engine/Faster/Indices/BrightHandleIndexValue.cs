namespace BrightChain.Engine.Faster.Indices
{
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using ProtoBuf;

    public class BrightHandleIndexValue : BrightChainIndexValue
    {
        public readonly BrightHandle BrightHandle;

        public BrightHandleIndexValue(BrightHandle brightHandle)
            : base(data: InternalSerialize(brightHandle))
        {
            this.BrightHandle = brightHandle;
        }

        public BrightHandleIndexValue(ReadOnlyMemory<byte> data)
            : base(data)
        {
            this.BrightHandle = InternalDeserialize(data);
        }

        internal static ReadOnlyMemory<byte> InternalSerialize(BrightHandle data)
        {
            return new ReadOnlyMemory<byte>(System.Text.Encoding.ASCII.GetBytes(data.BrightChainAddress(hostName: "hostname").ToString()));
        }

        internal static BrightHandle InternalDeserialize(ReadOnlyMemory<byte> data)
        {
            var uriString = System.Text.Encoding.ASCII.GetString(data.ToArray());
            return new BrightHandle(new Uri(uriString));
        }

    }
}
