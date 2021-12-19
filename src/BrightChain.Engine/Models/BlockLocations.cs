using System.Collections.Generic;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Models.Hashes;

namespace BrightChain.Engine.Models;

public struct BlockLocations
{
    public readonly BlockHash BlockId;
    public readonly Dictionary<BlockLocationType, BlockLocation> Locations;

    public BlockLocations(BlockHash blockHash)
    {
        this.BlockId = blockHash;
        this.Locations = new Dictionary<BlockLocationType, BlockLocation>();
    }
}
