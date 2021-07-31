namespace BrightChain.Engine.Models.Blocks.DataObjects
{
    using BrightChain.Engine.Exceptions;

    public class ChainLinqBlockParams : BlockParams
    {
        public readonly BlockHash Next = null;

        public ChainLinqBlockParams(BlockParams blockParams, BlockHash next = null)
       : base(
             blockSize: blockParams.BlockSize,
             requestTime: blockParams.RequestTime,
             keepUntilAtLeast: blockParams.KeepUntilAtLeast,
             redundancy: blockParams.Redundancy,
             privateEncrypted: blockParams.PrivateEncrypted)
        {
            this.Next = next;
        }

        public ChainLinqBlockParams Merge(BlockParams otherBlockParams)
        {
            if (otherBlockParams.BlockSize != this.BlockSize)
            {
                throw new BrightChainException("BlockSize mismatch");
            }

            return new ChainLinqBlockParams(
                blockParams: this.Merge(otherBlockParams),
                next: this.Next);
        }
    }
}
