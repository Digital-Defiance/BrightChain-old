using System;
using BrightChain.Engine.Enumerations;

namespace BrightChain.Engine.Models;

public struct BlockLocation
{
    public readonly BlockLocationType LocationType;
    public readonly Uri Location;
    public readonly Guid NodeId;

    public BlockLocation(BlockLocationType locationType, Uri location, Guid nodeId)
    {
        this.LocationType = locationType;
        this.Location = location;
        this.NodeId = nodeId;
    }
}
