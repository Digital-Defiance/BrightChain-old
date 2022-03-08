namespace BrightChain.Engine.Faster;

/// <summary>
///     User context to measure latency and/or check read result.
/// </summary>
public struct BrightChainFasterCacheContext
{
    public int type;
    public long ticks;
}
