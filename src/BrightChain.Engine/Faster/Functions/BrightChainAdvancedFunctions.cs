namespace BrightChain.Engine.Faster.Functions
{
    using FASTER.core;

    public class BrightChainAdvancedFunctions<Key, Value, Input, Output, Context> : FunctionsBase<Key, Value, Input, Output, Context>
    {
        public BrightChainAdvancedFunctions(bool locking = false)
            : base(locking: locking)
        {
        }

        /// <inheritdoc/>
        public override void ReadCompletionCallback(ref Key key, ref Input input, ref Output output, Context ctx, Status status)
        {
            if (output is null)
            {
                return;
            }
        }

        /// <inheritdoc/>
        public override void RMWCompletionCallback(ref Key key, ref Input input, Context ctx, Status status)
        {
        }

        /// <inheritdoc/>
        public override void UpsertCompletionCallback(ref Key key, ref Value value, Context ctx)
        {
        }

        /// <inheritdoc/>
        public override void DeleteCompletionCallback(ref Key key, Context ctx)
        {
        }

        /// <inheritdoc/>
        public override void CheckpointCompletionCallback(string sessionId, CommitPoint commitPoint)
        {
        }
    }
}
