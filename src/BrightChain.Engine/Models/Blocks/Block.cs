using System;
using System.Collections.Generic;
using BrightChain.Engine.Attributes;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Extensions;
using BrightChain.Engine.Helpers;
using BrightChain.Engine.Interfaces;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Models.Contracts;
using BrightChain.Engine.Models.Entities;

namespace BrightChain.Engine.Models.Blocks
{
    /// <summary>
    /// The block is the base unit persisted to disk
    /// </summary>
    public abstract class Block : IBlock, IComparable<IBlock>, IComparable<Block>
    {
        public BlockHash Id { get; }

        public StorageDurationContract StorageContract { get; set; }
        [BrightChainMetadata]
        public RedundancyContract RedundancyContract { get; set; }

        public ReadOnlyMemory<byte> Data { get; protected set; }

        public BlockSize BlockSize { get; }
        public bool HashVerified { get; private set; }

        [BrightChainMetadata]
        public BlockSignature Signature { get; internal set; }
        public bool Signed => (Signature != null);
        public bool SignatureVerified { get; internal set; }

        /// <summary>
        /// A list of the blocks, in order, required to complete this block. Not persisted to disk.
        /// Generally only used during construction of a chain
        /// </summary>
        public IEnumerable<Block> ConstituentBlocks { get; protected set; }

        /// <summary>
        /// Emits the serialization of the block minus data and any ignored attributes (including itself).
        /// </summary>
        public ReadOnlyMemory<byte> Metadata =>
            this.MetadataBytes();

        public abstract Block NewBlock(BlockParams blockArguments, ReadOnlyMemory<byte> data);

        public Block AsBlock => this;

        public IEnumerable<BrightChainValidationException> ValidationExceptions { get; private set; }

        public Block(BlockParams blockArguments, ReadOnlyMemory<byte> data)
        {
            if (BlockSizeMap.BlockSize(data.Length) != blockArguments.BlockSize)
            {
                throw new BrightChainException("Block size mismatch");
            }

            BlockSize = blockArguments.BlockSize;
            StorageContract = new StorageDurationContract(
                RequestTime: blockArguments.RequestTime,
                KeepUntilAtLeast: blockArguments.KeepUntilAtLeast,
                ByteCount: data.Length,
                PrivateEncrypted: blockArguments.PrivateEncrypted);
            RedundancyContract = new RedundancyContract(
                StorageContract: StorageContract,
                RedundancyContractType: blockArguments.Redundancy);
            Data = data;
            Id = new BlockHash(this); // must happen after data is in place
            ConstituentBlocks = new Block[] { };
            HashVerified = Validate(); // also fills in any validation errors in the array
            Signature = null;
            SignatureVerified = false;
        }

        /// <summary>
        /// XORs this block with another/randomizer block
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public Block XOR(IBlock other)
        {
            if (other is SourceBlock)
            {
                throw new BrightChainException("Unexpected SourceBlock");
            }

            if (Data.Length != other.Data.Length)
            {
                throw new BrightChainException("BlockSize mismatch");
            }

            DateTime keepUntil = DateTime.Compare(StorageContract.KeepUntilAtLeast, other.StorageContract.KeepUntilAtLeast) > 0 ?
                StorageContract.KeepUntilAtLeast :
                other.StorageContract.KeepUntilAtLeast;
            RedundancyContractType redundancy = RedundancyContract.RedundancyContractType > other.RedundancyContract.RedundancyContractType ?
                RedundancyContract.RedundancyContractType :
                other.RedundancyContract.RedundancyContractType;
            int blockSize = BlockSizeMap.Map[BlockSize];
            byte[] xorData = new byte[blockSize];
            for (int i = 0; i < blockSize; i++)
            {
                xorData[i] = Data.Slice(start: i, length: 1).ToArray()[0];
            }

            var result = NewBlock(
                blockArguments: new BlockParams(
                    blockSize: BlockSize,
                    requestTime: DateTime.Now,
                    keepUntilAtLeast: keepUntil,
                    redundancy: redundancy,
                    allowCommit: true,
                    privateEncrypted: StorageContract.PrivateEncrypted
                ),
                data: new ReadOnlyMemory<byte>(xorData)
            ); // these XOR functions should be one of the only places this even happens
            var newList = new List<IBlock>(ConstituentBlocks);
            if (!(this is SourceBlock))
            {
                newList.Add(this);
            }

            if (!(other is SourceBlock))
            {
                newList.Add(other);
            }

            result.ConstituentBlocks = (IEnumerable<Block>)newList.ToArray();
            return result;
        }

        /// <summary>
        /// XORs this block with a list of other/randomizer blocks
        /// </summary>
        /// <param name="others"></param>
        /// <returns></returns>
        public Block XOR(IBlock[] others)
        {
            DateTime keepUntil = StorageContract.KeepUntilAtLeast;
            RedundancyContractType redundancy = RedundancyContract.RedundancyContractType;
            int blockSize = BlockSizeMap.Map[BlockSize];
            var newList = new List<Block>(ConstituentBlocks);
            if (!(this is SourceBlock))
            {
                newList.Add(this);
            }

            byte[] xorData = Data.ToArray();

            foreach (Block b in others)
            {
                if (b is SourceBlock)
                {
                    throw new BrightChainException("Unexpected SourceBlock");
                }

                if (b.BlockSize != BlockSize)
                {
                    throw new BrightChainException("BlockSize mismatch");
                }

                keepUntil = (b.StorageContract.KeepUntilAtLeast > keepUntil) ? b.StorageContract.KeepUntilAtLeast : keepUntil;
                redundancy = (b.RedundancyContract.RedundancyContractType > redundancy) ? b.RedundancyContract.RedundancyContractType : redundancy;
                byte[] xorWith = b.Data.ToArray();
                for (int i = 0; i < blockSize; i++)
                {
                    xorData[i] = (byte)(xorData[0] ^ xorWith[i]);
                }

                newList.Add(b);
            }

            var result = NewBlock(
                new BlockParams(
                    blockSize: BlockSize,
                    requestTime: System.DateTime.Now,
                    keepUntilAtLeast: keepUntil,
                    redundancy: redundancy,
                    allowCommit: true,
                    privateEncrypted: StorageContract.PrivateEncrypted), // these XOR functions should be one of the only places this even happens
                data: new ReadOnlyMemory<byte>(xorData));
            result.ConstituentBlocks = newList.ToArray();
            return result;
        }

        public BlockSignature Sign(Agent user, string password)
        {
            throw new NotImplementedException();
            SignatureVerified = true;
        }

        public static bool operator ==(Block a, Block b)
        {
            return ReadOnlyMemoryComparer<byte>.Compare(a.Data, b.Data) == 0;
        }

        public static bool operator !=(Block a, Block b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            return obj is Block ? ReadOnlyMemoryComparer<byte>.Compare(Data, (obj as Block).Data) == 0 : false;
        }

        public override int GetHashCode()
        {
            return Data.GetHashCode();
        }

        public bool Validate()
        {
            IEnumerable<BrightChainValidationException> validationExceptions;
            var result = this.PerformValidation(out validationExceptions);
            ValidationExceptions = validationExceptions;
            return result;
        }

        public abstract void Dispose();

        public int CompareTo(IBlock other)
        {
            return other is null ? -1 : ReadOnlyMemoryComparer<byte>.Compare(Data, other.Data);
        }

        public int CompareTo(Block other)
        {
            return other is null ? -1 : ReadOnlyMemoryComparer<byte>.Compare(Data, other.Data);
        }
    }
}
