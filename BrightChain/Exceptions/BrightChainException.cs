using System;

namespace BrightChain.Exceptions
{
    /**
    <summary>Base class for all BrightChain exceptions</summary>
     */
    public class BrightChainException : Exception
    {
        public BrightChainException(string message) : base(message)
        {
        }
    }
}
