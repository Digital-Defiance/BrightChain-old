namespace BrightChain.Engine.Models.Blocks.Chains
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using global::BrightChain.Engine.Exceptions;
    using global::BrightChain.Engine.Models.Blocks.DataObjects;

    public class BrightChain : ConstituentBlockListBlock, IEnumerable<BrightenedBlock>
    {
        private readonly IEnumerable<BrightenedBlock> _blocks;

        public BrightChain(ConstituentBlockListBlockParams blockParams, IEnumerable<BrightenedBlock> sourceBlocks)
            : base(blockParams)
        {
            if (!sourceBlocks.Any())
            {
                throw new BrightChainException(nameof(sourceBlocks));
            }

            this._blocks = sourceBlocks;
        }

        public int Count()
        {
            return this._blocks.Count();
        }

        public BrightenedBlock First()
        {
            return this._blocks.First();
        }

        public BrightenedBlock Last()
        {
            return this._blocks.ElementAt(this.Count() - 1);
        }

        public IEnumerable<BrightenedBlock> All()
        {
            return this._blocks;
        }

        public IEnumerator<BrightenedBlock> GetEnumerator()
        {
            return this._blocks.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._blocks.GetEnumerator();
        }
    }
}
