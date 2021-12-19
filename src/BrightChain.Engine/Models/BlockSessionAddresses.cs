namespace BrightChain.Engine.Faster
{
    using System.Collections.Generic;
    using BrightChain.Engine.Faster.Enumerations;

    public struct BlockSessionAddresses
    {
        public readonly long Address;

        public BlockSessionAddresses(long address)
        {
            this.Address = address;
        }
    }
}
