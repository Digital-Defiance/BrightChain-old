namespace BrightChain.Engine.Tests.Helpers
{
    using System;
    using BrightChain.Engine.Enumerations;

    public static class TestHelpers
    {
        public static BlockSize RandomBlockSize()
        {
            Array values = Enum.GetValues(typeof(BlockSize));
            Random random = new Random();
            var blockSize = (BlockSize)values.GetValue(random.Next(values.Length));
            return (blockSize == BlockSize.Unknown) ? RandomBlockSize() : blockSize;
        }
    }
}
