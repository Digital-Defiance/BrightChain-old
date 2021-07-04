using BrightChain.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BrightChain.EntityFrameworkCore.Extensions
{
    public static class BrightChainEntityDependencyInjectionExtensions
    {
        private static void AddPersistence<T>(this IServiceCollection services, IConfiguration configuration)
            where T : DbContext
        {
            services.AddEntityFrameworkBrightChainDatabase();
            services.AddDbContext<T>((p, o) =>
                    o.UseBrightChainDatabase(databaseName: Guid.NewGuid().ToString())
                        .UseInternalServiceProvider(p));

            services.AddScoped<T>(provider =>
            {
                var dbContext = provider.GetService<T>();
                if (dbContext is null)
                {
                    throw new Exception("could not obtain db context");
                }

                return dbContext;
            });
        }
    }
}
