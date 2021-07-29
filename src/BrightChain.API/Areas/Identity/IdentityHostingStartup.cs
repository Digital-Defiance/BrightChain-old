// <copyright file="IdentityHostingStartup.cs" company="BrightChain">
// Copyright (c) BrightChain. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(BrightChain.API.Areas.Identity.IdentityHostingStartup))]

namespace BrightChain.API.Areas.Identity
{
    using System;
    using Microsoft.Extensions.DependencyInjection;

    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
                {
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
