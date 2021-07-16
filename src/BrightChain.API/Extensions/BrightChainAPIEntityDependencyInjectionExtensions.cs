using System;
using BrightChain.API.Identity.Data;
using BrightChain.EntityFrameworkCore.Interfaces;
using BrightChain.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static BrightChain.EntityFrameworkCore.BrightChainDbContextOptionsExtensions;

namespace BrightChain.API.Extensions
{
    public static class BrightChainAPIEntityDependencyInjectionExtensions
    {
        public static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEntityFrameworkBrightChain();
            //services.AddDbContext<DbContext>();
            services.AddDbContext<BrightChainIdentityDbContext>((p, o) =>
                    o.UseBrightChain(Guid.NewGuid().ToString(), "_", "_")
                        .UseInternalServiceProvider(p));

            services.AddScoped<IBrightChainDbContext>(provider =>
            {
                var dbContext = provider.GetService<BrightChainIdentityDbContext>();
                if (dbContext is null)
                {
                    throw new Exception("could not obtain db context");
                }

                return dbContext;
            });
        }
    }
}
