// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using BrightChain.EntityFrameworkCore.Infrastructure;
using BrightChain.EntityFrameworkCore.Storage.Internal;
using BrightChain.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace BrightChain.EntityFrameworkCore
{
    public class BrightChainApiConsistencyTest : ApiConsistencyTestBase<BrightChainApiConsistencyTest.BrightChainApiConsistencyFixture>
    {
        public BrightChainApiConsistencyTest(BrightChainApiConsistencyFixture fixture)
            : base(fixture)
        {
        }

        protected override void AddServices(ServiceCollection serviceCollection)
        {
            serviceCollection.AddEntityFrameworkBrightChain();
        }

        protected override Assembly TargetAssembly
            => typeof(BrightChainDatabaseWrapper).Assembly;

        public class BrightChainApiConsistencyFixture : ApiConsistencyFixtureBase
        {
            public override HashSet<Type> FluentApiTypes { get; } = new()
            {
                typeof(BrightChainModelBuilderExtensions),
                typeof(BrightChainPropertyBuilderExtensions),
                typeof(BrightChainServiceCollectionExtensions),
                typeof(BrightChainDbContextOptionsExtensions),
                typeof(BrightChainDbContextOptionsBuilder)
            };

            public override
                List<(Type Type,
                    Type ReadonlyExtensions,
                    Type MutableExtensions,
                    Type ConventionExtensions,
                    Type ConventionBuilderExtensions,
                    Type RuntimeExtensions)> MetadataExtensionTypes
            { get; }
                = new()
                {
                    (
                        typeof(IReadOnlyModel),
                        typeof(BrightChainModelExtensions),
                        typeof(BrightChainModelExtensions),
                        typeof(BrightChainModelExtensions),
                        typeof(BrightChainModelBuilderExtensions),
                        null
                    ),
                    (
                        typeof(IReadOnlyProperty),
                        typeof(BrightChainPropertyExtensions),
                        typeof(BrightChainPropertyExtensions),
                        typeof(BrightChainPropertyExtensions),
                        typeof(BrightChainPropertyBuilderExtensions),
                        null
                    )
                };
        }
    }
}
