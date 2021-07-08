using BrightChain.EntityFrameworkCore.Data;
using BrightChain.EntityFrameworkCore.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: HostingStartup(typeof(BrightChain.API.Areas.Identity.IdentityHostingStartup))]
namespace BrightChain.API.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder) => builder.ConfigureServices((context, services) =>
        {
            services.AddDbContext<BrightChainIdentityDbContext>((p, o) =>
                    o.UseBrightChainDatabase(databaseName: Guid.NewGuid().ToString()));

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