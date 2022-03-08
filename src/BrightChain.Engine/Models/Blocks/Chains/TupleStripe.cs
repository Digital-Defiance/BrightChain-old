using System;
using System.Collections.Generic;
using System.Linq;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Exceptions;

namespace BrightChain.Engine.Models.Blocks.Chains;

/// <summary>
///     A tuple stripe is a representation of the blocks used only to Recover a source block.
///     In each row, only the last block is expected to be the non-randomizer block, but order doesn't matter within a stripe.
/// </summary>
public struct TupleStripe
{
    public readonly IEnumerable<BrightenedBlock> Blocks;

    public readonly BlockSize BlockSize;

    public readonly Type OriginalType;

    public static void ValidateRandomizers(int tupleCountMatch, BlockSize blockSizeMatch, IEnumerable<BrightenedBlock> blocks)
    {
        if (tupleCountMatch != blocks.Count())
        {
            throw new BrightChainException(message: "Block length mismatch");
        }

        foreach (var block in blocks)
        {
            if (blockSizeMatch != block.BlockSize)
            {
                throw new BrightChainException(message: "block size mismatch");
            }
        }
    }

    public TupleStripe(int tupleCountMatch, BlockSize blockSizeMatch, Type originalType, IEnumerable<BrightenedBlock> brightenedBlocks)
    {
        ValidateRandomizers(tupleCountMatch: tupleCountMatch,
            blockSizeMatch: blockSizeMatch,
            blocks: brightenedBlocks);

        this.Blocks = brightenedBlocks;
        this.BlockSize = blockSizeMatch;
        this.OriginalType = originalType;
    }

    /// <summary>
    ///     XOR's the stripe's blocks back into an Identifiable Block.
    /// </summary>
    /// <returns></returns>
    public IdentifiableBlock Consolidate()
    {
        Block firstBlock = this.Blocks.First();
        // the XOR will never XOR with the same block as that would yield all zeroes. It will be skipped.
        var identifiable = new IdentifiableBlock(blockParams: firstBlock.BlockParams,
            data: firstBlock.XOR(others: this.Blocks));
        return identifiable;
        //return (IdentifiableBlock)Convert.ChangeType(identifiable, identifiable.OriginalType);
    }
}
