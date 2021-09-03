namespace BrightChain.Engine.Faster.Indices
{
    using BrightChain.Engine.Models.Blocks.DataObjects;

    public class BrightHandleIndexValue : BrightChainIndexValue
    {
        public readonly BrightHandle BrightHandle;

        public BrightHandleIndexValue(BrightHandle brightHandle)
            : base(data: new ReadOnlyMemory<byte>(
                System.Text.Encoding.ASCII.GetBytes(
                    brightHandle.BrightChainAddress(
                        hostName: "hostname")
                    .ToString())))
        {
            this.BrightHandle = brightHandle;
        }

        public BrightHandleIndexValue(ReadOnlyMemory<byte> data)
            : base(data)
        {
            var uriString = System.Text.Encoding.ASCII.GetString(data.ToArray());
            this.BrightHandle = new BrightHandle(new Uri(uriString));
        }
    }
}
