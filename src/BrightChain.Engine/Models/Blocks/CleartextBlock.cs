using System;
using BrightChain.Engine.Models.Blocks.DataObjects;

namespace BrightChain.Engine.Models.Blocks;

public class CleartextBlock : IdentifiableBlock
{
    public CleartextBlock(BlockParams blockParams, ReadOnlyMemory<byte> cleartextData)
        : base(blockParams: blockParams,
            data: cleartextData)
    {
    }
}
