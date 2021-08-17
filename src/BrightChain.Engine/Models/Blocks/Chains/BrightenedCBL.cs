namespace BrightChain.Engine.Models.Blocks.Chains
{
    using global::BrightChain.Engine.Models.Blocks.DataObjects;
    using global::BrightChain.Engine.Models.Hashes;

    public class BrightenedCBL : BrightenedBlock
    {
        public BrightenedCBL(BrightenedBlock cbl)
            : base(
                  blockParams: cbl.BlockParams,
                  data: cbl.Bytes,
                  constituentBlockHashes: cbl.ConstituentBlocks)
        {
        }
    }
}
