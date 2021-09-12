namespace BrightChain.Engine.Faster.Functions
{
    using FASTER.core;

    public class BrightChainAdvancedFunctions<Key, Value, Input, Output, Context> : FunctionsBase<Key, Value, Input, Output, Context>
        where Input : Value
        where Output : Input, Value
    {
        public BrightChainAdvancedFunctions(bool locking = false)
            : base(locking: locking)
        {
        }

        public override void ConcurrentReader(ref Key key, ref Input input, ref Value value, ref Output dst)
        {
            dst = (Output)value;
        }

        public override bool ConcurrentWriter(ref Key key, ref Value src, ref Value dst)
        {
            dst = src;
            return true;
        }

        public override void SingleWriter(ref Key key, ref Value src, ref Value dst)
        {
            dst = src;
        }

        public override void InitialUpdater(ref Key key, ref Input input, ref Value value)
        {
            value = input;
        }
    }
}
