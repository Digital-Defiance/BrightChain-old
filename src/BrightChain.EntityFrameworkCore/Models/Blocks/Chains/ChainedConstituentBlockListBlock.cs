using BrightChain.Interfaces;
using BrightChain.Models.Blocks.DataObjects;
using System;

namespace BrightChain.Models.Blocks.Chains
{
    /// <summary>
    /// A block which describes the hashes of all of the blocks needed to reconstitute a resultant block.
    /// TODO: Ensure that the resultant list doesn't exceed a block, split into two lists, make a new top block, etc.
    /// TODO: Ensure that the hash of the source file
    /// TODO: Validate constituent blocks can recompose into that data (break up by tuple size), validate all blocks are same length
    /// </summary>
    public class ChainedConstituentBlockListBlock : ConstituentBlockListBlock, IBlock, IDisposable, IValidatable
    {
        public ChainedConstituentBlockListBlock(ConstituentBlockListBlockParams blockArguments, ReadOnlyMemory<byte> data)
        : base(
              blockArguments: blockArguments)
        {
            // TODO : if finalDataHash is null, reconstitute and compute- or accept the validation result's hash essentially?
        }

        public new bool Validate()
        {
            // TODO: perform additional validation as described above
            return base.Validate();
        }
    }
}
