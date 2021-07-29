namespace BrightChain.API.Extensions
{
    using System;
    using BrightChain.API.Identity.Data;
    using BrightChain.API.Interfaces;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class BrightChainAPIEntityDependencyInjectionExtensions
    {
        public static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddEntityFrameworkBrightChain();
            services.AddDbContext<BrightChainIdentityDbContext>();
            //services.AddDbContext<BrightChainIdentityDbContext>((p, o) =>
            //        o.UseBrightChain(Guid.NewGuid().ToString(), "_", "_")
            //            .UseInternalServiceProvider(p));

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
