using BrightChain.EntityFrameworkCore;

namespace BrightChain.API.Identity.Data
{
    using System;
    using System.Data;
    using System.Reflection.Emit;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using BrightChain.EntityFrameworkCore.Data.Entities;
    using BrightChain.EntityFrameworkCore.Interfaces;
    using BrightChain.EntityFrameworkCore.Metadata.Internal;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;
    using Microsoft.EntityFrameworkCore.Metadata.Internal;
    using Microsoft.Extensions.Configuration;
    using static BrightChainEntityTypeBuilderExtensions;

    public class BrightChainIdentityDbContext : IdentityDbContext<BrightChainEntityUser>, IBrightChainDbContext
    {
        public IDbConnection Connection => Database.GetDbConnection();

        public BrightChainIdentityDbContext(DbContextOptions<BrightChainIdentityDbContext> options) : base(options)
        {
        }

        public new async Task<int> SaveChanges()
        {
            return await base.SaveChangesAsync().ConfigureAwait(false);
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
                //optionsBuilder.UseBrightChain(databaseName: Guid.NewGuid().ToString());

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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            base.OnModelCreating(builder);

            //builder.Entity<BrightChainEntityUser>()
            //    .UseETagConcurrency();

            //builder.Entity<IdentityUser>()
            //    .UseETagConcurrency();

            builder.Entity<BrightChainEntityUser>()
                .Property(c => c.ConcurrencyStamp)
                .IsConcurrencyToken(false);

            builder.Entity<BrightChainEntityUser>()
                .Property<string>("_etag")
                .IsConcurrencyToken();

            builder.Entity<IdentityRole>()
                .ToContainer("Roles")
                .UseETagConcurrency();
        }

        public async Task<BrightChainEntityUser> CreateUserAsync()
        {
            var user = new BrightChainEntityUser();
            // TODO: fill in user details from params
            throw new NotImplementedException();
            this.Database.EnsureCreated();
            this.Users.Add(user);
            await this.SaveChanges().ConfigureAwait(false);
            return user;
        }
    }
}
