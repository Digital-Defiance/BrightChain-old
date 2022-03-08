using System.Collections.Generic;
using BrightChain.Engine.Exceptions;

namespace BrightChain.Engine.Interfaces;

public interface IValidatable
{
    public IEnumerable<BrightChainValidationException> ValidationExceptions { get; }

    public bool Validate();
}
