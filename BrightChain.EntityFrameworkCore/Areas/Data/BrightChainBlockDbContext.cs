using BrightChain.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace BrightChain.EntityFrameworkCore.Data
{

    public class BrightChainBlockDbContext : DbContext, IBrightChainDbContext
    {
        public DbSet<BrightChainBlock> Blocks { get; set; }

        public BrightChainBlockDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                   //.SetBasePath(Directory.GetCurrentDirectory())
                   //.AddJsonFile("appsettings.json")
                   .Build();
                //optionsBuilder.UseBrightChainDatabase(databaseName: Guid.NewGuid().ToString());
            }
        }

        protected override void OnModelCreating(ModelBuilder builder) =>
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            base.OnModelCreating(builder);

        public new async Task<int> SaveChanges() => await base.SaveChangesAsync();
    }
}