namespace BrightChain.Engine.Models.Blocks.Chains
{
    using System.Collections;
    using System.Collections.Generic;

    public class BrightChain : IEnumerable<BrightenedBlock>
    {
        private IEnumerable<BrightenedBlock> _blocks;

        public BrightChain(IEnumerable<BrightenedBlock> sourceBlocks)
        {
            this._blocks = sourceBlocks;
        }

        public IEnumerator<BrightenedBlock> GetEnumerator()
            => this._blocks.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => this._blocks.GetEnumerator();
    }
}
