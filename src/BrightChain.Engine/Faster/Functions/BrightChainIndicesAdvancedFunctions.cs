namespace BrightChain.Engine.Faster.Functions
{
    using BrightChain.Engine.Faster.Indices;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using FASTER.core;

    public class BrightChainIndicesAdvancedFunctions : FunctionsBase<string, BrightChainIndexValue, BrightChainIndexValue, BrightChainIndexValue, BrightChainFasterCacheContext>
    {
        public BrightChainIndicesAdvancedFunctions(bool locking = false)
            : base(locking: locking)
        {
        }

        public override void ConcurrentReader(ref string key, ref BrightChainIndexValue input, ref BrightChainIndexValue value, ref BrightChainIndexValue dst)
        {
            dst = value;
        }

        public override bool ConcurrentWriter(ref string key, ref BrightChainIndexValue src, ref BrightChainIndexValue dst)
        {
            dst = src;
            return true;
        }

        public override void SingleWriter(ref string key, ref BrightChainIndexValue src, ref BrightChainIndexValue dst)
        {
            dst = src;
        }

        public override void InitialUpdater(ref string key, ref BrightChainIndexValue input, ref BrightChainIndexValue value, ref BrightChainIndexValue output)
        {
            value = input;
            output = input;
        }
    }
}
