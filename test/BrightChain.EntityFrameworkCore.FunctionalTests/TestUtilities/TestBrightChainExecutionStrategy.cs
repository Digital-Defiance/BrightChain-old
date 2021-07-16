// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using BrightChain.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BrightChain.EntityFrameworkCore.TestUtilities
{
    public class TestBrightChainExecutionStrategy : BrightChainExecutionStrategy
    {
        protected static new readonly int DefaultMaxRetryCount = 10;

        protected static new readonly TimeSpan DefaultMaxDelay = TimeSpan.FromSeconds(60);

        public TestBrightChainExecutionStrategy()
            : base(
                new DbContext(
                    new DbContextOptionsBuilder()
                        .EnableServiceProviderCaching(false)
                        .UseBrightChain(
                            TestEnvironment.DefaultConnection,
                            TestEnvironment.AuthToken,
                            "NonExistent").Options),
                DefaultMaxRetryCount, DefaultMaxDelay)
        {
        }

        public TestBrightChainExecutionStrategy(ExecutionStrategyDependencies dependencies)
            : base(dependencies, DefaultMaxRetryCount, DefaultMaxDelay)
        {
        }
    }
}
