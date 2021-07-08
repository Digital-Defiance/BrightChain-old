using BrightChain.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BrightChain.EntityFrameworkCore.Data
{

    public class BrightChainBlockDbContext : DbContext, IBrightChainDbContext
    {
        public DbSet<BrightChainBlock> Blocks { get; set; }

        public BrightChainBlockDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}