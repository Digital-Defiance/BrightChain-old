using BrightChain.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BrightChain.EntityFrameworkCore.Data
{

    public class BrightChainBlockDbContext : DbContext, IBrightChainDbContext
    {
        public DbSet<BrightChainBlock> Blocks { get; set; }

        public BrightChainBlockDbContext(DbContextOptions options) : base(options)
        {
        }

        public new async Task<int> SaveChanges() => await base.SaveChangesAsync();
    }
}