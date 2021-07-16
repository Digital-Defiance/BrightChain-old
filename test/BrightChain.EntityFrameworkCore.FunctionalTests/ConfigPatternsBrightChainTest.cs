// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using BrightChain.Engine.Client;
using BrightChain.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace BrightChain.EntityFrameworkCore
{
    public class ConfigPatternsBrightChainTest : IClassFixture<ConfigPatternsBrightChainTest.BrightChainFixture>
    {
        private const string DatabaseName = "ConfigPatternsBrightChain";

        protected BrightChainFixture Fixture { get; }

        public ConfigPatternsBrightChainTest(BrightChainFixture fixture)
        {
            Fixture = fixture;
        }

        [ConditionalFact]
        public async Task BrightChain_client_instance_is_shared_between_contexts()
        {
            await using var testDatabase = BrightChainTestStore.CreateInitialized(DatabaseName);
            var options = CreateOptions(testDatabase);

            BrightChainClient client;
            using (var context = new CustomerContext(options))
            {
                client = context.Database.GetBrightChainClient();
                Assert.NotNull(client);
                Assert.True(context.Database.IsBrightChain());
            }

            using (var context = new CustomerContext(options))
            {
                Assert.Same(client, context.Database.GetBrightChainClient());
            }

            await using var testDatabase2 = BrightChainTestStore.CreateInitialized(DatabaseName, null);
            options = CreateOptions(testDatabase2);

            using (var context = new CustomerContext(options))
            {
                Assert.NotSame(client, context.Database.GetBrightChainClient());
            }
        }

        private DbContextOptions CreateOptions(BrightChainTestStore testDatabase)
        {
            return Fixture.AddOptions(testDatabase.AddProviderOptions(new DbContextOptionsBuilder()))
                           .EnableDetailedErrors()
                           .Options;
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class CustomerContext : DbContext
        {
            public CustomerContext(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>();
            }
        }

        public class BrightChainFixture : ServiceProviderFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => BrightChainTestStoreFactory.Instance;
        }
    }
}
