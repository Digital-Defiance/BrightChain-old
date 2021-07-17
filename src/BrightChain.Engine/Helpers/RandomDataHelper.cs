using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Models.Blocks;

namespace BrightChain.Engine.Helpers
{
    public static class RandomDataHelper
    {
        public static byte[] RandomBytes(int length)
        {
            using (var rng = RandomNumberGenerator.Create()) // TODO: guarantee is CSPRNG
            {
                var rnd = new byte[length];
                rng.GetBytes(rnd);
                return rnd;
            }
        }

        public static ReadOnlyMemory<byte> RandomReadOnlyBytes(int length) =>
            new ReadOnlyMemory<byte>(RandomBytes(length: length).ToArray());

        public static ReadOnlyMemory<byte> DataFiller(ReadOnlyMemory<byte> inputData, BlockSize blockSize)
        {
            var iBlockSize = BlockSizeMap.BlockSize(blockSize);

            if (inputData.Length > iBlockSize)
            {
                throw new BrightChainException("data length too long");
            }
            else if (inputData.Length == iBlockSize)
            {
                return inputData;
            }

            var bytes = new List<byte>(inputData.ToArray());
            bytes.AddRange(RandomBytes(iBlockSize - inputData.Length));

            if (bytes.Count != iBlockSize)
            {
                throw new BrightChainException("math error");
            }

            return new ReadOnlyMemory<byte>(bytes.ToArray());
        }
    }
}
