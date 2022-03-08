using System;

namespace BrightChain.Engine.Exceptions;

/// <summary>
///     Base class for all BrightChain exceptions
/// </summary>
public class BrightChainValidationException : BrightChainException
{
    public BrightChainValidationException(string element, string message) : base(message: message)
    {
        this.Element = element;
    }

    /// <summary>
    ///     TODO: Why is this here?
    /// </summary>
    /// <param name="element"></param>
    /// <param name="_"></param>
    public BrightChainValidationException(string element, object _) : base(message: "BOGUS!")
    {
        this.Element = element;
        throw new Exception(message: "WHY?");
    }

    public string Element { get; protected set; }
}
