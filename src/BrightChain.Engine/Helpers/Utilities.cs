using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Models.Blocks;

namespace BrightChain.Engine.Helpers
{
    public static class Utilities
    {
        public static string HashToFormattedString(byte[] hashBytes) =>
            BitConverter.ToString(hashBytes)
                        .Replace("-", string.Empty)
                        .ToLower(culture: System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>
        /// Generate a hash of an empty array to determine the block hash byte length
        /// Used during testing.
        /// </summary>
        /// <param name="blockSize">Block size to generate zero vector for.</param>
        /// <param name="blockHash">Hash of the zero vector for the block.</param>
        public static void GenerateZeroVectorAndVerify(BlockSize blockSize, out BlockHash blockHash)
        {
            var blockBytes = new byte[BlockSizeMap.BlockSize(blockSize)];
            Array.Fill<byte>(blockBytes, 0);
            blockHash = new BlockHash(blockType: typeof(Block), dataBytes: blockBytes);
            if (blockHash.HashBytes.Length != (BlockHash.HashSize / 8))
            {
                throw new BrightChainException("BlockHash size mismatch.");
            }
        }

        public static Dictionary<BlockSize, BlockHash> ZeroVectorLookup = new Dictionary<BlockSize, BlockHash>()
        {
            { BlockSize.Unknown, null }, // Impossible
            { BlockSize.Tiny,  new BlockHash(blockType: typeof(Block), originalBlockSize: BlockSize.Tiny, providedHashBytes: Convert.FromHexString("5f70bf18a086007016e948b04aed3b82103a36bea41755b6cddfaf10ace3c6ef"), true) },
            { BlockSize.Small, new BlockHash(blockType: typeof(Block), originalBlockSize: BlockSize.Small, providedHashBytes: Convert.FromHexString("ad7facb2586fc6e966c004d7d1d16b024f5805ff7cb47c7a85dabd8b48892ca7"), true) },
            { BlockSize.Message, new BlockHash(blockType: typeof(Block), originalBlockSize: BlockSize.Message, providedHashBytes: Convert.FromHexString("076a27c79e5ace2a3d47f9dd2e83e4ff6ea8872b3c2218f66c92b89b55f36560"), true) },
            { BlockSize.Medium, new BlockHash(blockType: typeof(Block), originalBlockSize: BlockSize.Medium, providedHashBytes: Convert.FromHexString("30e14955ebf1352266dc2ff8067e68104607e750abb9d3b36582b8af909fcb58"), true) },
            { BlockSize.Large, new BlockHash(blockType: typeof(Block), originalBlockSize: BlockSize.Large, providedHashBytes: Convert.FromHexString("bb9f8df61474d25e71fa00722318cd387396ca1736605e1248821cc0de3d3af8"), true) },
        };

        public static BlockHash GetZeroVector(BlockSize blockSize)
        {
            BlockHash expectedVector;
            var b = ZeroVectorLookup.TryGetValue(blockSize, out expectedVector);
            if (!b)
            {
                throw new BrightChainException(nameof(blockSize));
            }

            return expectedVector;
        }
    }
}
