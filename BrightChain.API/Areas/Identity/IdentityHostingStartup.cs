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
            services.AddDbContext<BrightChainBlockDbContext>(options =>
                options.UseSqlServer(
                    context.Configuration.GetConnectionString("BrightChainAPIContextConnection")));

            services.AddDefaultIdentity<BrightChainUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<BrightChainBlockDbContext>();

            //services.AddPersistence<BrightChainBlockDbContext>(configuration: this.Configuration);
            //services.AddPersistence<BrightChainAPIUserContext>(configuration: this.Configuration);
            //services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //    .AddEntityFrameworkStores<ApplicationDbContext>();
            //services.AddIdentity<BrightChainUser, MyRole>()
            //    .AddEntityFrameworkStores<BrightChainAPIUserContext>()
            //    .AddUserStore<MyUserStore>()
            //    .AddRoleStore<MyRoleStore>()
            //    .AddRoleManager<MyRoleManager>()
            //    .AddDefaultTokenProviders();
        });
    }
}