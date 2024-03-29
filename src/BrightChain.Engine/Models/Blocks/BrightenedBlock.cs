﻿using System.Linq;

namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using System.Collections.Generic;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Hashes;
    using ProtoBuf;

    /// <summary>
    /// Block that is able to be stored, rolled back, committed, or prevented from being stored.
    /// </summary>
    [ProtoContract]
    public class BrightenedBlock : Block, IDisposable, ITransactable, ITransactableBlock, IComparable<BrightenedBlock>, IComparable<ITransactableBlock>, IEquatable<IBlock>
    {
        public BrightenedBlock(BrightenedBlockParams blockParams, ReadOnlyMemory<byte> data, IEnumerable<BlockHash> constituentBlockHashes = null)
            : base(
                blockParams: blockParams,
                data: data,
                constituentBlockHashes: constituentBlockHashes)
        {
            this.CacheManager = blockParams.CacheManager;
            this.State = !blockParams.AllowCommit ? TransactionStatus.DoNotWrite : TransactionStatus.Uncommitted;
            this.disposedValue = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrightenedBlock"/> class.
        /// For test methods.
        /// </summary>
        internal BrightenedBlock()
            : base(
                blockParams: new BlockParams(
                    blockSize: BlockSize.Message,
                    requestTime: DateTime.Now,
                    keepUntilAtLeast: DateTime.MaxValue,
                    redundancy: RedundancyContractType.HeapAuto,
                    privateEncrypted: false,
                    originalType: typeof(BrightenedBlock)),
                data: new ReadOnlyMemory<byte>() { },
                constituentBlockHashes: new List<BlockHash>())
        {
        }

        public BrightenedBlock AsTransactableBlock => this;

        /// <summary>
        /// Gets a bool indicating whether the block's data has been loaded from the attached cache, or kept after persisting to cache.
        /// </summary>
        public bool DataInMemory { get; }

        /// <summary>
        /// Boolean indicating whether our data has been disposed.
        /// </summary>
        private bool disposedValue;

        public ICacheManager<BlockHash, BrightenedBlock> CacheManager { get; internal set; }

        public TransactionStatus State { get; private set; }

        public bool AllowCommit
        {
            get
            {
                return new TransactionStatus[]
                {
                    TransactionStatus.Uncommitted,
                    TransactionStatus.Committed,
                    TransactionStatus.DroppedCommitted,
                    TransactionStatus.WrittenUnconfirmed,
                    TransactionStatus.RolledBackRewrite,
                }.Contains(this.State);
            }
        }

        public static bool operator ==(BrightenedBlock a, BrightenedBlock b)
        {
            return a.BlockSize == b.BlockSize && a.StoredData.Equals(b.StoredData);
        }

        public static bool operator !=(BrightenedBlock a, BrightenedBlock b)
        {
            return !a.Equals(b);
        }

        public void SetCacheManager(ICacheManager<BlockHash, BrightenedBlock> cacheManager)
        {
            this.CacheManager = cacheManager;
        }

        /// <summary>
        /// Commit the block to disk
        /// </summary>
        /// <exception cref="BrightChainException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public void Commit()
        {
            switch (this.State)
            {
                case TransactionStatus.DoNotWrite:
                case TransactionStatus.RolledBackDoNotWrite:
                    throw new BrightChainException("Block is not allowed to be committed");

                case TransactionStatus.RolledBackRewrite:
                case TransactionStatus.Uncommitted:
                    this.State = TransactionStatus.WrittenUnconfirmed;
                    throw new NotImplementedException();
                    return;

                case TransactionStatus.WrittenUnconfirmed:
                    this.State = TransactionStatus.Committed;
                    throw new NotImplementedException();
                    return;

                case TransactionStatus.Committed:
                case TransactionStatus.DroppedCommitted:
                    return;

                default:
                    throw new BrightChainException(nameof(this.State));
            }
        }

        public void Rollback(bool rewrite = false)
        {
            switch (this.State)
            {
                case TransactionStatus.DoNotWrite:
                case TransactionStatus.RolledBackDoNotWrite:
                    return;

                case TransactionStatus.RolledBackRewrite:
                case TransactionStatus.Uncommitted:
                case TransactionStatus.WrittenUnconfirmed:
                    this.State = rewrite ? TransactionStatus.RolledBackRewrite : TransactionStatus.RolledBackDoNotWrite;
                    throw new NotImplementedException();
                    return;

                case TransactionStatus.DroppedCommitted:
                case TransactionStatus.Committed:
                    throw new BrightChainException("Block already committed");

                default:
                    throw new BrightChainException(nameof(this.State));
            }
        }

        public override BrightenedBlockParams BlockParams => new BrightenedBlockParams(
                cacheManager: this.CacheManager,
                allowCommit: State.Equals(TransactionStatus.DoNotWrite) ? false : true,
                blockParams: new BlockParams(
                    blockSize: this.BlockSize,
                    requestTime: this.StorageContract.RequestTime,
                    keepUntilAtLeast: this.StorageContract.KeepUntilAtLeast,
                    redundancy: this.StorageContract.RedundancyContractType,
                    privateEncrypted: this.StorageContract.PrivateEncrypted,
                    originalType: Type.GetType(this.OriginalAssemblyTypeString)));

        public override bool Equals(object obj)
        {
            return obj is BrightenedBlock blockObj ? this.StoredData.Equals(blockObj.StoredData) : false;
        }

        public override int GetHashCode()
        {
            return this.StoredData.GetHashCode();
        }

        public int CompareTo(BrightenedBlock other)
        {
            return this.StoredData.CompareTo(other.StoredData);
        }

        public int CompareTo(ITransactableBlock other)
        {
            return this.StoredData.CompareTo(other.StoredData);
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TransactableBlock()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        /// <summary>
        /// Dispose block data and memory contents.
        /// </summary>
        public override void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose block data and memory contents.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                this.Rollback();

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this.disposedValue = true;
            }
        }
    }
}
