using BrightChain.Models.Blocks;
using System;

namespace BrightChain.Models.Events
{
    /// <summary>
    /// Any action related to a block will have these event args
    /// </summary>
    public class BlockEventArgs : EventArgs
    {
        public readonly Block Block;
    }
}
