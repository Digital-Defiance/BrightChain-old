using System.Threading.Tasks;
using BrightChain.EntityFrameworkCore.Data.Entities;
using BrightChain.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BrightChain.EntityFrameworkCore.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class BrightChainBlockDbContext : DbContext, IBrightChainDbContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public BrightChainBlockDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<BrightChainEntityBlock> Blocks { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                   //.SetBasePath(Directory.GetCurrentDirectory())
                   //.AddJsonFile("appsettings.json")
                   .Build();
                //optionsBuilder.UseBrightChain(databaseName: Guid.NewGuid().ToString());
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            base.OnModelCreating(builder);
        }

        public new async Task<int> SaveChanges()
        {
            return await base.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
