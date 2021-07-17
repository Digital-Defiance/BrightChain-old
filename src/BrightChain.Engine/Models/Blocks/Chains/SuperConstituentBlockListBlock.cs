using System;
using BrightChain.Engine.Interfaces;
using BrightChain.Engine.Models.Blocks.DataObjects;

namespace BrightChain.Engine.Models.Blocks.Chains
{
    /// <summary>
    /// A block which describes the hashes of all of the CBL blocks needed to reconstitute a resultant block.
    /// TODO: Ensure that the resultant list doesn't exceed a block, split into two lists, make a new top block, etc.
    /// TODO: Ensure that the hash of the source file
    /// TODO: Validate constituent blocks can recompose into that data (break up by tuple size), validate all blocks are same length
    /// </summary>
    public class SuperConstituentBlockListBlock : ConstituentBlockListBlock, IBlock, IDisposable, IValidatable
    {
        public SuperConstituentBlockListBlock(ConstituentBlockListBlockParams blockParams, ReadOnlyMemory<byte> data)
        : base(
              blockParams: blockParams)
        {
            // TODO : if finalBlockHash is null, reconstitute and compute- or accept the validation result's hash essentially?
        }

        public new bool Validate()
        {
            // TODO: perform additional validation as described above
            return base.Validate();
        }
    }
}
