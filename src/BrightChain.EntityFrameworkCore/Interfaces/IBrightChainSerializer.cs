namespace BrightChain.EntityFrameworkCore.Interfaces
{
    public interface IBrightChainSerializer<T> : BrightChain.EntityFrameworkCore.Client.IBrightChainSerializer<T> where T : class
    {
    }
}