using BrightChain.Contexts;
using BrightChain.Exceptions;
using BrightChain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrightChain.Extensions
{
    public static class DependencyInjection
    {
        public static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>();
            services.AddScoped<IApplicationDbContext>(provider =>
            {
                var dbContext = provider.GetService<ApplicationDbContext>();
                if (dbContext is null)
                {
                    throw new BrightChainException("could not obtain db context");
                }

                return dbContext;
            });
        }
    }
}
