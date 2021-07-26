using System;
using System.Collections.Generic;
using BrightChain.Engine.Enumerations;

namespace BrightChain.Engine.Models.Nodes
{
    public struct BrightChainNodeInfo
    {
        public readonly List<BlockSize> SupportedReadBlockSizes;
        public readonly List<BlockSize> SupportedWriteBlockSizes;
        public readonly UInt64 SuccessfulValidations;
        public readonly UInt64 MissedValidations;
        public readonly UInt64 RejectedValidations;
        public readonly UInt64 IncorrectValidations;
    }
}
