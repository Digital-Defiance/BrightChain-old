namespace BrightChain.Engine.Models.Blocks.Chains
{
    using System.Collections;
    using System.Collections.Generic;
    using global::BrightChain.Engine.Models.Blocks.DataObjects;

    public class BrightChain : ConstituentBlockListBlock, IEnumerable<BrightenedBlock>
    {
        private IEnumerable<BrightenedBlock> _blocks;

        public BrightChain(ConstituentBlockListBlockParams blockParams, IEnumerable<BrightenedBlock> sourceBlocks)
            : base(blockParams)
        {
            this._blocks = sourceBlocks;
        }

        public IEnumerator<BrightenedBlock> GetEnumerator()
            => this._blocks.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => this._blocks.GetEnumerator();
    }
}
