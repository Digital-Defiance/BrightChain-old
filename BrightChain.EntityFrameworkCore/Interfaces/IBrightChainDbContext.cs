using BrightChain.EntityFrameworkCore.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace BrightChain.EntityFrameworkCore.Interfaces
{
    public interface IBrightChainDbContext
    {
        public Microsoft.EntityFrameworkCore.DbSet<BrightChainUser> Users { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<BrightChainBlock> Blocks { get; set; }
        DatabaseFacade Database { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
