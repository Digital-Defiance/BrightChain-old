using BrightChain.Enumerations;
using BrightChain.Exceptions;
using BrightChain.Models.Blocks;
using BrightChain.Models.Blocks.Chains;
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
        public static bool PerformValidation(this Block block, out IEnumerable<BrightChainValidationException> validationExceptions)
        {
            var exceptions = new List<BrightChainValidationException>();

            if (block.BlockSize == BlockSize.Unknown)
            {
                exceptions.Add(new BrightChainValidationException(
                    element: nameof(block.BlockSize),
                    message: String.Format("{0} is invalid: {1}", nameof(block.BlockSize), block.BlockSize.ToString())));
            }

            if (block.BlockSize != BlockSizeMap.BlockSize(block.Data.Length))
            {
                exceptions.Add(new BrightChainValidationException(
                    element: nameof(block.BlockSize),
                    message: String.Format("{0} is invalid: {1}, actual {2} bytes", nameof(block.BlockSize), block.BlockSize.ToString(), block.Data.Length)));
            }

            var recomputedHash = new BlockHash(block);
            if (block.Id != recomputedHash)
            {
                exceptions.Add(new BrightChainValidationException(
                    element: nameof(block.Id),
                    message: String.Format("{0} is invalid: {1}, actual {2}", nameof(block.Id), block.Id.ToString(), recomputedHash.ToString())));
            }

            if (block.Data.Length != BlockSizeMap.BlockSize(block.BlockSize))
            {
                exceptions.Add(new BrightChainValidationException(
                    element: nameof(block.Data),
                    message: String.Format("{0} has no data: {1} bytes", nameof(block.Data), block.Data.Length)));
            }

            if (block.StorageContract.ByteCount != block.Data.Length)
            {
                exceptions.Add(new BrightChainValidationException(
                    element: nameof(block.StorageContract.ByteCount),
                    message: String.Format("{0} length {1} does not match data length of {1} bytes", nameof(block.StorageContract.ByteCount), block.StorageContract.ByteCount, block.Data.Length)));
            }

            if (!block.RedundancyContract.StorageContract.Equals(block.StorageContract))
            {
                exceptions.Add(new BrightChainValidationException(
                    element: nameof(block.RedundancyContract.StorageContract),
                    message: String.Format("{0} on redundancy contract does not match StorageContract", nameof(block.StorageContract))));
            }

            // TODO: Validate signature

            // fill the "out" variable
            validationExceptions = exceptions.ToArray();

            return (exceptions.Count == 0);
        }

        public static bool PerformValidation(this ConstituentBlockListBlock cblBlock, out IEnumerable<BrightChainValidationException> validationExceptions)
        {
            bool baseValidation = PerformValidation(cblBlock.AsBlock, out validationExceptions);
            if (!baseValidation)
            {
                return baseValidation;
            }

            var exceptions = new List<BrightChainValidationException>();

            // TODO: validate all data against finalDataHash

            // fill the "out" variable
            validationExceptions = exceptions.ToArray();

            return (exceptions.Count == 0);
        }
    }
}
