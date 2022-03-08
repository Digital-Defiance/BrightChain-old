using System.Collections.Generic;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Blocks.Chains;
using BrightChain.Engine.Models.Hashes;

namespace BrightChain.Engine.Extensions;

public static class BlockValidationExtensions
{
    /// <summary>
    ///     return true or throw an exception with the error
    /// </summary>
    /// <returns></returns>
    public static bool PerformValidation(this Block block, out IEnumerable<BrightChainValidationException> validationExceptions)
    {
        var exceptions = new List<BrightChainValidationException>();

        if (block.BlockSize == BlockSize.Unknown)
        {
            exceptions.Add(item: new BrightChainValidationException(
                element: nameof(block.BlockSize),
                message: string.Format(format: "{0} is invalid: {1}",
                    arg0: nameof(block.BlockSize),
                    arg1: block.BlockSize.ToString())));
        }

        if (!BlockSizeMap.LengthIsValid(length: block.Bytes.Length))
        {
            exceptions.Add(item: new BrightChainValidationException(
                element: nameof(block.Bytes.Length),
                message: string.Format(format: "{0} is not a valid data length",
                    arg0: nameof(block.Bytes.Length))));
        }

        if (block.BlockSize != BlockSizeMap.BlockSize(blockSize: block.Bytes.Length))
        {
            exceptions.Add(item: new BrightChainValidationException(
                element: nameof(block.BlockSize),
                message: string.Format(
                    format: "{0} is invalid: {1}, actual {2} bytes",
                    arg0: nameof(block.BlockSize),
                    arg1: block.BlockSize.ToString(),
                    arg2: block.Bytes.Length)));
        }

        var recomputedHash = new BlockHash(block: block);
        if (block.Id != recomputedHash)
        {
            exceptions.Add(item: new BrightChainValidationException(
                element: nameof(block.Id),
                message: string.Format(format: "{0} is invalid: {1}, actual {2}",
                    arg0: nameof(block.Id),
                    arg1: block.Id.ToString(),
                    arg2: recomputedHash.ToString())));
        }

        if (block.StorageContract.ByteCount != block.Bytes.Length)
        {
            exceptions.Add(item: new BrightChainValidationException(
                element: nameof(block.StorageContract.ByteCount),
                message: string.Format(format: "{0} length {1} does not match data length of {2} bytes",
                    arg0: nameof(block.StorageContract.ByteCount),
                    arg1: block.StorageContract.ByteCount,
                    arg2: block.Bytes.Length)));
        }

        if (!block.StorageContract.Equals(other: block.StorageContract))
        {
            exceptions.Add(item: new BrightChainValidationException(
                element: nameof(block.StorageContract),
                message: string.Format(format: "{0} on redundancy contract does not match StorageContract",
                    arg0: nameof(block.StorageContract))));
        }

        // TODO: Validate signature

        // fill the "out" variable
        validationExceptions = exceptions.ToArray();

        return exceptions.Count == 0;
    }

    public static bool PerformValidation(this ConstituentBlockListBlock cblBlock,
        out IEnumerable<BrightChainValidationException> validationExceptions)
    {
        var baseValidation = PerformValidation(block: cblBlock.AsBlock,
            validationExceptions: out validationExceptions);
        if (!baseValidation)
        {
            return baseValidation;
        }

        var exceptions = new List<BrightChainValidationException>();

        // TODO: validate all data against SourceId

        // fill the "out" variable
        validationExceptions = exceptions.ToArray();

        return exceptions.Count == 0;
    }
}
