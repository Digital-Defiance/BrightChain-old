using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Models.Blocks.DataObjects;

namespace BrightChain.Engine.Models.Blocks.Chains
{
    /// <summary>
    /// Represents a virtual map of all the contituent tuple-sets/blocks in a given source/reconstructed file. These cannot themselves be committed to disk
    /// The block datas may not actually be loaded in memory, but the appropriate blocks will be loaded (all non-local will be pulled into the cache first) relative to their access offsets.
    /// source file -> blockchainfilemap -> commit
    /// pull blocks from pool -> blockchainfilemap -> read.
    /// </summary>
    public class BlockChainFileMap
    {
        private IAsyncEnumerable<TupleStripe> TupleStripes { get; set; }

        public ConstituentBlockListBlock ConstituentBlockListBlock { get; }

        public static async IAsyncEnumerable<IEnumerable<T>> TakeIntoGroupsOf<T>(IEnumerable<T> list, int parts)
        {
            var result = list.Select((item, index) => new { index, item })
                       .GroupBy(x => x.index % parts)
                       .Select(x => x.Select(y => y.item));
            yield return (IEnumerable<T>)result;
            yield break;
        }

        public BlockChainFileMap(ConstituentBlockListBlock cblBlock, IAsyncEnumerable<TupleStripe> tupleStripes = null)
        {
            this.ConstituentBlockListBlock = cblBlock;
            this.TupleStripes = tupleStripes;
        }

        private BlockChainFileMap()
        {

        }

        public async IAsyncEnumerable<TupleStripe> ReconstructTupleStripes()
        {
            BlockHash[] listBlocks = (BlockHash[])this.ConstituentBlockListBlock.ConstituentBlocks;
            if ((listBlocks.Length % this.ConstituentBlockListBlock.TupleCount) != 0)
            {
                throw new BrightChainException("CBL length is not a multiple of the tuple count");
            }

            var tupleGroups = TakeIntoGroupsOf(listBlocks, this.ConstituentBlockListBlock.TupleCount);
            await foreach (var tupleGroup in tupleGroups)
            {
                throw new NotImplementedException();
                //yield return new TupleStripe(this.ConstituentBlockListBlock.TupleCount, this.ConstituentBlockListBlock.BlockSize, tupleGroup);
            }

            yield break;
        }

        public async IAsyncEnumerable<Block> ConsolidateTuplesToChainAsyc()
        {
            await foreach (TupleStripe tupleStripe in (this.TupleStripes is null) ? this.ReconstructTupleStripes() : this.TupleStripes)
            {
                yield return tupleStripe.Consolidate();
            }

            yield break;
        }

        public static async IAsyncEnumerator<byte> ReadValidatedChainToBytes(IAsyncEnumerable<Block> source)
        {
            await foreach (var block in source)
            {
                if (!block.Validate())
                {
                    throw new BrightChainValidationEnumerableException(block.ValidationExceptions, block.Id.ToString());
                }

                foreach (byte b in block.Data.ToArray())
                {
                    yield return b;
                }
            }
        }
    }
}
