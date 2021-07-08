using Bogus;
using BrightChain.EntityFrameworkCore.Data;
using BrightChain.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace BrightChain.Tests
{
    [TestClass]
    public class BrightChainEFTest
    {
        [TestMethod]
        public async Task TestBlockDbContextInitializes()
        {
            //create In Memory Database
            var options = new DbContextOptionsBuilder<BrightChainBlockDbContext>()
                .UseBrightChainDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

            using (var context = new BrightChainBlockDbContext(options))
            {
            }

            //var brightChainUser = await context.CreateUserAsync();
        }

        [TestMethod]
        public async Task TestIdentityDbContextInitializes()
        {
            //create In Memory Database
            var options = new DbContextOptionsBuilder<BrightChainIdentityDbContext>()
                .UseBrightChainDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

            using (var context = new BrightChainIdentityDbContext(options))
            {
                context.Database.EnsureCreated();

                var user = new Faker<BrightChainUser>();
                //Use an enum outside scope.

                //Basic rules using built-in generators
                user.RuleFor(u => u.UserName, (f, u) => f.Internet.UserName())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email());
                //Compound property with context, use the first/last name properties
                //.RuleFor(u => u.FullName, (f, u) => u.FirstName + " " + u.LastName)
                //And composability of a complex collection.
                context.Users.Add(user);
                await context.SaveChanges();
            }

        }
    }
}
