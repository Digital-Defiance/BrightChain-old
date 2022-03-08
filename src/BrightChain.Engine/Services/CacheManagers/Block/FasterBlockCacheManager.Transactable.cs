using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Faster.Enumerations;
using FASTER.core;

namespace BrightChain.Engine.Faster.CacheManager;

public partial class FasterBlockCacheManager
{
    private readonly BlockSessionCheckpoint lastCheckpoint;
    private readonly BlockSessionAddresses lastCommit;
    private readonly BlockSessionAddresses lastHead;

    public BlockSessionCheckpoint TakeFullCheckpoint(CheckpointType checkpointType = CheckpointType.Snapshot)
    {
        return this.CheckpointFunc(operation: FasterCheckpointOperation.Full,
            checkpointType: checkpointType);
    }

    private async Task<BlockSessionCheckpoint> TakeFullCheckpointAsync(CheckpointType checkpointType = CheckpointType.Snapshot)
    {
        return await this.CheckpointFuncAsync(func: () => new Dictionary<CacheStoreType, Task<(bool, Guid)>>
        {
            {CacheStoreType.BlockData, this.KV.TakeFullCheckpointAsync(checkpointType: checkpointType).AsTask()},
            {CacheStoreType.Indices, this.cblIndicesKV.TakeFullCheckpointAsync(checkpointType: checkpointType).AsTask()},
        }).ConfigureAwait(continueOnCapturedContext: false);
    }

    public BlockSessionCheckpoint TakeHybridCheckpoint(CheckpointType checkpointType)
    {
        return this.CheckpointFunc(operation: FasterCheckpointOperation.Hybrid,
            checkpointType: checkpointType);
    }

    public async Task<BlockSessionCheckpoint> TakeHybridCheckpointAsync(CheckpointType checkpointType = CheckpointType.Snapshot)
    {
        return await this.CheckpointFuncAsync(func: () => new Dictionary<CacheStoreType, Task<(bool, Guid)>>
        {
            {CacheStoreType.BlockData, this.KV.TakeHybridLogCheckpointAsync(checkpointType: checkpointType).AsTask()},
            {CacheStoreType.Indices, this.cblIndicesKV.TakeHybridLogCheckpointAsync(checkpointType: checkpointType).AsTask()},
        }).ConfigureAwait(continueOnCapturedContext: false);
    }

    public BlockSessionCheckpoint TakeIndexCheckpoint()
    {
        return this.CheckpointFunc(operation: FasterCheckpointOperation.Index);
    }

    public async Task<BlockSessionCheckpoint> TakeIndexCheckPointAsync()
    {
        return await this.CheckpointFuncAsync(func: () => new Dictionary<CacheStoreType, Task<(bool, Guid)>>
        {
            {CacheStoreType.BlockData, this.KV.TakeIndexCheckpointAsync().AsTask()},
            {CacheStoreType.Indices, this.cblIndicesKV.TakeIndexCheckpointAsync().AsTask()},
        }).ConfigureAwait(continueOnCapturedContext: false);
    }

    public BlockSessionCheckpoint CheckpointFunc(FasterCheckpointOperation operation,
        CheckpointType checkpointType = CheckpointType.Snapshot)
    {
        bool dataResult, expirationResult, cblResult, cblIndexResult;
        Guid dataToken, expirationToken, cblToken, cblIndexsToken;
        switch (operation)
        {
            case FasterCheckpointOperation.Full:
                dataResult = this.KV.TakeFullCheckpoint(token: out dataToken,
                    checkpointType: checkpointType);
                cblIndexResult = this.cblIndicesKV.TakeFullCheckpoint(token: out cblIndexsToken,
                    checkpointType: checkpointType);
                break;
            case FasterCheckpointOperation.Hybrid:
                dataResult = this.KV.TakeHybridLogCheckpoint(out dataToken);
                cblIndexResult = this.cblIndicesKV.TakeHybridLogCheckpoint(out cblIndexsToken);
                break;
            case FasterCheckpointOperation.Index:
                dataResult = this.KV.TakeIndexCheckpoint(out dataToken);
                cblIndexResult = this.cblIndicesKV.TakeIndexCheckpoint(out cblIndexsToken);
                break;
            default:
                throw new BrightChainExceptionImpossible(message: "Unexpected type");
        }

        return new BlockSessionCheckpoint(
            success: dataResult && cblIndexResult,
            results: new Dictionary<CacheStoreType, bool>
            {
                {CacheStoreType.BlockData, dataResult}, {CacheStoreType.Indices, cblIndexResult},
            },
            guids: new Dictionary<CacheStoreType, Guid> {{CacheStoreType.BlockData, dataToken}, {CacheStoreType.Indices, cblIndexsToken}});
    }

    public async Task<BlockSessionCheckpoint> CheckpointFuncAsync(Func<Dictionary<CacheStoreType, Task<(bool, Guid)>>> func)
    {
        var taskDict = func();

        await Task.WhenAll(taskDict.Values)
            .ConfigureAwait(continueOnCapturedContext: false);

        var resultDict = new Dictionary<CacheStoreType, bool>();
        var guidDict = new Dictionary<CacheStoreType, Guid>();
        var allGood = true;
        foreach (var task in taskDict)
        {
            var result = task.Value.Result;
            resultDict.Add(key: task.Key,
                value: result.Item1);
            guidDict.Add(key: task.Key,
                value: result.Item2);

            allGood = allGood && result.Item1;
        }

        return new BlockSessionCheckpoint(success: allGood,
            result: resultDict,
            guid: guidDict);
    }

    public async Task CompleteCheckpointAsync()
    {
        await Task
            .WhenAll(this.KV.CompleteCheckpointAsync().AsTask(),
                this.cblIndicesKV.CompleteCheckpointAsync().AsTask())
            .ConfigureAwait(continueOnCapturedContext: false);
    }

    public BlockSessionAddresses NextSerials()
    {
        using var sessionContext = this.NewFasterSessionContext;
        {
            return new BlockSessionAddresses(addresses: new Dictionary<CacheStoreType, long>
            {
                {CacheStoreType.BlockData, sessionContext.BlockDataBlobSession.NextSerialNo},
                {CacheStoreType.Indices, sessionContext.SharedCacheSession.NextSerialNo},
            });
        }
    }

    public BlockSessionAddresses HeadAddresses()
    {
        return new BlockSessionAddresses(addresses: new Dictionary<CacheStoreType, long>
        {
            {CacheStoreType.BlockData, this.KV.Log.HeadAddress}, {CacheStoreType.Indices, this.cblIndicesKV.Log.HeadAddress},
        });
    }

    public BlockSessionAddresses Compact(bool shiftBeginAddress = true)
    {
        using var sessionContext = this.NewFasterSessionContext;
        {
            return new BlockSessionAddresses(addresses: new Dictionary<CacheStoreType, long>
            {
                {
                    CacheStoreType.BlockData, sessionContext.BlockDataBlobSession.Compact(
                        untilAddress: this.KV.Log.HeadAddress,
                        shiftBeginAddress: shiftBeginAddress)
                },
                {
                    CacheStoreType.Indices, sessionContext.SharedCacheSession.Compact(
                        untilAddress: this.cblIndicesKV.Log.HeadAddress,
                        shiftBeginAddress: shiftBeginAddress)
                },
            });
        }
    }

    public async void Recover()
    {
        Task.WaitAll(tasks: new Task[] {this.KV.RecoverAsync().AsTask(), this.cblIndicesKV.RecoverAsync().AsTask()});
    }
}
