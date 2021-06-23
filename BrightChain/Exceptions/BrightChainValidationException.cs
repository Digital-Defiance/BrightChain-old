namespace BrightChain.Exceptions
{
    /// <summary>
    /// Base class for all BrightChain exceptions
    /// </summary>
    public class BrightChainValidationException : BrightChainException
    {
        public BrightChainValidationException(string message) : base(message)
        {
        }
    }
}
