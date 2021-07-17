// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace BrightChain.EntityFrameworkCore.Query
{
    public class QueryLoggingBrightChainTestInsensitive : QueryLoggingBrightChainTestBase, IClassFixture<QueryLoggingBrightChainTestInsensitive.NorthwindQueryBrightChainFixtureInsensitive<NoopModelCustomizer>>
    {
        public QueryLoggingBrightChainTestInsensitive(NorthwindQueryBrightChainFixtureInsensitive<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

        protected override bool ExpectSensitiveData
            => false;

        public class NorthwindQueryBrightChainFixtureInsensitive<TModelCustomizer> : NorthwindQueryBrightChainFixture<TModelCustomizer>
            where TModelCustomizer : IModelCustomizer, new()
        {
            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                return base.AddOptions(builder).EnableSensitiveDataLogging(false);
            }
        }
    }
}
