﻿namespace BrightChain.Engine.Models.Blocks.Chains
{
    using System;
    using global::BrightChain.Engine.Interfaces;
    using global::BrightChain.Engine.Models.Blocks.DataObjects;
    using ProtoBuf;

    /// <summary>
    /// A block which describes the hashes of all of the CBL blocks needed to reconstitute a resultant block.
    /// TODO: Ensure that the resultant list doesn't exceed a block, split into two lists, make a new top block, etc.
    /// TODO: Ensure that the hash of the source file
    /// TODO: Validate constituent blocks can recompose into that data (break up by tuple size), validate all blocks are same length
    /// </summary>
    [ProtoContract]
    public class SuperConstituentBlockListBlock : ConstituentBlockListBlock, IBlock, IDisposable, IValidatable
    {
        public SuperConstituentBlockListBlock(ConstituentBlockListBlockParams blockParams)
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
