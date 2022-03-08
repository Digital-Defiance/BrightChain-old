using System;
using BrightChain.Engine.Enumerations;

namespace BrightChain.Engine.Tests.Helpers;

public static class TestHelpers
{
    public static BlockSize RandomBlockSize()
    {
        var values = Enum.GetValues(enumType: typeof(BlockSize));
        var random = new Random();
        var blockSize = (BlockSize)values.GetValue(index: random.Next(maxValue: values.Length));
        return blockSize == BlockSize.Unknown ? RandomBlockSize() : blockSize;
    }
}
