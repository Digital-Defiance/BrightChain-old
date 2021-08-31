namespace BrightChain.Engine.Faster.CacheManager
{
    using System;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Faster;
    using BrightChain.Engine.Faster.Enumerations;
    using FASTER.core;

    public partial class FasterBlockCacheManager
    {
        private BlockSessionCheckpoint lastCheckpoint;
        private BlockSessionAddresses lastAddresses;

        public BlockSessionCheckpoint TakeFullCheckpoint(CheckpointType checkpointType = CheckpointType.Snapshot)
        {
            return this.CheckpointFunc(FasterCheckpointOperation.Full, checkpointType);
        }

        private async Task<BlockSessionCheckpoint> TakeFullCheckpointAsync(CheckpointType checkpointType = CheckpointType.Snapshot)
        {
            return await this.CheckpointFuncAsync(() => new Dictionary<CacheStoreType, Task<(bool, Guid)>>()
            {
                { CacheStoreType.Metadata, this.blockMetadataKV.TakeFullCheckpointAsync(checkpointType: checkpointType).AsTask() },
                { CacheStoreType.Data, this.blockDataKV.TakeFullCheckpointAsync(checkpointType: checkpointType).AsTask() },
                { CacheStoreType.CBL, this.cblSourceHashesKV.TakeFullCheckpointAsync(checkpointType: checkpointType).AsTask() },
                { CacheStoreType.CBLCorrelation, this.cblCorrelationIdsKV.TakeFullCheckpointAsync(checkpointType: checkpointType).AsTask() },
            }).ConfigureAwait(false);
        }

        public BlockSessionCheckpoint TakeHybridCheckpoint(CheckpointType checkpointType)
        {
            return this.CheckpointFunc(FasterCheckpointOperation.Hybrid, checkpointType);
        }

        public async Task<BlockSessionCheckpoint> TakeHybridCheckpointAsync(CheckpointType checkpointType = CheckpointType.Snapshot)
        {
            return await this.CheckpointFuncAsync(() => new Dictionary<CacheStoreType, Task<(bool, Guid)>>()
            {
                { CacheStoreType.Metadata, this.blockMetadataKV.TakeHybridLogCheckpointAsync(checkpointType: checkpointType).AsTask() },
                { CacheStoreType.Data, this.blockDataKV.TakeHybridLogCheckpointAsync(checkpointType: checkpointType).AsTask() },
                { CacheStoreType.CBL, this.cblSourceHashesKV.TakeHybridLogCheckpointAsync(checkpointType: checkpointType).AsTask() },
                { CacheStoreType.CBLCorrelation, this.cblCorrelationIdsKV.TakeHybridLogCheckpointAsync(checkpointType: checkpointType).AsTask() },
            }).ConfigureAwait(false);
        }

        public BlockSessionCheckpoint TakeIndexCheckpoint()
        {
            return this.CheckpointFunc(FasterCheckpointOperation.Index);
        }

        public async Task<BlockSessionCheckpoint> TakeIndexCheckPointAsync()
        {
            return await this.CheckpointFuncAsync(() => new Dictionary<CacheStoreType, Task<(bool, Guid)>>()
            {
                { CacheStoreType.Metadata, this.blockMetadataKV.TakeIndexCheckpointAsync().AsTask() },
                { CacheStoreType.Data, this.blockDataKV.TakeIndexCheckpointAsync().AsTask() },
                { CacheStoreType.CBL, this.cblSourceHashesKV.TakeIndexCheckpointAsync().AsTask() },
                { CacheStoreType.CBLCorrelation, this.cblCorrelationIdsKV.TakeIndexCheckpointAsync().AsTask() },
            }).ConfigureAwait(false);
        }

        public BlockSessionCheckpoint CheckpointFunc(FasterCheckpointOperation operation, CheckpointType checkpointType = CheckpointType.Snapshot)
        {
            bool metadataResult, dataResult, cblResult, cblCorrelationResult;
            Guid metadataToken, dataToken, cblToken, cblCorrelationsToken;
            switch (operation)
            {
                case FasterCheckpointOperation.Full:
                    metadataResult = this.blockMetadataKV.TakeFullCheckpoint(token: out metadataToken, checkpointType: checkpointType);
                    dataResult = this.blockDataKV.TakeFullCheckpoint(token: out dataToken, checkpointType: checkpointType);
                    cblResult = this.cblSourceHashesKV.TakeFullCheckpoint(token: out cblToken, checkpointType: checkpointType);
                    cblCorrelationResult = this.cblCorrelationIdsKV.TakeFullCheckpoint(token: out cblCorrelationsToken, checkpointType: checkpointType);
                    break;
                case FasterCheckpointOperation.Hybrid:
                    metadataResult = this.blockMetadataKV.TakeHybridLogCheckpoint(out metadataToken);
                    dataResult = this.blockDataKV.TakeHybridLogCheckpoint(out dataToken);
                    cblResult = this.cblSourceHashesKV.TakeHybridLogCheckpoint(out cblToken);
                    cblCorrelationResult = this.cblCorrelationIdsKV.TakeHybridLogCheckpoint(out cblCorrelationsToken);
                    break;
                case FasterCheckpointOperation.Index:
                    metadataResult = this.blockMetadataKV.TakeIndexCheckpoint(out metadataToken);
                    dataResult = this.blockDataKV.TakeIndexCheckpoint(out dataToken);
                    cblResult = this.cblSourceHashesKV.TakeIndexCheckpoint(out cblToken);
                    cblCorrelationResult = this.cblCorrelationIdsKV.TakeIndexCheckpoint(out cblCorrelationsToken);
                    break;
                default:
                    throw new BrightChainExceptionImpossible("Unexpected type");
            }

            return new BlockSessionCheckpoint(
                success: metadataResult && dataResult && cblResult && cblCorrelationResult,
                results: new Dictionary<CacheStoreType, bool>()
                    {
                    { CacheStoreType.Metadata, metadataResult },
                    { CacheStoreType.Data, dataResult },
                    { CacheStoreType.CBL, cblResult },
                    { CacheStoreType.CBLCorrelation, cblCorrelationResult },
                    },
                guids: new Dictionary<CacheStoreType, Guid>()
                    {
                    { CacheStoreType.Metadata, metadataToken },
                    { CacheStoreType.Data, dataToken },
                    { CacheStoreType.CBL, cblToken },
                    { CacheStoreType.CBLCorrelation, cblCorrelationsToken },
                    });
        }

        public async Task<BlockSessionCheckpoint> CheckpointFuncAsync(Func<Dictionary<CacheStoreType, Task<(bool, Guid)>>> func)
        {
            var taskDict = func();

            await Task.WhenAll(taskDict.Values)
                .ConfigureAwait(false);

            var resultDict = new Dictionary<CacheStoreType, bool>();
            var guidDict = new Dictionary<CacheStoreType, Guid>();
            var allGood = true;
            foreach (var task in taskDict)
            {
                var result = task.Value.Result;
                resultDict.Add(task.Key, result.Item1);
                guidDict.Add(task.Key, result.Item2);

                allGood = allGood && result.Item1;
            }

            return new BlockSessionCheckpoint(allGood, resultDict, guidDict);
        }

        public async Task CompleteCheckpointAsync()
        {
            await Task
                .WhenAll(new Task[]
                    {
                        this.blockMetadataKV.CompleteCheckpointAsync().AsTask(),
                        this.blockDataKV.CompleteCheckpointAsync().AsTask(),
                        this.cblSourceHashesKV.CompleteCheckpointAsync().AsTask(),
                        this.cblCorrelationIdsKV.CompleteCheckpointAsync().AsTask(),
                    })
                .ConfigureAwait(false);
        }

        public BlockSessionAddresses NextSerials()
        {
            return new BlockSessionAddresses(addresses: new Dictionary<CacheStoreType, long>
            {
                {
                    CacheStoreType.Metadata,
                    this.sessionContext.MetadataSession.NextSerialNo
                },
                {
                    CacheStoreType.Data,
                    this.sessionContext.DataSession.NextSerialNo
                },
                {
                    CacheStoreType.CBL,
                    this.sessionContext.CblSourceHashSession.NextSerialNo
                },
                {
                    CacheStoreType.CBLCorrelation,
                    this.sessionContext.CblCorrelationIdsSession.NextSerialNo
                },
            });
        }

        public BlockSessionAddresses Compact(bool shiftBeginAddress = true)
        {
            return new BlockSessionAddresses(addresses: new Dictionary<CacheStoreType, long>
            {
                {
                    CacheStoreType.Metadata,
                    this.sessionContext.MetadataSession.Compact(
                        untilAddress: this.blockMetadataKV.Log.HeadAddress,
                        shiftBeginAddress: shiftBeginAddress)
                },
                {
                    CacheStoreType.Data,
                    this.sessionContext.DataSession.Compact(
                        untilAddress: this.blockDataKV.Log.HeadAddress,
                        shiftBeginAddress: shiftBeginAddress)
                },
                {
                    CacheStoreType.CBL,
                    this.sessionContext.CblSourceHashSession.Compact(
                        untilAddress: this.cblSourceHashesKV.Log.HeadAddress,
                        shiftBeginAddress: shiftBeginAddress)
                },
                {
                    CacheStoreType.CBLCorrelation,
                    this.sessionContext.CblCorrelationIdsSession.Compact(
                        untilAddress: this.cblCorrelationIdsKV.Log.HeadAddress,
                        shiftBeginAddress: shiftBeginAddress)
                },
            });
        }

        public async void Recover()
        {
            Task.WaitAll(new Task[]
            {
                this.blockMetadataKV.RecoverAsync().AsTask(),
                this.blockDataKV.RecoverAsync().AsTask(),
                this.cblSourceHashesKV.RecoverAsync().AsTask(),
                this.cblCorrelationIdsKV.RecoverAsync().AsTask(),
            });
        }
    }
}
