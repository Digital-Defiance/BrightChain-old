using System.IO;

namespace BrightChain.EntityFrameworkCore.Client
{
    public interface IBrightChainSerializer<T> where T : class
    {
        public T ReadFrom(Stream stream);
        public void WriteTo(T value, Stream stream);
    }
}