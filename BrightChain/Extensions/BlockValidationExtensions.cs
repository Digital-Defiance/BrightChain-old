using BrightChain.Enumerations;
using BrightChain.Exceptions;
using BrightChain.Models.Blocks;
using System;
using System.Collections.Generic;

namespace BrightChain.Extensions
{
    public static class BlockValidationExtensions
    {
        /// <summary>
        /// return true or throw an exception with the error
        /// </summary>
        /// <returns></returns>
        public static bool PerformValidation(this Block block, out List<BrightChainValidationException> validationExceptions)
        {
            validationExceptions = new List<BrightChainValidationException>();

            if (block.BlockSize == BlockSize.Unknown)
            {
                validationExceptions.Add(new BrightChainValidationException(
                    element: nameof(block.BlockSize),
                    message: String.Format("{0} is invalid: {1}", nameof(block.BlockSize), block.BlockSize.ToString())));
            }

            if (block.BlockSize != BlockSizeMap.BlockSize(block.Data.Length))
            {
                validationExceptions.Add(new BrightChainValidationException(
                    element: nameof(block.BlockSize),
                    message: String.Format("{0} is invalid: {1}, actual {2} bytes", nameof(block.BlockSize), block.BlockSize.ToString(), block.Data.Length)));
            }

            var recomputedHash = new BlockHash(block);
            if (block.Id != recomputedHash)
            {
                validationExceptions.Add(new BrightChainValidationException(
                    element: nameof(block.Id),
                    message: String.Format("{0} is invalid: {1}, actual {2}", nameof(block.Id), block.Id, recomputedHash)));
            }

            if (block.Data.Length != BlockSizeMap.BlockSize(block.BlockSize))
            {
                validationExceptions.Add(new BrightChainValidationException(
                    element: nameof(block.Data),
                    message: String.Format("{0} has no data: {1} bytes", nameof(block.Data), block.Data.Length)));
            }

            if (block.StorageContract.ByteCount != block.Data.Length)
            {
                validationExceptions.Add(new BrightChainValidationException(
                    element: nameof(block.StorageContract.ByteCount),
                    message: String.Format("{0} length {1} does not match data length of {1} bytes", nameof(block.StorageContract.ByteCount), block.StorageContract.ByteCount, block.Data.Length)));
            }

            if (!block.RedundancyContract.StorageContract.Equals(block.StorageContract))
            {
                validationExceptions.Add(new BrightChainValidationException(
                    element: nameof(block.RedundancyContract.StorageContract),
                    message: String.Format("{0} on redundancy contract does not match StorageContract", nameof(block.StorageContract))));
            }

            block.ValidationExceptions = validationExceptions;

            return (validationExceptions.Count == 0);
        }
    }
}
