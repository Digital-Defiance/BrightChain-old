// <copyright file="IdentityHostingStartup.cs" company="BrightChain">
// Copyright (c) BrightChain. All rights reserved.
// </copyright>

using BrightChain.API.Extensions;
using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(BrightChain.API.Areas.Identity.IdentityHostingStartup))]

namespace BrightChain.API.Areas.Identity
{
    using System;
    using BrightChain.API.Identity.Data;
    using BrightChain.API.Infrastructure;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using static BrightChainAPIEntityDependencyInjectionExtensions;

    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
                {
                    //services.AddPersistence<BrightChainIdentityDbContext>(configuration: context.Configuration);
                    //services.AddDefaultIdentity<BrightChainIdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                    //    .AddEntityFrameworkStores<BrightChainIdentityDbContext>();
                    services.AddIdentity<BrightChainIdentityUser, BrightChainIdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                        .AddEntityFrameworkStores<BrightChainIdentityDbContext>()
                        .AddUserStore<BrightChainUserStore>()
                        .AddRoleStore<BrightChainRoleStore>()
                        .AddRoleManager<BrightChainRoleManager>()
                        .AddDefaultTokenProviders();
                });
        }
    }
}
