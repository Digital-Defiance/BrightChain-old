using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BrightChain.API.Infrastructure
{
    public class DbInitializer : IDbInitializer
    {
        private readonly IdentityDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(
            IdentityDbContext context,
            RoleManager<IdentityRole> roleManager)
        {
            this._context = context;
            this._roleManager = roleManager;
        }

        //This example just creates an Administrator role and one Admin users
        public async Task Initialize()
        {
            //create database schema if none exists
            this._context.Database.EnsureCreated();

            //If there is already an Administrator role, abort
            var adminRoleExists = await this._roleManager.RoleExistsAsync("Admin").ConfigureAwait(false);

            if (!adminRoleExists)
            {
                //Create the Admin Role
                var adminRole = new IdentityRole("Admin");
                var result = await this._roleManager.CreateAsync(adminRole).ConfigureAwait(false);

                if (result.Succeeded)
                {
                    // Add the Trial claim
                    var foreverTrialClaim = new Claim("Trial", DateTime.Now.AddYears(1).ToString());
                    await this._roleManager.AddClaimAsync(adminRole, foreverTrialClaim).ConfigureAwait(false);
                }
            }
        }

    }

    public interface IDbInitializer
    {
        Task Initialize();
    }
}
