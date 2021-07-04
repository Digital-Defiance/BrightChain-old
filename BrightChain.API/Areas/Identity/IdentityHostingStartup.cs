using BrightChain.API.Data;
using BrightChain.EntityFrameworkCore.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(BrightChain.API.Areas.Identity.IdentityHostingStartup))]
namespace BrightChain.API.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder) => builder.ConfigureServices((context, services) =>
        {
            services.AddDbContext<BrightChainAPIUserContext>(options =>
                options.UseSqlServer(
                    context.Configuration.GetConnectionString("BrightChainAPIContextConnection")));

            services.AddDefaultIdentity<BrightChainUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<BrightChainAPIUserContext>();
        });
    }
}