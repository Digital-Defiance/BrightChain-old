using System.IO;

namespace BrightChain.Interfaces
{
    public interface ISerializer<T>
    {
        public T ReadFrom(Stream stream);
        public void WriteTo(T value, Stream stream);
    }
}