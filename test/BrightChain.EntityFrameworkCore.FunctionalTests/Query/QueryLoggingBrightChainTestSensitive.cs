// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace BrightChain.EntityFrameworkCore.Query
{
    public class QueryLoggingBrightChainTestSensitive : QueryLoggingBrightChainTestBase, IClassFixture<NorthwindQueryBrightChainFixture<NoopModelCustomizer>>
    {
        public QueryLoggingBrightChainTestSensitive(NorthwindQueryBrightChainFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }
    }
}
