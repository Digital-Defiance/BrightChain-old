using BrightChain.Models.Blocks;
using System;

namespace BrightChain.Models.Events
{
    public class BlockEventArgs : EventArgs
    {
        public readonly Block Block;
    }
}
