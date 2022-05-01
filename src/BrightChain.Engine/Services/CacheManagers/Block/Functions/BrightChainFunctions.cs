using FASTER.core;

namespace BrightChain.Engine.Faster.Functions;

public class BrightChainFunctions<Key, Value, Input, Output, Context> : FunctionsBase<Key, Value, Input, Output, Context>
    where Input : Value
    where Output : Input, Value
{
   
}
