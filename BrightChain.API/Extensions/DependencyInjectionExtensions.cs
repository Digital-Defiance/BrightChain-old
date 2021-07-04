using BrightChain.EntityFrameworkCore.Data;
using BrightChain.EntityFrameworkCore.Extensions;
using BrightChain.EntityFrameworkCore.Interfaces;
using BrightChain.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BrightChain.API.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEntityFrameworkBrightChainDatabase();
            //services.AddDbContext<DbContext>();
            services.AddDbContext<BrightChainBlockDbContext>((p, o) =>
                    o.UseBrightChainDatabase(databaseName: Guid.NewGuid().ToString())
                        .UseInternalServiceProvider(p));

            services.AddScoped<IBrightChainDbContext>(provider =>
            {
                var dbContext = provider.GetService<BrightChainBlockDbContext>();
                if (dbContext is null)
                {
                    throw new Exception("could not obtain db context");
                }

                return dbContext;
            });
        }
    }
}
