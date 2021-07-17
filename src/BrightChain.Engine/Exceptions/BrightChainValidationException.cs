namespace BrightChain.Engine.Exceptions
{
    /// <summary>
    /// Base class for all BrightChain exceptions
    /// </summary>
    public class BrightChainValidationException : BrightChainException
    {
        public string Element { get; protected set; }

        public BrightChainValidationException(string element, string message) : base(message)
        {
            Element = element;
        }

        /// <summary>
        /// TODO: Why is this here?
        /// </summary>
        /// <param name="element"></param>
        /// <param name="_"></param>
        public BrightChainValidationException(string element, object _) : base("BOGUS!")
        {
            this.Element = element;
            throw new System.Exception("WHY?");
        }
    }
}
