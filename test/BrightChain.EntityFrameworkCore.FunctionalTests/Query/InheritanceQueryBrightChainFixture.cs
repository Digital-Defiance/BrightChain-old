// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BrightChain.EntityFrameworkCore.Query
{
    public class InheritanceQueryBrightChainFixture : InheritanceQueryFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => BrightChainTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)this.ServiceProvider.GetRequiredService<ILoggerFactory>();
    }
}
