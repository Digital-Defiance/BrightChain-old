namespace BrightChain.API.Interfaces
{
    using System;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Internal;

#pragma warning disable EF1001 // Internal EF Core API usage.
    public interface IBrightChainDbContext
        : IDisposable, IAsyncDisposable, IInfrastructure<IServiceProvider>, IDbContextDependencies, IDbSetCache, IDbContextPoolable, IResettableService
    {
    }
#pragma warning restore EF1001 // Internal EF Core API usage.
}
