using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Models.Hashes;
using FASTER.core;

namespace BrightChain.Engine.Faster.Functions;

public class BrightChainBlockHashAdvancedFunctions : FunctionsBase<BlockHash, BlockData, BlockData, BlockData,
    BrightChainFasterCacheContext>
{
    public BrightChainBlockHashAdvancedFunctions(bool locking = false)
        : base(locking: locking)
    {
    }

    public override void ConcurrentReader(ref BlockHash key, ref BlockData input, ref BlockData value, ref BlockData dst)
    {
        dst = value;
    }

    public override bool ConcurrentWriter(ref BlockHash key, ref BlockData src, ref BlockData dst)
    {
        dst = src;
        return true;
    }

    public override void SingleWriter(ref BlockHash key, ref BlockData src, ref BlockData dst)
    {
        dst = src;
    }

    public override void InitialUpdater(ref BlockHash key, ref BlockData input, ref BlockData value, ref BlockData output)
    {
        value = input;
        output = input;
    }
}
