namespace BrightChain.Engine.Faster.Indices
{
    using BrightChain.Engine.Models.Blocks.DataObjects;

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
            this.BrightHandle = InternalDeserialize<BrightHandle>(data);
        }
    }
}
