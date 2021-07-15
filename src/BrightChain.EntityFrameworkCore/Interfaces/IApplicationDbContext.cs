using BrightChain.Models.Blocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace BrightChain.Interfaces
{
    public interface IApplicationDbContext
    {
        DatabaseFacade Database { get; }
        public DbSet<Block> Blocks { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
