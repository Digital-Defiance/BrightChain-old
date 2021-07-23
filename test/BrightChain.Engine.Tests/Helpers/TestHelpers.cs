using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightChain.Engine.Enumerations;

namespace BrightChain.Engine.Tests.Helpers
{
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
