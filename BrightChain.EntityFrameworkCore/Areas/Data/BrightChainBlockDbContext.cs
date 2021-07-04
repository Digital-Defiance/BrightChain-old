using BrightChain.EntityFrameworkCore.Data;
using BrightChain.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;

public class BrightChainBlockDbContext : DbContext, IBrightChainDbContext
{
    public DbSet<BrightChainBlock> Blocks { get; set; }

    public BrightChainBlockDbContext(DbContextOptions options) : base(options)
    {
    }
}