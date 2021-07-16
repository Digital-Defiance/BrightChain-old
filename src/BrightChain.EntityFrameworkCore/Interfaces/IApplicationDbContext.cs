using System.Threading;
using System.Threading.Tasks;
using BrightChain.Engine.Models.Blocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BrightChain.EntityFrameworkCore.Interfaces
{
    public interface IApplicationDbContext
    {
        DatabaseFacade Database { get; }
        public DbSet<Block> Blocks { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
