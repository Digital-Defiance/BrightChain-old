namespace BrightChain.Engine.Faster
{
    using BrightChain.Engine.Faster.Enumerations;

    public struct BlockSessionAddresses
    {
        public readonly Dictionary<CacheStoreType, long> Addresses;

        public BlockSessionAddresses(Dictionary<CacheStoreType, long> addresses)
        {
            this.Addresses = addresses;
        }
    }
}
