// <copyright file="IdentityHostingStartup.cs" company="BrightChain">
// Copyright (c) BrightChain. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(BrightChain.API.Areas.Identity.IdentityHostingStartup))]

namespace BrightChain.API.Areas.Identity
{
    using System;
    using BrightChain.API.Identity.Data;
    using BrightChain.EntityFrameworkCore.Data.Entities;
    using Microsoft.Extensions.DependencyInjection;
    using static BrightChain.EntityFrameworkCore.BrightChainDbContextOptionsExtensions;

    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
                {
                    services.AddDbContext<BrightChainIdentityDbContext>((p, o) =>
                    o.UseBrightChain(Guid.NewGuid().ToString(), "_", "_"));

                    services.AddDefaultIdentity<BrightChainEntityUser>(options => options.SignIn.RequireConfirmedAccount = true)
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
