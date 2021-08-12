namespace BrightChain.Engine.Models.Blocks.DataObjects
{
    using BrightChain.Engine.Exceptions;

    [Serializable]
    public class ChainLinqBlockParams : BlockParams
    {
        public ChainLinqBlockParams(BlockParams blockParams)
       : base(
             blockSize: blockParams.BlockSize,
             requestTime: blockParams.RequestTime,
             keepUntilAtLeast: blockParams.KeepUntilAtLeast,
             redundancy: blockParams.Redundancy,
             privateEncrypted: blockParams.PrivateEncrypted,
             originalType: blockParams.OriginalType)
        {
        }

        public ChainLinqBlockParams Merge(BlockParams otherBlockParams)
        {
            if (otherBlockParams.BlockSize != this.BlockSize)
            {
                throw new BrightChainException("BlockSize mismatch");
            }

            return new ChainLinqBlockParams(blockParams: this.Merge(otherBlockParams));
        }
    }
}
