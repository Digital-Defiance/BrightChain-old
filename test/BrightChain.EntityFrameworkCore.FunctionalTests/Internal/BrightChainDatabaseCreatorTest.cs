// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using BrightChain.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace BrightChain.EntityFrameworkCore.Storage.Internal
{
    public class BrightChainDatabaseCreatorTest
    {
        public static IEnumerable<object[]> IsAsyncData = new[]
        {
            new object[] { true },
            //new object[] { false }
        };

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task EnsureCreated_returns_true_when_database_does_not_exist(bool async)
        {
            await using var testDatabase = BrightChainTestStore.Create("NonExisting");
            try
            {
                using var context = new BloggingContext(testDatabase);
                var creator = context.GetService<IDatabaseCreator>();

                Assert.True(async ? await creator.EnsureCreatedAsync() : creator.EnsureCreated());
            }
            finally
            {
                testDatabase.Initialize(testDatabase.ServiceProvider, () => new BloggingContext(testDatabase));
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task EnsureCreated_returns_true_when_database_exists_but_collections_do_not(bool async)
        {
            await using var testDatabase = BrightChainTestStore.Create("EnsureCreatedTest");
            try
            {
                using var context = new BloggingContext(testDatabase);
                var creator = context.GetService<IDatabaseCreator>();

                Assert.True(async ? await creator.EnsureCreatedAsync() : creator.EnsureCreated());
            }
            finally
            {
                testDatabase.Initialize(testDatabase.ServiceProvider, () => new BloggingContext(testDatabase));
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task EnsureCreated_returns_false_when_database_and_collections_exist(bool async)
        {
            await using var testDatabase = BrightChainTestStore.Create("EnsureCreatedReady");
            testDatabase.Initialize(testDatabase.ServiceProvider, testStore => new BloggingContext((BrightChainTestStore)testStore));

            using var context = new BloggingContext(testDatabase);
            var creator = context.GetService<IDatabaseCreator>();

            Assert.False(async ? await creator.EnsureCreatedAsync() : creator.EnsureCreated());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task EnsureDeleted_returns_true_when_database_exists(bool async)
        {
            await using var testDatabase = BrightChainTestStore.CreateInitialized("EnsureDeleteBlogging");
            using var context = new BloggingContext(testDatabase);
            var creator = context.GetService<IDatabaseCreator>();

            Assert.True(async ? await creator.EnsureDeletedAsync() : creator.EnsureDeleted());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task EnsureDeleted_returns_false_when_database_does_not_exist(bool async)
        {
            await using var testDatabase = BrightChainTestStore.Create("EnsureDeleteBlogging");
            using var context = new BloggingContext(testDatabase);
            var creator = context.GetService<IDatabaseCreator>();

            Assert.False(async ? await creator.EnsureDeletedAsync() : creator.EnsureDeleted());
        }

        private class BloggingContext : DbContext
        {
            private readonly string _connectionUri;
            private readonly string _authToken;
            private readonly string _name;

            public BloggingContext(BrightChainTestStore testStore)
            {
                this._connectionUri = testStore.ConnectionUri;
                this._authToken = testStore.AuthToken;
                this._name = testStore.Name;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder
                    .UseBrightChain(
                        this._connectionUri,
                        this._authToken,
                        this._name,
                        b => b.ApplyConfiguration());
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
            }

            public DbSet<Blog> Blogs { get; set; }
        }

        private class Blog
        {
            public int Id { get; set; }
        }
    }
}
