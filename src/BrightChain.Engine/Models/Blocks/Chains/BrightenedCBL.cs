namespace BrightChain.Engine.Models.Blocks.Chains
{
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
