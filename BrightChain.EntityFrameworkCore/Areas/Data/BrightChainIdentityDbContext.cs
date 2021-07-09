using BrightChain.EntityFrameworkCore.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Threading.Tasks;

namespace BrightChain.EntityFrameworkCore.Data
{
    public class BrightChainIdentityDbContext : IdentityDbContext<BrightChainUser>, IBrightChainDbContext
    {
        public IDbConnection Connection => this.Database.GetDbConnection();

        public BrightChainIdentityDbContext(DbContextOptions<BrightChainIdentityDbContext> options) : base(options)
        {
        }

        public new async Task<int> SaveChanges() => await base.SaveChangesAsync();

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

                //builder.Entity<Role>().HasData(new List<Role>
                //    {
                //      new Role {
                //        Id = 1,
                //        Name = "Admin",
                //        NormalizedName = "
                //        ADMIN"
                //      },
                //      new Role {
                //        Id = 2,
                //        Name = "Staff",
                //        NormalizedName = "STAFF"
                //      },
                //    });
                //builder.Entity<LogEvent>().HasKey(x => x.LogId);
                //builder.Entity<LogEvent>().ToTable("LogEvents");
                //builder.Entity<Client>().HasKey(x => x.ClientId);
                //builder.Entity<Client>().ToTable("Clients");
                //builder.Entity<LogEventsHistory>().HasKey(x => x.HistoryId);
                //builder.Entity<Flag>().HasKey(x => x.FlagId);
                //builder.Entity<Flag>().ToTable("Flags");
                //builder.Entity<LogRallyHistory>().HasKey(x => x.HistoryId);
                //builder.Entity<LogEventsLineHistory>().HasKey(x => x.LineHistoryId);
            }
        }

        protected override void OnModelCreating(ModelBuilder builder) =>
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            base.OnModelCreating(builder);

        public async Task<BrightChainUser> CreateUserAsync()
        {
            var user = new BrightChainUser();
            // TODO: fill in user details from params
            throw new NotImplementedException();
            this.Database.EnsureCreated();
            this.Users.Add(user);
            await this.SaveChanges();
            return user;
        }
    }
}