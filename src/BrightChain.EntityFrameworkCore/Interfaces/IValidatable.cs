using BrightChain.Exceptions;
using System.Collections.Generic;

namespace BrightChain.Interfaces
{
    public interface IValidatable
    {
        public IEnumerable<BrightChainValidationException> ValidationExceptions { get; }

        public bool Validate();
    }
}
