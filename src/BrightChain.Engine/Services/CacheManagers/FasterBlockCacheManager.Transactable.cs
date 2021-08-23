namespace BrightChain.Engine.Services.CacheManagers
{
    using System;

    public partial class FasterBlockCacheManager
    {

        public async Task<IEnumerable<Guid>> CheckpointAsync()
        {
            this.blockMetadataKV.TakeFullCheckpoint(out Guid metadataToken);
            this.blockDataKV.TakeFullCheckpoint(out Guid dataToken);
            this.cblSourceHashesKV.TakeFullCheckpoint(out Guid cblSourceHashToken);
            this.cblCorrelationIdsKV.TakeFullCheckpoint(out Guid cblCorrelationToken);

            var result = new Guid[]
            {
                metadataToken,
                dataToken,
                cblSourceHashToken,
                cblCorrelationToken,
            };

            await this.blockMetadataKV.CompleteCheckpointAsync().ConfigureAwait(false);
            await this.blockDataKV.CompleteCheckpointAsync().ConfigureAwait(false);
            await this.cblSourceHashesKV.CompleteCheckpointAsync().ConfigureAwait(false);
            await this.cblCorrelationIdsKV.CompleteCheckpointAsync().ConfigureAwait(false);

            return result;
        }

        public void Compact(bool shiftBeginAddress = true)
        {
            this.SessionContext.MetadataSession.Compact(
                untilAddress: this.blockMetadataKV.Log.HeadAddress,
                shiftBeginAddress: shiftBeginAddress);
            this.SessionContext.DataSession.Compact(
                untilAddress: this.blockDataKV.Log.HeadAddress,
                shiftBeginAddress: shiftBeginAddress);
            this.SessionContext.CblSourceHashSession.Compact(
                untilAddress: this.cblSourceHashesKV.Log.HeadAddress,
                shiftBeginAddress: shiftBeginAddress);
            this.SessionContext.CblCorrelationIdsSession.Compact(
                untilAddress: this.cblCorrelationIdsKV.Log.HeadAddress,
                shiftBeginAddress: shiftBeginAddress);
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
