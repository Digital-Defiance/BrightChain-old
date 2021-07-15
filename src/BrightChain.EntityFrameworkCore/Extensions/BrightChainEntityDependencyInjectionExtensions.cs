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
            services.AddEntityFrameworkBrightChain();
            services.AddDbContext<T>((p, o) =>
                    o.UseBrightChain(Guid.NewGuid().ToString(), "_", null)
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
