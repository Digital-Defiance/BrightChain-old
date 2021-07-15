using BrightChain.EntityFrameworkCore.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using static BrightChain.EntityFrameworkCore.BrightChainDbContextOptionsExtensions;

[assembly: HostingStartup(typeof(BrightChain.API.Areas.Identity.IdentityHostingStartup))]
namespace BrightChain.API.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
{
    services.AddDbContext<BrightChainIdentityDbContext>((p, o) =>
    o.UseBrightChain(Guid.NewGuid().ToString(), "_", "_"));

    services.AddDefaultIdentity<BrightChainUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<BrightChainIdentityDbContext>();

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
}