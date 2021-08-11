namespace BrightChain.Engine.Interfaces
{
    public interface IBrightChainSerializer<T> where T : class
    {
        public T ReadFrom(Stream stream);
        public void WriteTo(T value, Stream stream);
    }
}
