using System.Collections.Generic;

namespace BrightChain.Engine.Exceptions
{
    /// <summary>
    /// Base class for all BrightChain exceptions
    /// </summary>
    public class BrightChainValidationEnumerableException : BrightChainException
    {
        public IEnumerable<BrightChainValidationException> Exceptions { get; protected set; }

        public BrightChainValidationEnumerableException(IEnumerable<BrightChainValidationException> exceptions, string message)
            : base(message)
        {
            this.Exceptions = exceptions;
        }
    }
}
