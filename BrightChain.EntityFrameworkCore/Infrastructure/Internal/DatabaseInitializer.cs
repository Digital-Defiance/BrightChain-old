using BrightChain.EntityFrameworkCore.Data;
using IdentityModel;
using IdentityServer;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Claims;

namespace BrightChain.EntityFrameworkCore.Infrastructure.Internal
{
    public class DatabaseInitializer
    {
        public const string DefaultUser = "brightchain";
        public static void Init(IServiceProvider provider, bool useInMemoryStores)
        {
            if (!useInMemoryStores)
            {
                provider.GetRequiredService<BrightChainIdentityDbContext>().Database.Migrate();
                provider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                provider.GetRequiredService<ConfigurationDbContext>().Database.Migrate();
            }
            InitializeIdentityServer(provider);

            var userManager = provider.GetRequiredService<UserManager<BrightChainUser>>();
            var defaultUser = userManager.FindByNameAsync(DefaultUser).Result;
            if (defaultUser == null)
            {
                defaultUser = new BrightChainUser
                {
                    UserName = DefaultUser
                };
                var result = userManager.CreateAsync(defaultUser, "$AspNetIdentity10$").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                defaultUser = userManager.FindByNameAsync(DefaultUser).Result;

                result = userManager.AddClaimsAsync(defaultUser, new Claim[]{
                    new Claim(JwtClaimTypes.Name, "BrightChain"),
                    new Claim(JwtClaimTypes.GivenName, "BrightChain"),
                    new Claim(JwtClaimTypes.FamilyName, "The Revolution(ary) Network"),
                    new Claim(JwtClaimTypes.Email, "viva@revolution.network"),
                    new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                    new Claim(JwtClaimTypes.WebSite, "https://brightchain.net"),
                    new Claim(JwtClaimTypes.Address, @"{ 'street_address': 'localhost 10', 'postal_code': 123456, 'country': 'United States' }",
                        IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json)
                }).Result;

                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
                Console.WriteLine(String.Format("{0} created", DefaultUser));
            }
            else
            {
                Console.WriteLine(String.Format("{0} already exists", DefaultUser));
            }
        }

        private static void InitializeIdentityServer(IServiceProvider provider)
        {
            var context = provider.GetRequiredService<ConfigurationDbContext>();
            if (!context.Clients.Any())
            {
                foreach (var client in Config.GetClients())
                {
                    context.Clients.Add(client.ToEntity());
                }
                context.SaveChanges();
            }

            if (!context.IdentityResources.Any())
            {
                foreach (var resource in Config.GetIdentityResources())
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }

            if (!context.ApiResources.Any())
            {
                foreach (var resource in Config.GetApis())
                {
                    context.ApiResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
        }
    }
}
