// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using BrightChain.EntityFrameworkCore.Properties;
using BrightChain.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace BrightChain.EntityFrameworkCore
{
    public class ConnectionSpecificationTest
    {
        [ConditionalFact]
        public async Task Can_specify_connection_string_in_OnConfiguring()
        {
            await using var testDatabase = BrightChainTestStore.Create("NonExisting");
            using var context = new BloggingContext(testDatabase);
            var creator = context.GetService<IDatabaseCreator>();

            Assert.False(await creator.EnsureDeletedAsync());
        }

        public class BloggingContext : DbContext
        {
            private readonly string _connectionString;
            private readonly string _name;

            public BloggingContext(BrightChainTestStore testStore)
            {
                this._connectionString = testStore.ConnectionString;
                this._name = testStore.Name;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseBrightChain(this._connectionString, this._name, b => b.ApplyConfiguration());
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
            }

            public DbSet<Blog> Blogs { get; set; }
        }

        [ConditionalFact]
        public async Task Specifying_connection_string_and_account_endpoint_throws()
        {
            await using var testDatabase = BrightChainTestStore.Create("NonExisting");

            using var context = new BloggingContextWithConnectionConflict(testDatabase);

            Assert.Equal(
                BrightChainStrings.ConnectionStringConflictingConfiguration,
                Assert.Throws<InvalidOperationException>(() => context.GetService<IDatabaseCreator>()).Message);
        }

        public class BloggingContextWithConnectionConflict : DbContext
        {
            private readonly string _connectionString;
            private readonly string _connectionUri;
            private readonly string _authToken;
            private readonly string _name;

            public BloggingContextWithConnectionConflict(BrightChainTestStore testStore)
            {
                this._connectionString = testStore.ConnectionString;
                this._connectionUri = testStore.ConnectionUri;
                this._authToken = testStore.AuthToken;
                this._name = testStore.Name;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseBrightChain(this._connectionString, this._name, b => b.ApplyConfiguration())
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

        public class Blog
        {
            public int Id { get; set; }
        }
    }
}
