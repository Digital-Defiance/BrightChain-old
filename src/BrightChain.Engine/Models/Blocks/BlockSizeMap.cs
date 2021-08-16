namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using System.Collections.Generic;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Models.Hashes;

    /// <summary>
    ///     Map of the block size enumeration values to their actual sizes.
    /// </summary>
    public static class BlockSizeMap
    {
        /// <summary>
        /// Smallest block size. Best for encryption keys, etc.
        /// </summary>
        public const int NanoSize = 128;

        /// <summary>
        /// Best for extremely small payloads. 256 bytes.
        /// </summary>
        public const int MicroSize = 256;

        /// <summary>
        /// Best for small payloads like messages. 512 bytes.
        /// </summary>
        public const int MessageSize = 512;

        /// <summary>
        /// Best for small payloads larger than basic messages. 1024 bytes. 1K.
        /// </summary>
        public const int TinySize = 1024;

        /// <summary>
        /// Best for small files. 4024 bytes. 4K.
        /// </summary>
        public const int SmallSize = 4 * 1024;

        /// <summary>
        /// Medium block size. Best for small to moderately sized data. 1,048,576 bytes. 1M.
        /// </summary>
        public const int MediumSize = 1024 * 1024;

        /// <summary>
        /// Large block size. Best for large data. 4,194.304 bytes. 4M.
        /// </summary>
        public const int LargeSize = 4 * 1024 * 1024;

        public static readonly Dictionary<BlockSize, int> Map = new()
        {
            { Enumerations.BlockSize.Unknown, -1 },
            { Enumerations.BlockSize.Nano, NanoSize },
            { Enumerations.BlockSize.Micro, MicroSize },
            { Enumerations.BlockSize.Message, MessageSize },
            { Enumerations.BlockSize.Tiny, TinySize },
            { Enumerations.BlockSize.Small, SmallSize },
            { Enumerations.BlockSize.Medium, MediumSize },
            { Enumerations.BlockSize.Large, LargeSize },
        };

        public static readonly Dictionary<BlockSize, int> HashesPerBlockMap = new()
        {
            { Enumerations.BlockSize.Unknown, -1 },
            { Enumerations.BlockSize.Nano, NanoSize / DataHash.HashSizeBytes },
            { Enumerations.BlockSize.Micro, MicroSize / DataHash.HashSizeBytes },
            { Enumerations.BlockSize.Message, MessageSize / DataHash.HashSizeBytes },
            { Enumerations.BlockSize.Tiny, TinySize / DataHash.HashSizeBytes },
            { Enumerations.BlockSize.Small, SmallSize / DataHash.HashSizeBytes },
            { Enumerations.BlockSize.Medium, MediumSize / DataHash.HashSizeBytes },
            { Enumerations.BlockSize.Large, LargeSize / DataHash.HashSizeBytes },
        };

        /// <summary>
        /// Map of Zero Vector Block generators by block size, created from known hashes.
        /// </summary>
        public static readonly Dictionary<BlockSize, BlockHash> ZeroVectorMap = new()
        {
            {
                Enumerations.BlockSize.Unknown,
                null // Impossible
            },
            {
                Enumerations.BlockSize.Nano,
                new BlockHash(
                    blockType: typeof(ZeroVectorBlock),
                    originalBlockSize: Enumerations.BlockSize.Nano,
                    providedHashBytes: Convert.FromHexString("38723a2e5e8a17aa7950dc008209944e898f69a7bd10a23c839d341e935fd5ca"),
                    computed: true)
            },
            {
                Enumerations.BlockSize.Micro,
                new BlockHash(
                    blockType: typeof(ZeroVectorBlock),
                    originalBlockSize: Enumerations.BlockSize.Micro,
                    providedHashBytes: Convert.FromHexString("5341e6b2646979a70e57653007a1f310169421ec9bdd9f1a5648f75ade005af1"),
                    computed: true)
            },
            {
                Enumerations.BlockSize.Tiny,
                new BlockHash(
                    blockType: typeof(ZeroVectorBlock),
                    originalBlockSize: Enumerations.BlockSize.Tiny,
                    providedHashBytes: Convert.FromHexString("5f70bf18a086007016e948b04aed3b82103a36bea41755b6cddfaf10ace3c6ef"),
                    computed: true)
            },
            {
                Enumerations.BlockSize.Small,
                new BlockHash(
                    blockType: typeof(ZeroVectorBlock),
                    originalBlockSize: Enumerations.BlockSize.Small,
                    providedHashBytes: Convert.FromHexString("ad7facb2586fc6e966c004d7d1d16b024f5805ff7cb47c7a85dabd8b48892ca7"),
                    computed: true)
            },
            {
                Enumerations.BlockSize.Message,
                new BlockHash(
                    blockType: typeof(ZeroVectorBlock),
                    originalBlockSize: Enumerations.BlockSize.Message,
                    providedHashBytes: Convert.FromHexString("076a27c79e5ace2a3d47f9dd2e83e4ff6ea8872b3c2218f66c92b89b55f36560"),
                    computed: true)
            },
            {
                Enumerations.BlockSize.Medium,
                new BlockHash(
                    blockType: typeof(ZeroVectorBlock),
                    originalBlockSize: Enumerations.BlockSize.Medium,
                    providedHashBytes: Convert.FromHexString("30e14955ebf1352266dc2ff8067e68104607e750abb9d3b36582b8af909fcb58"),
                    computed: true)
            },
            {
                Enumerations.BlockSize.Large,
                new BlockHash(
                    blockType: typeof(ZeroVectorBlock),
                    originalBlockSize: Enumerations.BlockSize.Large,
                    providedHashBytes: Convert.FromHexString("bb9f8df61474d25e71fa00722318cd387396ca1736605e1248821cc0de3d3af8"),
                    computed: true)
            },
        };

        /// <summary>
        ///     Map a block size enumeration to its actual size in bytes.
        /// </summary>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        public static int BlockSize(BlockSize blockSize)
        {
            if (!Map.ContainsKey(blockSize))
            {
                throw new KeyNotFoundException(nameof(blockSize));
            }

            return Map[blockSize];
        }

        public static bool LengthIsValid(int length)
        {
            foreach (var pair in Map)
            {
                if (pair.Value == length)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Map a block size enumeration to the number of hashes it can contain.
        /// </summary>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        public static long HashesPerBlock(BlockSize blockSize, int exponent = 1)
        {
            if (!HashesPerBlockMap.ContainsKey(blockSize))
            {
                throw new KeyNotFoundException(nameof(blockSize));
            }

            var value = HashesPerBlockMap[blockSize];
            if (exponent <= 1)
            {
                return value;
            }

            return (long)Math.Pow(value, exponent);
        }

        /// <summary>
        ///     Map a block size in bytes back to its block size enumeration.
        /// </summary>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        public static BlockSize BlockSize(int blockSize)
        {
            foreach (var pair in Map)
            {
                if (pair.Value == blockSize)
                {
                    return pair.Key;
                }
            }

            throw new KeyNotFoundException(nameof(blockSize));
        }

        public static BlockHash ZeroVectorHash(BlockSize blockSize)
        {
            BlockHash expectedVector;
            var b = BlockSizeMap.ZeroVectorMap.TryGetValue(blockSize, out expectedVector);
            if (!b)
            {
                throw new BrightChainException(nameof(blockSize));
            }

            return expectedVector;
        }

        public static ZeroVectorBlock ZeroVectorBlock(BlockSize blockSize)
        {
            return new ZeroVectorBlock(blockSize);
        }
    }
}
