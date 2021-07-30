namespace BrightChain.API.Extensions
{
    using System;
    using BrightChain.API.Areas.Identity;
    using BrightChain.API.Identity.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    public static class BrightChainAPIEntityDependencyInjectionExtensions
    {
        public static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddDbContext<BrightChainIdentityDbContext>();
            //services.AddEntityFrameworkBrightChain();
            services.AddDbContext<BrightChainIdentityDbContext>((p, o) =>
                        o.UseApplicationServiceProvider(p));

            services.AddScoped<DbContext>(provider =>
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
