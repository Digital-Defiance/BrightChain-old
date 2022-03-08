using System;
using System.Text;
using BrightChain.Engine.Models.Blocks.DataObjects;

namespace BrightChain.Engine.Faster.Indices;

public class BrightHandleIndexValue : BrightChainIndexValue
{
    public readonly BrightHandle BrightHandle;

    public BrightHandleIndexValue(BrightHandle brightHandle)
        : base(data: new ReadOnlyMemory<byte>(
            array: Encoding.ASCII.GetBytes(
                s: brightHandle.BrightChainAddress(
                        hostName: "hostname")
                    .ToString())))
    {
        this.BrightHandle = brightHandle;
    }

    public BrightHandleIndexValue(ReadOnlyMemory<byte> data)
        : base(data: data)
    {
        var uriString = Encoding.ASCII.GetString(bytes: data.ToArray());
        this.BrightHandle = new BrightHandle(brightChainAddress: new Uri(uriString: uriString));
    }
}
