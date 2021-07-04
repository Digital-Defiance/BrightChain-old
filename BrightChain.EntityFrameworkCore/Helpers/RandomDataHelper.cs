﻿using BrightChain.Enumerations;
using BrightChain.Exceptions;
using BrightChain.Models.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace BrightChain.Helpers
{
    public static class RandomDataHelper
    {
        private static IEnumerable<byte> RandomBytes(int length)
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
            var length = BlockSizeMap.BlockSize(blockSize);

            if (inputData.Length > length)
            {
                throw new BrightChainException("data length too long");
            }
            else if (inputData.Length == length)
            {
                return inputData;
            }

            var bytes = new List<byte>(inputData.ToArray());
            bytes.AddRange(RandomBytes(length - inputData.Length));

            if (bytes.Count != length)
            {
                throw new BrightChainException("math error");
            }

            return new ReadOnlyMemory<byte>(bytes.ToArray());
        }
    }
}