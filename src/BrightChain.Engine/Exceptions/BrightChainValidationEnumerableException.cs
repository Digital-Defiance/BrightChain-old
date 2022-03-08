using System.Collections.Generic;

namespace BrightChain.Engine.Exceptions;

/// <summary>
///     Base class for all BrightChain exceptions
/// </summary>
public class BrightChainValidationEnumerableException : BrightChainException
{
    public BrightChainValidationEnumerableException(IEnumerable<BrightChainValidationException> exceptions, string message)
        : base(message: message)
    {
        this.Exceptions = exceptions;
    }

    public IEnumerable<BrightChainValidationException> Exceptions { get; protected set; }
}
